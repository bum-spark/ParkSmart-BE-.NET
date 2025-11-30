namespace ParkSmart;

public class NivelDTO
{
public Guid nivelId { get; set; }
    public int numeroPiso { get; set; }
    public int capacidad { get; set; }
    public Guid sedeId { get; set; }
    public string nombreSede { get; set; } = string.Empty;
    public int totalCajones { get; set; }
    public int cajonesLibres { get; set; }
    public int cajonesOcupados { get; set; }
    public List<CajonDTO> cajones { get; set; } = new List<CajonDTO>();
    public NivelDTO(Nivel nivel)
    {
        nivelId = nivel.nivelId;
        numeroPiso = nivel.numeroPiso;
        capacidad = nivel.capacidad;
        sedeId = nivel.sedeId;
        nombreSede = nivel.sede?.nombre ?? "";
        
        if (nivel.cajones != null)
        {
            cajones = nivel.cajones.Select(c => new CajonDTO(c)).ToList();
            totalCajones = cajones.Count;
            cajonesLibres = cajones.Count(c => c.estadoActual == "libre");
            cajonesOcupados = cajones.Count(c => c.estadoActual == "ocupado");
        }
    }
}
