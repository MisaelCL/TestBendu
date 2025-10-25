using System;

namespace C_C.Model
{
    public class MensajeModel
    {
        public Guid Id { get; set; }

        public Guid ChatId { get; set; }

        public Guid RemitenteId { get; set; }

        public string Contenido { get; set; }

        public DateTime EnviadoEl { get; set; }

        public bool Leido { get; set; }
    }
}
