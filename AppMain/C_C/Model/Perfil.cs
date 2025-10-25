namespace C_C.Model;

public class Perfil
{
    public int ID_Perfil { get; set; }
    public int ID_Cuenta { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido_Paterno { get; set; } = string.Empty;
    public string? Apellido_Materno { get; set; }
    public string Genero { get; set; } = string.Empty;
    public DateTime Fecha_Nacimiento { get; set; }
    public string Carrera { get; set; } = string.Empty;
    public string? Biografia { get; set; }
    public string? Foto_Principal { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime Fecha_Actualizacion { get; set; }
}
