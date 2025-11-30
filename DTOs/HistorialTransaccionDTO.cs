namespace ParkSmart;

public class HistorialTransaccionDTO
{
    public Guid transaccionId { get; set; }
    public string tipo { get; set; } = string.Empty; // para ticket o reserva
    public string placaVehiculo { get; set; } = string.Empty;
    public DateTime fechaInicio { get; set; }
    public DateTime? fechaFin { get; set; }
    public int duracionHoras { get; set; }
    public decimal? montoTotal { get; set; }
    public string estado { get; set; } = string.Empty; // activo, completado, cancelado, pendiente
    public Guid cajonId { get; set; }
    public string numeroCajon { get; set; } = string.Empty;
    public string tipoCajon { get; set; } = string.Empty;
    public int numeroPiso { get; set; }
    public string? nombreCreador { get; set; } //este atributo solo lo tiene reservas

    public HistorialTransaccionDTO(Ticket ticket)
    {
        transaccionId = ticket.ticketId;
        tipo = "ticket";
        placaVehiculo = ticket.placaVehiculo;
        fechaInicio = ticket.horaEntrada;
        fechaFin = ticket.horaSalida;
        duracionHoras = ticket.horaSalida.HasValue 
            ? (int)Math.Ceiling((ticket.horaSalida.Value - ticket.horaEntrada).TotalHours) 
            : 0;
        montoTotal = ticket.montoTotal;
        estado = ticket.estado;
        cajonId = ticket.cajonId;
        numeroCajon = ticket.cajon?.numeroCajon ?? "N/A";
        tipoCajon = ticket.cajon?.tipo ?? "N/A";
        numeroPiso = ticket.cajon?.nivel?.numeroPiso ?? 0;
        nombreCreador = null; // los tickets no tienen creador
    }

    public HistorialTransaccionDTO(Reserva reserva, decimal tarifaPorHora)
    {
        transaccionId = reserva.reservaId;
        tipo = "reserva";
        placaVehiculo = reserva.placaVehiculo;
        fechaInicio = reserva.fechaReserva;
        fechaFin = reserva.fechaReserva.AddHours(reserva.duracionEstimadaHoras);
        duracionHoras = reserva.duracionEstimadaHoras;
        montoTotal = reserva.estado == "completado" 
            ? tarifaPorHora * reserva.duracionEstimadaHoras 
            : null;
        estado = reserva.estado;
        cajonId = reserva.cajonId;
        numeroCajon = reserva.cajon?.numeroCajon ?? "N/A";
        tipoCajon = reserva.cajon?.tipo ?? "N/A";
        numeroPiso = reserva.cajon?.nivel?.numeroPiso ?? 0;
        nombreCreador = reserva.creadoPor?.nombreCompleto;
    }
}
    

