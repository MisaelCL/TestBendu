using System;
using System.ComponentModel.DataAnnotations;

namespace C_C.App.Model;

public class PerfilModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Required]
    [MaxLength(512)]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(128)]
    public string Ciudad { get; set; } = string.Empty;

    [MaxLength(256)]
    public string FotoUrl { get; set; } = string.Empty;

    public string Intereses { get; set; } = string.Empty;

    public UserModel? User { get; set; }
}
