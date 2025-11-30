namespace ParkSmart;

public interface IAsignacionCajonService
{
        Task<AsignacionCajonDTO?> AsignarCajonEspecifico(AsignarCajonEspecificoDTO dto);
        Task<AsignacionCajonDTO?> AsignarCajonAutomatico(AsignarCajonAutomaticoDTO dto);
        Task<ReservaDTO?> CrearReserva(CrearReservaDTO dto, Guid usuarioId);
        Task<List<ReservaDTO>> ObtenerReservasDeSede(Guid sedeId);
        Task<ReservaDTO?> ObtenerReservaPorId(Guid reservaId);
        Task<CalculoPagoReservaDTO?> CalcularPagoReserva(Guid reservaId);
        Task<bool> PagarReserva(Guid reservaId);
        Task<CalculoPagoTicketDTO?> CalcularPagoTicket(Guid ticketId);
        Task<bool> PagarTicket(Guid ticketId);
        Task<List<HistorialTransaccionDTO>> ObtenerHistorialCompleto(Guid sedeId);
        Task<ResultadoCambioTipoCajonDTO> CambiarTipoCajon(CambiarTipoCajonDTO dto);
}
