namespace ParkSmart;

public class CrearSedeDTO
{
    public string nombre { get; set; } = string.Empty;
    public string direccion { get; set; } = string.Empty;
    public string passwordAcceso { get; set; } = string.Empty;
    public decimal tarifaPorHora { get; set; }
    
    public decimal multaPorHora { get; set; } = 0.00m;
    public bool multaConTope { get; set; } = false;
    public decimal? montoMaximoMulta { get; set; } = null;
    public List<ConfiguracionNivelDTO> niveles { get; set; } = new List<ConfiguracionNivelDTO>();
}
