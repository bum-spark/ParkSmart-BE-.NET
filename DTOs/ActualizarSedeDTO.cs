namespace ParkSmart;

public class ActualizarSedeDTO
{
    public string nombre { get; set; } = string.Empty;
    public string direccion { get; set; } = string.Empty;
    public string? passwordAcceso { get; set; } = null;
    public decimal tarifaPorHora { get; set; }
    public decimal multaPorHora { get; set; }
    public bool multaConTope { get; set; }
    public decimal? montoMaximoMulta { get; set; }
    public string estado { get; set; } = string.Empty;
}

public class ActualizarSedeCompletaDTO
{
    public string contraseñaCreador { get; set; } = string.Empty;
    public string? nombre { get; set; } = null;
    public string? direccion { get; set; } = null;
    public string? passwordAcceso { get; set; } = null;
    public decimal? tarifaPorHora { get; set; } = null;
    public decimal? multaPorHora { get; set; } = null;
    public bool? multaConTope { get; set; } = null;
    public decimal? montoMaximoMulta { get; set; } = null;
    public string? estado { get; set; } = null;
    
    public List<ActualizarNivelCapacidadDTO>? niveles { get; set; } = null;
}

public class ActualizarNivelCapacidadDTO
{
    public int numeroPiso { get; set; }
    public int capacidad { get; set; }
}
