namespace C_C.Model;

public class Mensaje
{
    public long ID_Mensaje { get; set; }
    public int ID_Chat { get; set; }
    public int Remitente { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public DateTime Fecha_Envio { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsEdited { get; set; }
    public bool Confirmacion_Lectura { get; set; }
}
