using System;
using System.Collections.Generic;

namespace C_C.Model
{
    public class ChatModel
    {
        public Guid Id { get; set; }

        public Guid MatchId { get; set; }

        public Guid UsuarioAId { get; set; }

        public Guid UsuarioBId { get; set; }

        public List<MensajeModel> Mensajes { get; } = new List<MensajeModel>();
    }
}
