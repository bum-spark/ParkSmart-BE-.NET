namespace ParkSmart;

public class SedeDTO
{
    public Guid sedeId { get; set; }
    public string nombre { get; set; } = string.Empty;
    public string direccion { get; set; } = string.Empty;
    public string estado { get; set; } = string.Empty;
    public decimal tarifaPorHora { get; set; }
    
    public decimal multaPorHora { get; set; }
    public bool multaConTope { get; set; }
    public decimal? montoMaximoMulta { get; set; }
    
    public DateTime fechaCreacion { get; set; }
    public Guid creadoPorUsuarioId { get; set; }
    public string nombreCreador { get; set; } = string.Empty;
    
    public int totalNiveles { get; set; }
    public int totalCajones { get; set; }
    public int cajonesLibres { get; set; }
    public int ticketsActivos { get; set; }
    public int reservasActivas { get; set; }

    public SedeDTO(Sede sede)
    {
        sedeId = sede.sedeId;
        nombre = sede.nombre;
        direccion = sede.direccion;
        estado = sede.estado;
        tarifaPorHora = sede.tarifaPorHora;
        multaPorHora = sede.multaPorHora;
        multaConTope = sede.multaConTope;
        montoMaximoMulta = sede.montoMaximoMulta;
        fechaCreacion = sede.fechaCreacion;
        creadoPorUsuarioId = sede.creadoPorUsuarioId;
        nombreCreador = sede.creadoPor?.nombreCompleto ?? "Desconocido";
        
        var todosLosCajones = sede.niveles?.SelectMany(n => n.cajones).ToList() ?? new List<Cajon>();
        
        totalNiveles = sede.niveles?.Count ?? 0;
        totalCajones = todosLosCajones.Count;
        cajonesLibres = todosLosCajones.Count(c => c.estadoActual == "libre");
        ticketsActivos = todosLosCajones.SelectMany(c => c.tickets ?? new List<Ticket>()).Count(t => t.estado == "activo");
        reservasActivas = todosLosCajones.SelectMany(c => c.reservas ?? new List<Reserva>()).Count(r => r.estado == "pendiente");
    }
}

public class SedeResponseDTO
{
    public Guid sedeId { get; set; }
    public string nombre { get; set; } = string.Empty;
    public string direccion { get; set; } = string.Empty;
    public string estado { get; set; } = string.Empty;
    public decimal tarifaPorHora { get; set; }
    public Guid creadoPorUsuarioId { get; set; }

    public SedeResponseDTO(SedeDTO sede)
    {
        sedeId = sede.sedeId;
        nombre = sede.nombre;
        direccion = sede.direccion;
        estado = sede.estado;
        tarifaPorHora = sede.tarifaPorHora;
        creadoPorUsuarioId = sede.creadoPorUsuarioId;
    }
}
