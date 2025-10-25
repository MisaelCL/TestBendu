using C_C.App.Model;
using Microsoft.EntityFrameworkCore;

namespace C_C.App.Repositories;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserModel> Users => Set<UserModel>();
    public DbSet<PerfilModel> Perfiles => Set<PerfilModel>();
    public DbSet<PreferenciasModel> Preferencias => Set<PreferenciasModel>();
    public DbSet<MatchModel> Matches => Set<MatchModel>();
    public DbSet<ChatModel> Chats => Set<ChatModel>();
    public DbSet<MensajeModel> Mensajes => Set<MensajeModel>();
    public DbSet<ReporteModel> Reportes => Set<ReporteModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserModel>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<UserModel>()
            .HasOne(u => u.Perfil)
            .WithOne(p => p.User)
            .HasForeignKey<PerfilModel>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserModel>()
            .HasOne(u => u.Preferencias)
            .WithOne(p => p.User)
            .HasForeignKey<PreferenciasModel>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MatchModel>()
            .HasOne(m => m.UserA)
            .WithMany(u => u.MatchesInitiated)
            .HasForeignKey(m => m.UserIdA)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchModel>()
            .HasOne(m => m.UserB)
            .WithMany(u => u.MatchesReceived)
            .HasForeignKey(m => m.UserIdB)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MatchModel>()
            .HasIndex(m => new { m.UserIdA, m.UserIdB })
            .IsUnique();

        modelBuilder.Entity<MatchModel>()
            .HasOne(m => m.Chat)
            .WithOne(c => c.Match)
            .HasForeignKey<ChatModel>(c => c.MatchId);

        modelBuilder.Entity<ChatModel>()
            .HasMany(c => c.Mensajes)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ReporteModel>()
            .HasOne(r => r.Reportante)
            .WithMany(u => u.ReportesRealizados)
            .HasForeignKey(r => r.ReportanteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReporteModel>()
            .HasOne(r => r.Reportado)
            .WithMany(u => u.ReportesRecibidos)
            .HasForeignKey(r => r.ReportadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
