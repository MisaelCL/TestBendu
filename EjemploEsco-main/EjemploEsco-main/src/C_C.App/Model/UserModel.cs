using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace C_C.App.Model;

public class UserModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public bool IsBlocked { get; set; }

    public PerfilModel? Perfil { get; set; }

    public PreferenciasModel? Preferencias { get; set; }

    public ICollection<MatchModel> MatchesInitiated { get; set; } = new List<MatchModel>();

    public ICollection<MatchModel> MatchesReceived { get; set; } = new List<MatchModel>();

    public ICollection<ChatModel> Chats { get; set; } = new List<ChatModel>();

    public ICollection<ReporteModel> ReportesRealizados { get; set; } = new List<ReporteModel>();

    public ICollection<ReporteModel> ReportesRecibidos { get; set; } = new List<ReporteModel>();
}
