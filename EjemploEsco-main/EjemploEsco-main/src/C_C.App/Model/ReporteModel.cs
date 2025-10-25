using System;
using System.ComponentModel.DataAnnotations;

namespace C_C.App.Model;

public class ReporteModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ReportanteId { get; set; }

    public Guid ReportadoId { get; set; }

    [Required]
    [MaxLength(512)]
    public string Motivo { get; set; } = string.Empty;

    public DateTime CreadoEnUtc { get; set; } = DateTime.UtcNow;

    public UserModel? Reportante { get; set; }

    public UserModel? Reportado { get; set; }
}
