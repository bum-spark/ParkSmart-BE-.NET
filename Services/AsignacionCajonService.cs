using Microsoft.EntityFrameworkCore;

namespace ParkSmart;

public class AsignacionCajonService : IAsignacionCajonService
{
    private readonly ParkSmartDbContext _context;

    public AsignacionCajonService(ParkSmartDbContext context)
    {
        _context = context;
    }

    public async Task<AsignacionCajonDTO?> AsignarCajonEspecifico(AsignarCajonEspecificoDTO dto)
    {
        //verificar que el cajon este es esa sede
        var cajon = await _context.Cajones
            .Include(c => c.nivel)
            .ThenInclude(n => n.sede)
            .FirstOrDefaultAsync(c => c.cajonId == dto.cajonId && c.nivel.sedeId == dto.sedeId);

        if (cajon == null)return null; 
        if (cajon.estadoActual != "libre")return null;

        //se verifica que no haya un ticket activo
        var ticketActivo = await _context.Tickets
            .Include(t => t.cajon)
            .ThenInclude(c => c.nivel)
            .Where(t => t.placaVehiculo == dto.placaVehiculo 
                     && t.estado == "activo" 
                     && t.cajon.nivel.sedeId == dto.sedeId)
            .FirstOrDefaultAsync();

            if (ticketActivo != null) return null;

            var ticket = new Ticket
            {
                placaVehiculo = dto.placaVehiculo,
                horaEntrada = FechaHelper.AhoraLocal(),
                estado = "activo",
                cajonId = dto.cajonId
            };

            _context.Tickets.Add(ticket);
            cajon.estadoActual = "ocupado";

            await _context.SaveChangesAsync();

            return new AsignacionCajonDTO
            {
                ticketId = ticket.ticketId,
                cajonId = cajon.cajonId,
                numeroCajon = cajon.numeroCajon,
                tipoCajon = cajon.tipo,
                numeroPiso = cajon.nivel.numeroPiso,
                placaVehiculo = ticket.placaVehiculo,
                horaEntrada = ticket.horaEntrada
            };
        }

        public async Task<AsignacionCajonDTO?> AsignarCajonAutomatico(AsignarCajonAutomaticoDTO dto)
        {
            var ticketActivo = await _context.Tickets
                .Include(t => t.cajon)
                .ThenInclude(c => c.nivel)
                .Where(t => t.placaVehiculo == dto.placaVehiculo 
                         && t.estado == "activo" 
                         && t.cajon.nivel.sedeId == dto.sedeId)
                .FirstOrDefaultAsync();

            if (ticketActivo != null) return null;

            //Buscar un nivel especifico
            var nivelEspecifico = await _context.Niveles
                .Include(n => n.cajones)
                .FirstOrDefaultAsync(n => n.sedeId == dto.sedeId && n.numeroPiso == dto.numeroPiso);

            Cajon cajonSeleccionado = null;

            if (nivelEspecifico != null) 
            {
                cajonSeleccionado = BuscarCajonConPrioridad(nivelEspecifico.cajones.ToList());
            }

            if (cajonSeleccionado == null)
            {
                var todosLosNiveles = await _context.Niveles
                    .Include(n => n.cajones)
                    .Where(n => n.sedeId == dto.sedeId && n.numeroPiso != dto.numeroPiso)
                    .OrderBy(n => n.numeroPiso)
                    .ToListAsync();

                foreach (var nivel in todosLosNiveles)
                {
                    cajonSeleccionado = BuscarCajonConPrioridad(nivel.cajones.ToList());
                    if (cajonSeleccionado != null)
                    {
                        break;
                    }
                }
            }

            if (cajonSeleccionado == null) return null;

            var ticket = new Ticket
            {
                placaVehiculo = dto.placaVehiculo,
                horaEntrada = FechaHelper.AhoraLocal(),
                estado = "activo",
                cajonId = cajonSeleccionado.cajonId
            };

            _context.Tickets.Add(ticket);

            cajonSeleccionado.estadoActual = "ocupado";

            await _context.SaveChangesAsync();

            var nivelCompleto = await _context.Niveles
                .FirstOrDefaultAsync(n => n.nivelId == cajonSeleccionado.nivelId);

            return new AsignacionCajonDTO
            {
                ticketId = ticket.ticketId,
                cajonId = cajonSeleccionado.cajonId,
                numeroCajon = cajonSeleccionado.numeroCajon,
                tipoCajon = cajonSeleccionado.tipo,
                numeroPiso = nivelCompleto?.numeroPiso ?? 0,
                placaVehiculo = ticket.placaVehiculo,
                horaEntrada = ticket.horaEntrada
            };
        }

