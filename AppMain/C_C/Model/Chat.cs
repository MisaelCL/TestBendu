namespace C_C.Model;

public class Chat
{
    public int IDChat { get; set; }
    public int IDMatch { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? LastMessageAtUtc { get; set; }
    public long? LastMessageId { get; set; }
}
