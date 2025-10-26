using System;

namespace C_C_Final.Model
{
    public sealed class Chat
    {
        public int IdChat { get; set; }
        public int IdMatch { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? LastMessageAtUtc { get; set; }
        public long? LastMessageId { get; set; }
    }
}
