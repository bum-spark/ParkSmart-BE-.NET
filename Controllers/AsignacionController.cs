using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ParkSmart;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AsignacionController : ControllerBase
    {
        private readonly IAsignacionCajonService _asignacionService;
        private readonly IAccesoSedeService _accesoSedeService;

        public AsignacionController(
            IAsignacionCajonService asignacionService,
            IAccesoSedeService accesoSedeService)
        {
            _asignacionService = asignacionService;
            _accesoSedeService = accesoSedeService;
        }

        private IActionResult? VerificarAccesoUsuario(Guid sedeId)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                return Unauthorized(new { error = true, msg = "No se pudo obtener el ID del usuario del token" });
            }

            var usuarioId = Guid.Parse(userIdClaim.Value);

            if (!_accesoSedeService.TieneAcceso(usuarioId, sedeId))
            {
                return Unauthorized(new
                {
                    error = true,
                    msg = "No tienes acceso a esta sede. Por favor inrgesa la contraseña de la sede"
                });
            }

            return null; 
        }

        [HttpPost("cajon-especifico")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> AsignarCajonEspecifico([FromBody] AsignarCajonEspecificoDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.placaVehiculo))
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "La placa del vehículo es requerida"
                });
            }

            var asignacion = await _asignacionService.AsignarCajonEspecifico(dto);

            if (asignacion == null)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "No se pudo asignar el cajón. Verifique que el cajón exista, pertenezca a la sede, esté disponible y el vehículo no tenga un ticket activo."
                });
            }

            return Ok(new
            {
                error = false,
                msg = "Cajón asignado exitosamente",
                data = asignacion
            });
        }

        [HttpPost("cajon-automatico")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> AsignarCajonAutomatico([FromBody] AsignarCajonAutomaticoDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.placaVehiculo))
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "La placa del vehículo es requerida"
                });
            }

            var asignacion = await _asignacionService.AsignarCajonAutomatico(dto);

            if (asignacion == null)
            {
                return NotFound(new
                {
                    error = true,
                    msg = "No hay espacios disponibles en la sede o el vehículo ya tiene un ticket activo"
                });
            }

            return Ok(new
            {
                error = false,
                msg = "Cajón asignado automáticamente",
                data = asignacion
            });
        }

        [HttpPost("reserva")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> CrearReserva([FromBody] CrearReservaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.placaVehiculo))
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "La placa del vehículo es requerida"
                });
            }

            if (dto.duracionHoras <= 0)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "La duración de la reserva debe ser mayor a 0 horas"
                });
            }

            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)
                           ?? User.FindFirst("sub");

            if (userIdClaim == null)
            {
                return Unauthorized(new { error = true, msg = "No se pudo obtener el ID del usuario del token" });
            }

            var usuarioId = Guid.Parse(userIdClaim.Value);

            var reserva = await _asignacionService.CrearReserva(dto, usuarioId);

            if (reserva == null)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "No se pudo crear la reserva. Verifique que el cajón exista, pertenezca a la sede, no esté ocupado y no haya conflictos de horario."
                });
            }

            return Ok(new
            {
                error = false,
                msg = "Reserva creada exitosamente. La fecha de inicio fue establecida automáticamente.",
                data = reserva
            });
        }

        [HttpGet("reservas/sede/{sedeId}")]
        public async Task<IActionResult> ObtenerReservasDeSede(Guid sedeId)
        {
            var errorAcceso = VerificarAccesoUsuario(sedeId);
            if (errorAcceso != null) return Unauthorized(new
                {
                    error = true,
                    msg = "No tienes acceso a esta sede. Por favor inrgesa la contraseña de la sede"
                });;

            var reservas = await _asignacionService.ObtenerReservasDeSede(sedeId);

            return Ok(new
            {
                error = false,
                msg = "Reservas obtenidas exitosamente",
                data = reservas
            });
        }

        [HttpGet("reserva/{reservaId}")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> ObtenerReservaPorId(Guid reservaId)
        {
            var reserva = await _asignacionService.ObtenerReservaPorId(reservaId);

            if (reserva == null)
            {
                return NotFound(new
                {
                    error = true,
                    msg = "Reserva no encontrada"
                });
            }

            return Ok(new
            {
                error = false,
                msg = "Reserva obtenida exitosamente",
                data = reserva
            });
        }

        [HttpGet("reserva/{reservaId}/calcular-pago")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> CalcularPagoReserva(Guid reservaId)
        {
            var calculo = await _asignacionService.CalcularPagoReserva(reservaId);

            if (calculo == null)
            {
                return NotFound(new
                {
                    error = true,
                    msg = "Reserva no encontrada o ya fue pagada/cancelada"
                });
            }

            return Ok(new
            {
                error = false,
                msg = calculo.tieneMulta 
                    ? $"Cálculo realizado. ATENCIÓN: Se aplicó multa de {calculo.montoMulta:C} por {calculo.horasExcedidas} hora(s) de exceso."
                    : "Cálculo realizado sin multas",
                data = calculo
            });
        }

        [HttpPost("reserva/{reservaId}/pagar")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> PagarReserva(Guid reservaId)
        {
            var calculo = await _asignacionService.CalcularPagoReserva(reservaId);

            if (calculo == null)
            {
                return NotFound(new
                {
                    error = true,
                    msg = "Reserva no encontrada o ya fue pagada/cancelada"
                });
            }

            var exito = await _asignacionService.PagarReserva(reservaId);

            if (!exito)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "No se pudo procesar el pago"
                });
            }

            return Ok(new
            {
                error = false,
                msg = "Pago procesado exitosamente. El cajón ha sido liberado.",
                montoPagado = calculo.montoTotal,
                detalles = calculo
            });
        }

        [HttpGet("ticket/{ticketId}/calcular-pago")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> CalcularPagoTicket(Guid ticketId)
        {
            var calculo = await _asignacionService.CalcularPagoTicket(ticketId);

            if (calculo == null)
            {
                return NotFound(new
                {
                    error = true,
                    msg = "Ticket no encontrado o ya fue pagado"
                });
            }

            return Ok(new
            {
                error = false,
                msg = $"Tiempo estacionado: {calculo.horasCobradas} hora(s). Monto a pagar: {calculo.montoTotal:C}",
                data = calculo
            });
        }

        [HttpPost("ticket/{ticketId}/pagar")]
        [Authorize(Roles = "Admin,Gerente,Empleado")]
        public async Task<IActionResult> PagarTicket(Guid ticketId)
        {
            var calculo = await _asignacionService.CalcularPagoTicket(ticketId);

            if (calculo == null)
            {
                return NotFound(new
                {
                    error = true,
                    msg = "Ticket no encontrado o ya fue pagado"
                });
            }

            var exito = await _asignacionService.PagarTicket(ticketId);

            if (!exito)
            {
                return BadRequest(new
                {
                    error = true,
                    msg = "No se pudo procesar el pago"
                });
            }

            return Ok(new
            {
                error = false,
                msg = "Pago procesado exitosamente. El cajón ha sido liberado.",
                montoPagado = calculo.montoTotal,
                detalles = calculo
            });
        }

        [HttpGet("historial/sede/{sedeId}")]
        public async Task<IActionResult> ObtenerHistorialCompleto(Guid sedeId)
        {
            var errorAcceso = VerificarAccesoUsuario(sedeId);
            if (errorAcceso != null) return Unauthorized(new
                {
                    error = true,
                    msg = "No tienes acceso a esta sede. Por favor inrgesa la contraseña de la sede"
                });;

            var historial = await _asignacionService.ObtenerHistorialCompleto(sedeId);

            var totalTickets = historial.Count(h => h.tipo == "ticket");
            var totalReservas = historial.Count(h => h.tipo == "reserva");

            return Ok(new
            {
                error = false,
                msg = "Historial completo obtenido exitosamente",
                totalRegistros = historial.Count,
                totalTickets = totalTickets,
                totalReservas = totalReservas,
                data = historial
            });
        }

        [HttpPatch("cajon/cambiar-tipo")]
        [Authorize(Roles = "Admin,Gerente")]
        public async Task<IActionResult> CambiarTipoCajon([FromBody] CambiarTipoCajonDTO dto)
        {
            // Verificar acceso a la sede
            var errorAcceso = VerificarAccesoUsuario(dto.sedeId);
            if (errorAcceso != null) return Unauthorized(new
            {
                error = true,
                msg = "No tienes acceso a esta sede. Por favor ingresa la contraseña de la sede"
            });

            // Validar que el tipo sea uno de los permitidos
            var tiposPermitidos = Enum.GetNames(typeof(TipoCajon)).Select(t => t.ToLower()).ToList();
            if (!tiposPermitidos.Contains(dto.nuevoTipo.ToLower()))
            {
                return BadRequest(new
                {
                    error = true,
                    msg = $"Tipo de cajón no válido. Los tipos permitidos son: {string.Join(", ", tiposPermitidos)}"
                });
            }

            var resultado = await _asignacionService.CambiarTipoCajon(dto);

            if (!resultado.exito)
            {
                return resultado.motivo switch
                {
                    "cajon_no_encontrado" => NotFound(new
                    {
                        error = true,
                        msg = "No se encontró el cajón o no pertenece a la sede especificada"
                    }),
                    "cajon_ocupado" => BadRequest(new
                    {
                        error = true,
                        msg = "No se puede cambiar el tipo del cajón porque tiene un ticket activo (está ocupado)"
                    }),
                    "cajon_reservado" => BadRequest(new
                    {
                        error = true,
                        msg = "No se puede cambiar el tipo del cajón porque tiene una reserva pendiente"
                    }),
                    "tipo_invalido" => BadRequest(new
                    {
                        error = true,
                        msg = $"Tipo de cajón no válido. Los tipos permitidos son: {string.Join(", ", tiposPermitidos)}"
                    }),
                    _ => BadRequest(new
                    {
                        error = true,
                        msg = "Error al cambiar el tipo del cajón"
                    })
                };
            }

            return Ok(new
            {
                error = false,
                msg = $"Tipo de cajón actualizado exitosamente a '{resultado.cajon!.tipo}'",
                data = resultado.cajon
            });
        }
    }
}
