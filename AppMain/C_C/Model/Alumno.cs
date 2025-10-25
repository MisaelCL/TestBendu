namespace C_C.Model;

public class Alumno
{
    public int IDAlumno { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string? ApellidoMaterno { get; set; }
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
}
