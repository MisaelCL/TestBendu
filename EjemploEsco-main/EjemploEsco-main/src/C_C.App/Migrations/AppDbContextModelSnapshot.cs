using System;
using C_C.App.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace C_C.App.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.5");

            modelBuilder.Entity("C_C.App.Model.ChatModel", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property<Guid>("MatchId");
                b.Property<Guid>("UserIdA");
                b.Property<Guid>("UserIdB");
                b.HasKey("Id");
                b.HasIndex("MatchId").IsUnique();
                b.ToTable("Chats");
            });

            modelBuilder.Entity("C_C.App.Model.MatchModel", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property<Guid?>("ChatId");
                b.Property<DateTime>("CreatedAtUtc");
                b.Property<bool>("IsMutual");
                b.Property<Guid>("UserIdA");
                b.Property<Guid>("UserIdB");
                b.HasKey("Id");
                b.HasIndex("UserIdA", "UserIdB").IsUnique();
                b.ToTable("Matches");
            });

            modelBuilder.Entity("C_C.App.Model.MensajeModel", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property<Guid>("ChatId");
                b.Property<string>("Contenido").IsRequired().HasMaxLength(1024);
                b.Property<DateTime>("EnviadoEnUtc");
                b.Property<bool>("Leido");
                b.Property<Guid>("RemitenteId");
                b.HasKey("Id");
                b.HasIndex("ChatId");
                b.ToTable("Mensajes");
            });

            modelBuilder.Entity("C_C.App.Model.PerfilModel", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property<string>("Bio").IsRequired().HasMaxLength(512);
                b.Property<string>("Ciudad").IsRequired().HasMaxLength(128);
                b.Property<string>("FotoUrl").IsRequired().HasMaxLength(256);
                b.Property<string>("Intereses").IsRequired();
                b.Property<Guid>("UserId");
                b.HasKey("Id");
                b.HasIndex("UserId").IsUnique();
                b.ToTable("Perfiles");
            });

            modelBuilder.Entity("C_C.App.Model.PreferenciasModel", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property<double>("DistanciaMaximaKm");
                b.Property<int>("EdadMaxima");
                b.Property<int>("EdadMinima");
                b.Property<string>("GeneroBuscado").IsRequired().HasMaxLength(64);
                b.Property<Guid>("UserId");
                b.HasKey("Id");
                b.HasIndex("UserId").IsUnique();
                b.ToTable("Preferencias");
            });

            modelBuilder.Entity("C_C.App.Model.ReporteModel", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property<DateTime>("CreadoEnUtc");
                b.Property<string>("Motivo").IsRequired().HasMaxLength(512);
                b.Property<Guid>("ReportadoId");
                b.Property<Guid>("ReportanteId");
                b.HasKey("Id");
                b.HasIndex("ReportadoId");
                b.HasIndex("ReportanteId");
                b.ToTable("Reportes");
            });

            modelBuilder.Entity("C_C.App.Model.UserModel", b =>
            {
                b.Property<Guid>("Id").ValueGeneratedOnAdd();
                b.Property<DateTime>("CreatedAtUtc");
                b.Property<DateOnly>("DateOfBirth");
                b.Property<string>("DisplayName").IsRequired().HasMaxLength(128);
                b.Property<string>("Email").IsRequired().HasMaxLength(256);
                b.Property<bool>("IsActive");
                b.Property<bool>("IsBlocked");
                b.Property<string>("PasswordHash").IsRequired().HasMaxLength(256);
                b.HasKey("Id");
                b.HasIndex("Email").IsUnique();
                b.ToTable("Users");
            });

            modelBuilder.Entity("C_C.App.Model.ChatModel", b =>
            {
                b.HasOne("C_C.App.Model.MatchModel", "Match")
                    .WithOne("Chat")
                    .HasForeignKey("C_C.App.Model.ChatModel", "MatchId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Match");
            });

            modelBuilder.Entity("C_C.App.Model.MatchModel", b =>
            {
                b.HasOne("C_C.App.Model.UserModel", "UserA")
                    .WithMany("MatchesInitiated")
                    .HasForeignKey("UserIdA")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("C_C.App.Model.UserModel", "UserB")
                    .WithMany("MatchesReceived")
                    .HasForeignKey("UserIdB")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("UserA");
                b.Navigation("UserB");
            });

            modelBuilder.Entity("C_C.App.Model.MensajeModel", b =>
            {
                b.HasOne("C_C.App.Model.ChatModel", "Chat")
                    .WithMany("Mensajes")
                    .HasForeignKey("ChatId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Chat");
            });

            modelBuilder.Entity("C_C.App.Model.PerfilModel", b =>
            {
                b.HasOne("C_C.App.Model.UserModel", "User")
                    .WithOne("Perfil")
                    .HasForeignKey("C_C.App.Model.PerfilModel", "UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("User");
            });

            modelBuilder.Entity("C_C.App.Model.PreferenciasModel", b =>
            {
                b.HasOne("C_C.App.Model.UserModel", "User")
                    .WithOne("Preferencias")
                    .HasForeignKey("C_C.App.Model.PreferenciasModel", "UserId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("User");
            });

            modelBuilder.Entity("C_C.App.Model.ReporteModel", b =>
            {
                b.HasOne("C_C.App.Model.UserModel", "Reportado")
                    .WithMany("ReportesRecibidos")
                    .HasForeignKey("ReportadoId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("C_C.App.Model.UserModel", "Reportante")
                    .WithMany("ReportesRealizados")
                    .HasForeignKey("ReportanteId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Reportado");
                b.Navigation("Reportante");
            });

            modelBuilder.Entity("C_C.App.Model.ChatModel", b =>
            {
                b.Navigation("Mensajes");
            });

            modelBuilder.Entity("C_C.App.Model.MatchModel", b =>
            {
                b.Navigation("Chat");
            });

            modelBuilder.Entity("C_C.App.Model.UserModel", b =>
            {
                b.Navigation("MatchesInitiated");
                b.Navigation("MatchesReceived");
                b.Navigation("Perfil");
                b.Navigation("Preferencias");
                b.Navigation("ReportesRealizados");
                b.Navigation("ReportesRecibidos");
            });
#pragma warning restore 612, 618
        }
    }
}
