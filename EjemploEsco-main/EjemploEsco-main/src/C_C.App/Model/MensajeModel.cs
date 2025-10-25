using System;
using System.ComponentModel.DataAnnotations;

namespace C_C.App.Model;

public class MensajeModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ChatId { get; set; }

    public Guid RemitenteId { get; set; }

    [Required]
    [MaxLength(1024)]
    public string Contenido { get; set; } = string.Empty;

    public DateTime EnviadoEnUtc { get; set; } = DateTime.UtcNow;

    public bool Leido { get; set; }

    public ChatModel? Chat { get; set; }
}
