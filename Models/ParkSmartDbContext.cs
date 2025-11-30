using Microsoft.EntityFrameworkCore;
namespace ParkSmart;

public class ParkSmartDbContext : DbContext
{
    public ParkSmartDbContext(DbContextOptions<ParkSmartDbContext> options) : base(options) {}

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Sede> Sedes { get; set; }
    public DbSet<Nivel> Niveles { get; set; }
    public DbSet<Cajon> Cajones { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Reserva> Reservas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de relaciones y restricciones adicionales
        // Usuario - Sede (1:N)
        modelBuilder.Entity<Sede>()
            .HasOne(s => s.creadoPor)
            .WithMany(u => u.sedesCreadas)
            .HasForeignKey(s => s.creadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar usuario si tiene sedes

        // Sede - Nivel (1:N)
        modelBuilder.Entity<Nivel>()
            .HasOne(n => n.sede)
            .WithMany(s => s.niveles)
            .HasForeignKey(n => n.sedeId)
            .OnDelete(DeleteBehavior.Cascade); // Al eliminar sede, eliminar niveles

        // Nivel - Cajon (1:N)
        modelBuilder.Entity<Cajon>()
            .HasOne(c => c.nivel)
            .WithMany(n => n.cajones)
            .HasForeignKey(c => c.nivelId)
            .OnDelete(DeleteBehavior.Cascade); // Al eliminar nivel, eliminar espacios

        // Cajon - Ticket (1:N)
        modelBuilder.Entity<Ticket>()
            .HasOne(t => t.cajon)
            .WithMany(c => c.tickets)
            .HasForeignKey(t => t.cajonId)
            .OnDelete(DeleteBehavior.Restrict); // No permitir eliminar espacio si tiene tickets activos

        // Cajon - Reserva (1:N)
        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.cajon)
            .WithMany(c => c.reservas)
            .HasForeignKey(r => r.cajonId)
            .OnDelete(DeleteBehavior.Restrict);

        // Usuario - Reserva (1:N)
        modelBuilder.Entity<Reserva>()
            .HasOne(r => r.creadoPor)
            .WithMany(u => u.reservas)
            .HasForeignKey(r => r.creadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices únicos para mejorar rendimiento y evitar duplicados
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.email)
            .IsUnique();

        modelBuilder.Entity<Cajon>()
            .HasIndex(c => new { c.nivelId, c.numeroCajon })
            .IsUnique(); // No puede haber dos espacios con el mismo número en el mismo nivel
    }
}
