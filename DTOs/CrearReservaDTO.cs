namespace ParkSmart;

public class CrearReservaDTO
{
    public Guid sedeId { get; set; }
    public Guid cajonId { get; set; }
    public string placaVehiculo { get; set; } = string.Empty;
    public int duracionHoras { get; set; }
}
