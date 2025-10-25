namespace C_C.Model;

public class Alumno
{
    public int ID_Alumno { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido_Paterno { get; set; } = string.Empty;
    public string? Apellido_Materno { get; set; }
    public DateTime Fecha_Nacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
    public DateTime Fecha_Registro { get; set; }
}
