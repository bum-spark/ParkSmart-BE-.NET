using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace ParkSmart;

public class SedeService : ISedeService
{
private readonly ParkSmartDbContext _context;

    public SedeService(ParkSmartDbContext context)
    {
        _context = context;
    }

    public async Task<List<SedeDTO>> ObtenerTodasLasSedes()
    {
        var sedes = await _context.Sedes
            .Include(s => s.creadoPor)
            .Include(s => s.niveles)
                .ThenInclude(n => n.cajones)
                    .ThenInclude(c => c.tickets)
            .Include(s => s.niveles)
                .ThenInclude(n => n.cajones)
                    .ThenInclude(c => c.reservas)
            .ToListAsync();
        return sedes.Select(s => new SedeDTO(s)).ToList();
    }

    public async Task<SedeDTO> ObtenerSedePorId(Guid sedeId)
    {
        var sede = await _context.Sedes
            .Include(s => s.creadoPor)
            .Include(s => s.niveles)
                .ThenInclude(n => n.cajones)
                    .ThenInclude(c => c.tickets)
            .Include(s => s.niveles)
                .ThenInclude(n => n.cajones)
                    .ThenInclude(c => c.reservas)
            .FirstOrDefaultAsync(s => s.sedeId == sedeId);
        return sede == null ? null : new SedeDTO(sede);
    }
    public async Task<SedeDTO> CrearSede(CrearSedeDTO crearSede, Guid usuarioId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var nuevaSede = new Sede
            {
                sedeId = Guid.NewGuid(),
                nombre = crearSede.nombre,
                direccion = crearSede.direccion,
                passwordAcceso = HashPassword(crearSede.passwordAcceso),
                tarifaPorHora = crearSede.tarifaPorHora,
                multaPorHora = crearSede.multaPorHora,
                multaConTope = crearSede.multaConTope,
                montoMaximoMulta = crearSede.montoMaximoMulta,
                estado = "activo",
                fechaCreacion = FechaHelper.AhoraLocal(),
                creadoPorUsuarioId = usuarioId
            };
            _context.Sedes.Add(nuevaSede);
            await _context.SaveChangesAsync();

            foreach (var configNivel in crearSede.niveles)
            {
                var nuevoNivel = new Nivel
                {
                    nivelId = Guid.NewGuid(),
                    numeroPiso = configNivel.numeroPiso,
                    capacidad = configNivel.capacidad,
                    sedeId = nuevaSede.sedeId
                };
                _context.Niveles.Add(nuevoNivel);
                await _context.SaveChangesAsync();
                for (int i = 1; i <= configNivel.capacidad; i++)
                {
                    var numeroCajon = $"N{configNivel.numeroPiso}-{i}";
                    
                    var nuevoCajon = new Cajon
                    {
                        cajonId = Guid.NewGuid(),
                        numeroCajon = numeroCajon,
                        tipo = "normal", 
                        estadoActual = "libre",
                        nivelId = nuevoNivel.nivelId
                    };
                    _context.Cajones.Add(nuevoCajon);
                }
                
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            await _context.Entry(nuevaSede)
                .Reference(s => s.creadoPor)
                .LoadAsync();

            await _context.Entry(nuevaSede)
                .Collection(s => s.niveles)
                .LoadAsync();

            foreach (var nivel in nuevaSede.niveles)
            {
                await _context.Entry(nivel)
                    .Collection(n => n.cajones)
                    .LoadAsync();
            }

            return new SedeDTO(nuevaSede);
        }catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> VerificarAccesoSede(Guid sedeId, string password)
    {
        var sede = await _context.Sedes.FindAsync(sedeId);
        if (sede == null) return false;
        return VerificarPassword(password, sede.passwordAcceso);
    }

    public async Task<bool> ActualizarEstadoSede(Guid sedeId, string nuevoEstado, string contraseñaCreador)
    {
        if (!Enum.TryParse<EstadoSede>(nuevoEstado, ignoreCase: true, out _)) return false;
        var sede = await _context.Sedes
            .Include(s => s.creadoPor)
            .FirstOrDefaultAsync(s => s.sedeId == sedeId);
        if (sede == null) return false;
        if (!VerificarPassword(contraseñaCreador, sede.creadoPor.passwordHash))
        {
            return false;
        }
        sede.estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<object> ObtenerEstadisticasSede(Guid sedeId)
    {
        var sede = await _context.Sedes
            .Include(s => s.niveles)
                .ThenInclude(n => n.cajones)
                    .ThenInclude(c => c.tickets)
            .Include(s => s.niveles)
                .ThenInclude(n => n.cajones)
                    .ThenInclude(c => c.reservas)
            .FirstOrDefaultAsync(s => s.sedeId == sedeId);

        if (sede == null) return null!;

        var todosLosCajones = sede.niveles.SelectMany(n => n.cajones).ToList();
        var totalCajones = todosLosCajones.Count();

        var cajonesConTicketActivo = todosLosCajones
            .Count(c => c.tickets.Any(t => t.estado == "activo"));

        var cajonesConReservaPendiente = todosLosCajones
            .Count(c => c.reservas.Any(r => r.estado == "pendiente") && 
                       !c.tickets.Any(t => t.estado == "activo"));

        var cajonesOcupados = cajonesConTicketActivo + cajonesConReservaPendiente;
        var cajonesLibres = totalCajones - cajonesOcupados;

        var hoy = FechaHelper.AhoraLocal().Date;

        var ingresosTickets = todosLosCajones
            .SelectMany(c => c.tickets)
            .Where(t => t.horaSalida.HasValue && 
                       t.horaSalida.Value.Date == hoy &&
                       t.estado == "pagado")
            .Sum(t => t.montoTotal ?? 0);

        var ingresosReservas = todosLosCajones
            .SelectMany(c => c.reservas)
            .Where(r => r.fechaReserva.Date == hoy && r.estado == "completado")
            .Sum(r => sede.tarifaPorHora * r.duracionEstimadaHoras);

        var ingresosDia = ingresosTickets + ingresosReservas;

        return new
        {
            sedeId = sede.sedeId,
            nombreSede = sede.nombre,
            totalCajones,
            cajonesOcupados,
            cajonesLibres,
            cajonesConTicketActivo,
            cajonesConReservaPendiente,
            porcentajeOcupacion = totalCajones > 0 ? Math.Round((cajonesOcupados * 100.0 / totalCajones), 2) : 0,
            ingresosDia,
            ingresosTickets,
            ingresosReservas,
            tarifaPorHora = sede.tarifaPorHora
        };
    }

    public async Task<SedeDTO?> ActualizarSedeCompleta(Guid sedeId, ActualizarSedeCompletaDTO dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
            
        try
        {
            var sede = await _context.Sedes
                .Include(s => s.creadoPor)
                .Include(s => s.niveles)
                    .ThenInclude(n => n.cajones)
                        .ThenInclude(c => c.tickets)
                .Include(s => s.niveles)
                    .ThenInclude(n => n.cajones)
                        .ThenInclude(c => c.reservas)
                .FirstOrDefaultAsync(s => s.sedeId == sedeId);

            if (sede == null) return null;

            if (!VerificarPassword(dto.contraseñaCreador, sede.creadoPor.passwordHash))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(dto.nombre))
                sede.nombre = dto.nombre;
            
            if (!string.IsNullOrEmpty(dto.direccion))
                sede.direccion = dto.direccion;
            
            if (!string.IsNullOrEmpty(dto.passwordAcceso))
                sede.passwordAcceso = HashPassword(dto.passwordAcceso);
            
            if (dto.tarifaPorHora.HasValue)
                sede.tarifaPorHora = dto.tarifaPorHora.Value;
            
            if (dto.multaPorHora.HasValue)
                sede.multaPorHora = dto.multaPorHora.Value;
            
            if (dto.multaConTope.HasValue)
                sede.multaConTope = dto.multaConTope.Value;
            
            if (dto.montoMaximoMulta.HasValue)
                sede.montoMaximoMulta = dto.montoMaximoMulta;
            
            if (!string.IsNullOrEmpty(dto.estado))
                sede.estado = dto.estado;

            await _context.SaveChangesAsync();

            //si hay niveles se actualizaran
            if (dto.niveles != null && dto.niveles.Any())
            {
                var todosLosCajones = sede.niveles.SelectMany(n => n.cajones).ToList();
                
                foreach (var cajon in todosLosCajones)
                {
                    var tieneTicketActivo = cajon.tickets.Any(t => t.estado == "activo");
                    var tieneReservaPendiente = cajon.reservas.Any(r => r.estado == "pendiente");

                    if (tieneTicketActivo || tieneReservaPendiente)
                    {
                        await transaction.RollbackAsync();
                        throw new InvalidOperationException(
                            $"No se puede actualizar la estructura de cajones porque el cajón '{cajon.numeroCajon}' tiene tickets activos o reservas pendientes."
                        );
                    }

                    // Marcar cajón como inactivo (desactivar)
                    cajon.estadoActual = "inactivo";
                }

                await _context.SaveChangesAsync();

                _context.Niveles.RemoveRange(sede.niveles);
                await _context.SaveChangesAsync();

                //cajones automáticos basados en capacidad
                foreach (var nivelDTO in dto.niveles)
                {
                    var nuevoNivel = new Nivel
                    {
                        nivelId = Guid.NewGuid(),
                        numeroPiso = nivelDTO.numeroPiso,
                        capacidad = nivelDTO.capacidad,
                        sedeId = sede.sedeId
                    };

                    _context.Niveles.Add(nuevoNivel);
                    await _context.SaveChangesAsync();

                    for (int i = 1; i <= nivelDTO.capacidad; i++)
                    {
                        var numeroCajon = $"N{nivelDTO.numeroPiso}-{i}";
                        
                        var nuevoCajon = new Cajon
                        {
                            cajonId = Guid.NewGuid(),
                            numeroCajon = numeroCajon,
                            tipo = "normal", 
                            estadoActual = "libre",
                            nivelId = nuevoNivel.nivelId
                        };

                        _context.Cajones.Add(nuevoCajon);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            await transaction.CommitAsync();

            //actualizar la sede con los datos nuevos
            await _context.Entry(sede)
                .Collection(s => s.niveles)
                .LoadAsync();

            foreach (var nivel in sede.niveles)
            {
                await _context.Entry(nivel)
                    .Collection(n => n.cajones)
                    .LoadAsync();
            }

            return new SedeDTO(sede);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    public async Task<List<NivelDTO>> ObtenerNivelesDeSede(Guid sedeId)
    {
        var niveles = await _context.Niveles
            .Include(n => n.sede)
            .Include(n => n.cajones)
                .ThenInclude(c => c.tickets)
            .Where(n => n.sedeId == sedeId)
            .OrderBy(n => n.numeroPiso)
            .ToListAsync();
        return niveles.Select(n => new NivelDTO(n)).ToList();
    }

    public async Task<List<CajonDTO>> ObtenerCajonesPorNivel(Guid sedeId, Guid nivelId)
    {
        
        var nivel = await _context.Niveles
            .FirstOrDefaultAsync(n => n.nivelId == nivelId && n.sedeId == sedeId);

        if (nivel == null) return null;

        var cajones = await _context.Cajones
            .Include(c => c.nivel)
            .Where(c => c.nivelId == nivelId)
            .OrderBy(c => c.numeroCajon)
            .ToListAsync();

        return cajones.Select(c => new CajonDTO(c)).ToList();
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    private bool VerificarPassword(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
    
}
