namespace C_C.Model;

public class Chat
{
    public int ID_Chat { get; set; }
    public int ID_Match { get; set; }
    public DateTime Fecha_Creacion { get; set; }
    public DateTime? LastMessageAtUtc { get; set; }
    public long? LastMessageId { get; set; }
}
