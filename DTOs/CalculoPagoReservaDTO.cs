namespace ParkSmart;

public class CalculoPagoReservaDTO
{
    public Guid reservaId { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
    public DateTime fechaReserva { get; set; }
    public DateTime fechaVencimiento { get; set; }
    public int duracionReservadaHoras { get; set; }
    public int horasExcedidas { get; set; }
    public decimal tarifaPorHora { get; set; }
    public decimal montoTarifa { get; set; }
    public decimal multaPorHora { get; set; }
    public decimal montoMulta { get; set; }
    public decimal montoTotal { get; set; }
    public bool tieneMulta { get; set; }
    public bool multaConTope { get; set; }
    public decimal? montoMaximoMulta { get; set; }
}
