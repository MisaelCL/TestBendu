namespace C_C.Model;

public class Cuenta
{
    public int ID_Cuenta { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Hash_Contrasena { get; set; } = string.Empty;
    public byte Estado_Cuenta { get; set; }
    public DateTime Fecha_Registro { get; set; }
}
