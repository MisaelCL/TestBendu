using System;
namespace C_C.Domain;

public class Perfil
{
    public int ID_Perfil { get; set; }
    public int ID_Cuenta { get; set; }
    public string Nikname { get; set; } = string.Empty;
    public string? Biografia { get; set; }
    public byte[]? Foto_Perfil { get; set; }
    public DateTime Fecha_Creacion { get; set; }
}
