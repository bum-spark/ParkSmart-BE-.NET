using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkSmart;

public class Usuario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid usuarioId { get; set; }

    [Required]
    [MaxLength(100)]
    public string nombreCompleto { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string passwordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string rol { get; set; } = "Operador"; // Admin, Gerente, Operador

    public DateTime fechaCreacion { get; set; } = FechaHelper.AhoraLocal();

    public ICollection<Sede> sedesCreadas { get; set; } = new List<Sede>();
    public ICollection<Reserva> reservas { get; set; } = new List<Reserva>();
}
