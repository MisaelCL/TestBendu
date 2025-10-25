namespace C_C.Model;

public class Match
{
    public int ID_Match { get; set; }
    public int Perfil_Emisor { get; set; }
    public int Perfil_Receptor { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime Fecha_Match { get; set; }
}
