namespace C_C.Model;

public class Preferencias
{
    public int ID_Preferencias { get; set; }
    public int ID_Perfil { get; set; }
    public byte Preferencia_Genero { get; set; }
    public int Edad_Minima { get; set; }
    public int Edad_Maxima { get; set; }
    public int RadioKm { get; set; }
}
