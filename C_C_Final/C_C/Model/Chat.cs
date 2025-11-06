using System;

namespace C_C_Final.Model
{
    /// <summary>
    ///     Contenedor de conversación asociado a un match aceptado.
    /// </summary>
    public sealed class Chat
    {
        /// <summary>Identificador del chat.</summary>
        public int IdChat { get; set; }

        /// <summary>Match al que pertenece este chat.</summary>
        public int IdMatch { get; set; }

        /// <summary>Fecha de creación del chat (UTC).</summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>Fecha del último mensaje enviado (en UTC).</summary>
        public DateTime? LastMessageAtUtc { get; set; }

        /// <summary>Identificador del último mensaje para acceso rápido.</summary>
        public long? LastMessageId { get; set; }
    }
}
