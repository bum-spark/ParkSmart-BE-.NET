using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkSmart;

public class Reserva
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid reservaId { get; set; }

    [Required]
    [MaxLength(20)]
    public string placaVehiculo { get; set; } = string.Empty;

    [Required]
    public DateTime fechaReserva { get; set; } = FechaHelper.AhoraLocal();

    [Required]
    public int duracionEstimadaHoras { get; set; } = 1;

    [Required]
    [MaxLength(20)]
    public string estado { get; set; } = "pendiente"; // pendiente, completado, cancelado

    [ForeignKey("cajon")]
    public Guid cajonId { get; set; }

    [ForeignKey("creadoPor")]
    public Guid creadoPorUsuarioId { get; set; }

    public Cajon cajon { get; set; } = null!;
    public Usuario creadoPor { get; set; } = null!;
}
