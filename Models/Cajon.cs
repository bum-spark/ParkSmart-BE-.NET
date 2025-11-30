using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkSmart;

public class Cajon
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid cajonId { get; set; }

    [Required]
    [MaxLength(10)]
    public string numeroCajon { get; set; } = string.Empty; // Ejemplo: "A-01", "B-15"

    [Required]
    [MaxLength(20)]
    public string tipo { get; set; } = "normal"; // normal, discapacitado, electrico

    [Required]
    [MaxLength(20)]
    public string estadoActual { get; set; } = "libre"; // libre, ocupado, reservado

    [ForeignKey("nivel")]
    public Guid nivelId { get; set; }

    public Nivel nivel { get; set; } = null!;
    public ICollection<Ticket> tickets { get; set; } = new List<Ticket>();
    public ICollection<Reserva> reservas { get; set; } = new List<Reserva>();
}
