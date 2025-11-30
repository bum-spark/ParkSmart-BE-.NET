using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkSmart;

public class Ticket
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid ticketId { get; set; }

    [Required]
    [MaxLength(20)]
    public string placaVehiculo { get; set; } = string.Empty;

    [Required]
    public DateTime horaEntrada { get; set; } = FechaHelper.AhoraLocal();

    public DateTime? horaSalida { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? montoTotal { get; set; }

    [Required]
    [MaxLength(20)]
    public string estado { get; set; } = "activo"; // activo, completado, cancelado

    [ForeignKey("cajon")]
    public Guid cajonId { get; set; }

    public Cajon cajon { get; set; } = null!;
}
