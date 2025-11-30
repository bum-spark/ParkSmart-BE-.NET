namespace ParkSmart;

public class CambiarTipoCajonDTO
{
    public Guid sedeId { get; set; }
    public Guid cajonId { get; set; }
    public string nuevoTipo { get; set; } = string.Empty;
}

public class ResultadoCambioTipoCajonDTO
{
    public bool exito { get; set; }
    public string? motivo { get; set; }
    public CajonDTO? cajon { get; set; }
}