        /// Buscar un cajon por priodidad, normal (baja), electrico (media), discapacitado (alta)
        private Cajon BuscarCajonConPrioridad(List<Cajon> cajones)
        {
            var cajonesLibres = cajones.Where(c => c.estadoActual == "libre").ToList();

            if (!cajonesLibres.Any()) return null;

            // prioridad 1: normal
            var cajonNormal = cajonesLibres.FirstOrDefault(c => c.tipo == "normal");
            if (cajonNormal != null) return cajonNormal;

            // prioridad 2: electrico
            var cajonElectrico = cajonesLibres.FirstOrDefault(c => c.tipo == "electrico");
            if (cajonElectrico != null) return cajonElectrico;

            // prioridad 3: Discapacitado
            var cajonDiscapacitado = cajonesLibres.FirstOrDefault(c => c.tipo == "discapacitado");
            return cajonDiscapacitado;
        }

        public async Task<ReservaDTO> CrearReserva(CrearReservaDTO dto, Guid usuarioId)
        {
            //verificar que el cajón existe y pertenece a la sede
            var cajon = await _context.Cajones
                .Include(c => c.nivel)
                .ThenInclude(n => n.sede)
                .Include(c => c.reservas)
                .FirstOrDefaultAsync(c => c.cajonId == dto.cajonId && c.nivel.sedeId == dto.sedeId);

            if (cajon == null) return null;
            if (cajon.estadoActual == "ocupado") return null;

            var fechaActual = FechaHelper.AhoraLocal();
            var fechaFin = fechaActual.AddHours(dto.duracionHoras);
            
            var reservaConflicto = await _context.Reservas
                .Where(r => r.cajonId == dto.cajonId 
                         && r.estado == "pendiente"
                         && r.fechaReserva < fechaFin)
                .FirstOrDefaultAsync();

            if (reservaConflicto != null) return null;

            // Crear la reserva con fecha actual del sistema
            var reserva = new Reserva
            {
                placaVehiculo = dto.placaVehiculo,
                fechaReserva = fechaActual,
                duracionEstimadaHoras = dto.duracionHoras,
                estado = "pendiente",
                cajonId = dto.cajonId,
                creadoPorUsuarioId = usuarioId
            };

            _context.Reservas.Add(reserva);
            cajon.estadoActual = "reservado";

            await _context.SaveChangesAsync();

            var reservaCompleta = await _context.Reservas
                .Include(r => r.cajon)
                .ThenInclude(c => c.nivel)
                .Include(r => r.creadoPor)
                .FirstOrDefaultAsync(r => r.reservaId == reserva.reservaId);

            return reservaCompleta != null ? new ReservaDTO(reservaCompleta) : null;
        }

        public async Task<List<ReservaDTO>> ObtenerReservasDeSede(Guid sedeId)
        {
            var reservas = await _context.Reservas
                .Include(r => r.cajon)
                .ThenInclude(c => c.nivel)
                .Include(r => r.creadoPor)
                .Where(r => r.cajon.nivel.sedeId == sedeId)
                .OrderByDescending(r => r.fechaReserva)
                .ToListAsync();

            return reservas.Select(r => new ReservaDTO(r)).ToList();
        }

