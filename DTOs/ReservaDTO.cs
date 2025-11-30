namespace ParkSmart;

public class ReservaDTO
{
public Guid reservaId { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
    public DateTime fechaReserva { get; set; }
    public int duracionEstimadaHoras { get; set; }
    public string estado { get; set; } = string.Empty;
    public Guid cajonId { get; set; }
    public string numeroCajon { get; set; } = string.Empty;
    public string tipoCajon { get; set; } = string.Empty;
    public int numeroPiso { get; set; }
    public Guid creadoPorUsuarioId { get; set; }
    public string nombreCreador { get; set; } = string.Empty;
    public DateTime fechaVencimiento { get; set; }
    public ReservaDTO(Reserva reserva)
    {
        reservaId = reserva.reservaId;
        placaVehiculo = reserva.placaVehiculo;
        fechaReserva = reserva.fechaReserva;
        duracionEstimadaHoras = reserva.duracionEstimadaHoras;
        estado = reserva.estado;
        cajonId = reserva.cajonId;
        numeroCajon = reserva.cajon?.numeroCajon ?? "N/A";
        tipoCajon = reserva.cajon?.tipo ?? "N/A";
        numeroPiso = reserva.cajon?.nivel?.numeroPiso ?? 0;
        creadoPorUsuarioId = reserva.creadoPorUsuarioId;
        nombreCreador = reserva.creadoPor?.nombreCompleto ?? "N/A";
        fechaVencimiento = reserva.fechaReserva.AddHours(reserva.duracionEstimadaHoras);
    }
}
