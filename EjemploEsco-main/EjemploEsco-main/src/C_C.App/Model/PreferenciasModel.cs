using System;
using System.ComponentModel.DataAnnotations;

namespace C_C.App.Model;

public class PreferenciasModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    [Range(18, 120)]
    public int EdadMinima { get; set; } = 18;

    [Range(18, 120)]
    public int EdadMaxima { get; set; } = 80;

    [MaxLength(64)]
    public string GeneroBuscado { get; set; } = "Todos";

    public double DistanciaMaximaKm { get; set; } = 100;

    public UserModel? User { get; set; }
}
