namespace ParkSmart;

public class CajonDTO
{
        public Guid cajonId { get; set; }
        public string numeroCajon { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string estadoActual { get; set; } = string.Empty;
        public Guid nivelId { get; set; }
        public int numeroPiso { get; set; }
        public Guid? ticketActualId { get; set; }

        public CajonDTO() { }

        public CajonDTO(Cajon cajon)
        {
            cajonId = cajon.cajonId;
            numeroCajon = cajon.numeroCajon;
            tipo = cajon.tipo;
            estadoActual = cajon.estadoActual;
            nivelId = cajon.nivelId;
            numeroPiso = cajon.nivel?.numeroPiso ?? 0;
            ticketActualId = cajon.tickets?.FirstOrDefault(t => t.estado == "activo")?.ticketId;
        }
}