        public async Task<ReservaDTO> ObtenerReservaPorId(Guid reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.cajon)
                .ThenInclude(c => c.nivel)
                .Include(r => r.creadoPor)
                .FirstOrDefaultAsync(r => r.reservaId == reservaId);

            return reserva != null ? new ReservaDTO(reserva) : null;
        }

        public async Task<CalculoPagoReservaDTO> CalcularPagoReserva(Guid reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.cajon)
                .ThenInclude(c => c.nivel)
                .ThenInclude(n => n.sede)
                .FirstOrDefaultAsync(r => r.reservaId == reservaId);

            if (reserva == null || reserva.estado != "pendiente") return null;

            var sede = reserva.cajon.nivel.sede;
            var fechaPago = FechaHelper.AhoraLocal();
            var fechaVencimiento = reserva.fechaReserva.AddHours(reserva.duracionEstimadaHoras);

            var montoTarifa = sede.tarifaPorHora * reserva.duracionEstimadaHoras;

            var horasExcedidas = 0;
            var montoMulta = 0m;
            var tieneMulta = false;

            if (fechaPago > fechaVencimiento)
            {
                tieneMulta = true;
                var tiempoExcedido = fechaPago - fechaVencimiento;
                horasExcedidas = (int) Math.Ceiling(tiempoExcedido.TotalHours);

                montoMulta = sede.multaPorHora * horasExcedidas;

                if (sede.multaConTope && sede.montoMaximoMulta.HasValue)
                {
                    montoMulta = Math.Min(montoMulta, sede.montoMaximoMulta.Value);
                }
            }

            var montoTotal = montoTarifa + montoMulta;

            return new CalculoPagoReservaDTO
            {
                reservaId = reserva.reservaId,
                placaVehiculo = reserva.placaVehiculo,
                fechaReserva = reserva.fechaReserva,
                fechaVencimiento = fechaVencimiento,
                duracionReservadaHoras = reserva.duracionEstimadaHoras,
                horasExcedidas = horasExcedidas,
                tarifaPorHora = sede.tarifaPorHora,
                montoTarifa = montoTarifa,
                multaPorHora = sede.multaPorHora,
                montoMulta = montoMulta,
                montoTotal = montoTotal,
                tieneMulta = tieneMulta,
                multaConTope = sede.multaConTope,
                montoMaximoMulta = sede.montoMaximoMulta
            };
        }

        public async Task<bool> PagarReserva(Guid reservaId)
        {
            var reserva = await _context.Reservas
                .Include(r => r.cajon)
                .FirstOrDefaultAsync(r => r.reservaId == reservaId);

            if (reserva == null || reserva.estado != "pendiente") return false;
            
            reserva.estado = "completado";

            // liberar cajon
            var otraReservaPendiente = await _context.Reservas
                .AnyAsync(r => r.cajonId == reserva.cajonId 
                            && r.reservaId != reservaId 
                            && r.estado == "pendiente");

            if (!otraReservaPendiente)
            {
                reserva.cajon.estadoActual = "libre";
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CalculoPagoTicketDTO?> CalcularPagoTicket(Guid ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.cajon)
                .ThenInclude(c => c.nivel)
                .ThenInclude(n => n.sede)
                .FirstOrDefaultAsync(t => t.ticketId == ticketId);

            if (ticket == null || ticket.estado != "activo") return null;

            var sede = ticket.cajon.nivel.sede;
            var horaSalida = FechaHelper.AhoraLocal();
            var tiempoTranscurrido = horaSalida - ticket.horaEntrada;
            var minutosTranscurridos = (int)Math.Ceiling(tiempoTranscurrido.TotalMinutes);
            var horasCobradas = (int) Math.Ceiling(tiempoTranscurrido.TotalHours);

            if (horasCobradas < 1) horasCobradas = 1;

            var montoTotal = sede.tarifaPorHora * horasCobradas;

            return new CalculoPagoTicketDTO
            {
                ticketId = ticket.ticketId,
                placaVehiculo = ticket.placaVehiculo,
                horaEntrada = ticket.horaEntrada,
                horaSalida = horaSalida,
                minutosTranscurridos = minutosTranscurridos,
                horasCobradas = horasCobradas,
                tarifaPorHora = sede.tarifaPorHora,
                montoTotal = montoTotal,
                numeroCajon = ticket.cajon.numeroCajon,
                numeroPiso = ticket.cajon.nivel.numeroPiso
            };
        }

        public async Task<bool> PagarTicket(Guid ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.cajon)
                .FirstOrDefaultAsync(t => t.ticketId == ticketId);

            if (ticket == null || ticket.estado != "activo") return false;

            var calculo = await CalcularPagoTicket(ticketId);
            if (calculo == null) return false;

            ticket.horaSalida = FechaHelper.AhoraLocal();
            ticket.montoTotal = calculo.montoTotal;
            ticket.estado = "pagado";

            ticket.cajon.estadoActual = "libre";

            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<HistorialTransaccionDTO>> ObtenerHistorialCompleto(Guid sedeId)
        {
            var sede = await _context.Sedes.FindAsync(sedeId);
            if (sede == null)
            {
                return new List<HistorialTransaccionDTO>();
            }

            var tickets = await _context.Tickets
                .Include(t => t.cajon)
                .ThenInclude(c => c.nivel)
                .Where(t => t.cajon.nivel.sedeId == sedeId)
                .ToListAsync();

            var reservas = await _context.Reservas
                .Include(r => r.cajon)
                .ThenInclude(c => c.nivel)
                .Include(r => r.creadoPor)
                .Where(r => r.cajon.nivel.sedeId == sedeId)
                .ToListAsync();

            // convertir tickets a HistorialTransaccionDTO
            var historialTickets = tickets.Select(t => new HistorialTransaccionDTO(t)).ToList();

            // convertir reservas a HistorialTransaccionDTO
            var historialReservas = reservas.Select(r => new HistorialTransaccionDTO(r, sede.tarifaPorHora)).ToList();

            // combinar listas por fecha
            var historialCompleto = historialTickets
                .Concat(historialReservas)
                .OrderByDescending(h => h.fechaInicio)
                .ToList();

            return historialCompleto;
        }

        public async Task<ResultadoCambioTipoCajonDTO> CambiarTipoCajon(CambiarTipoCajonDTO dto)
        {
            // Validar que el tipo sea válido usando el enum
            if (!Enum.TryParse<TipoCajon>(dto.nuevoTipo, ignoreCase: true, out var tipoValido))
            {
                return new ResultadoCambioTipoCajonDTO
                {
                    exito = false,
                    motivo = "tipo_invalido"
                };
            }

            // Buscar el cajón y verificar que pertenezca a la sede
            var cajon = await _context.Cajones
                .Include(c => c.nivel)
                .Include(c => c.tickets)
                .Include(c => c.reservas)
                .FirstOrDefaultAsync(c => c.cajonId == dto.cajonId && c.nivel.sedeId == dto.sedeId);

            if (cajon == null)
            {
                return new ResultadoCambioTipoCajonDTO
                {
                    exito = false,
                    motivo = "cajon_no_encontrado"
                };
            }

            // Verificar que no tenga ticket activo
            var tieneTicketActivo = cajon.tickets.Any(t => t.estado == "activo");
            if (tieneTicketActivo)
            {
                return new ResultadoCambioTipoCajonDTO
                {
                    exito = false,
                    motivo = "cajon_ocupado"
                };
            }

            // Verificar que no tenga reserva pendiente
            var tieneReservaPendiente = cajon.reservas.Any(r => r.estado == "pendiente");
            if (tieneReservaPendiente)
            {
                return new ResultadoCambioTipoCajonDTO
                {
                    exito = false,
                    motivo = "cajon_reservado"
                };
            }

            // Actualizar el tipo del cajón
            cajon.tipo = tipoValido.ToString();
            await _context.SaveChangesAsync();

            return new ResultadoCambioTipoCajonDTO
            {
                exito = true,
                cajon = new CajonDTO(cajon)
            };
        }
}
