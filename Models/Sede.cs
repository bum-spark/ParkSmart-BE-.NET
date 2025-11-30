using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkSmart;

public class Sede
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid sedeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string nombre { get; set; } = string.Empty;

    [Required]
    public string direccion { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string estado { get; set; } = "activo"; // activo, mantenimiento, inactivo

    [Required]
    [MaxLength(255)]
    public string passwordAcceso { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal tarifaPorHora { get; set; } = 0.00m;

    [Column(TypeName = "decimal(10,2)")]
    public decimal multaPorHora { get; set; } = 0.00m;

    public bool multaConTope { get; set; } = false;

    [Column(TypeName = "decimal(10,2)")]
    public decimal? montoMaximoMulta { get; set; } = null;

    public DateTime fechaCreacion { get; set; } = FechaHelper.AhoraLocal();

    [ForeignKey("creadoPor")]
    public Guid creadoPorUsuarioId { get; set; }

    public Usuario creadoPor { get; set; } = null!;
    public ICollection<Nivel> niveles { get; set; } = new List<Nivel>();
}
