namespace C_C.Model;

public class Alumno
{
    public int Matricula { get; set; }
    public int ID_Cuenta { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apaterno { get; set; } = string.Empty;
    public string Amaterno { get; set; } = string.Empty;
    public DateTime F_Nac { get; set; }
    public char Genero { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string Carrera { get; set; } = string.Empty;
}
