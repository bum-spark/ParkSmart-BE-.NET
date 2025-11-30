namespace ParkSmart;

public class ReporteIngresosSede
{
    public Guid sedeId { get; set; }
    public string nombreSede { get; set; } = string.Empty;
    public DateTime fechaInicio { get; set; }
    public DateTime fechaFin { get; set; }
    public int totalTickets { get; set; }
    public int totalReservas { get; set; }
    public decimal ingresosPorTickets { get; set; }
    public decimal ingresosPorReservas { get; set; }
    public decimal multasCobradas { get; set; }
    public decimal ingresoTotal { get; set; }
    public List<ReporteNivel> detallesPorNivel { get; set; } = new List<ReporteNivel>();
}
