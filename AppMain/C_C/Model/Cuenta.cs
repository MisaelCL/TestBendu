namespace C_C.Model;

public class Cuenta
{
    public int IDCuenta { get; set; }
    public int IDAlumno { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public bool IsActive { get; set; }
}
