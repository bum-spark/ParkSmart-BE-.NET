using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkSmart;

public class Nivel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid nivelId { get; set; }

    [Required]
    public int numeroPiso { get; set; } // Este podra ser negatico, para pisos subterraneos

    [Required]
    public int capacidad { get; set; }

    [ForeignKey("sede")]
    public Guid sedeId { get; set; }

    public Sede sede { get; set; } = null!;
    public ICollection<Cajon> cajones { get; set; } = new List<Cajon>();
}
