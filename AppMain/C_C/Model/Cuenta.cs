namespace C_C.Model;

public class Cuenta
{
    public int ID_Cuenta { get; set; }
    public int ID_Alumno { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime Fecha_Registro { get; set; }
    public DateTime? Ultimo_Acceso { get; set; }
    public bool IsActive { get; set; }
}
