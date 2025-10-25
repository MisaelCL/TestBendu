using System;
namespace C_C.Domain;

public class Preferencias
{
    public int ID_Preferencias { get; set; }
    public int ID_Perfil { get; set; }
    public byte Preferencia_Genero { get; set; }
    public int Edad_Minima { get; set; }
    public int Edad_Maxima { get; set; }
    public string Preferencia_Carrera { get; set; } = string.Empty;
    public string? Intereses { get; set; }
}
