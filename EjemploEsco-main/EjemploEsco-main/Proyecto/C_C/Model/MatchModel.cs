using System;

namespace C_C.Model
{
    public class MatchModel
    {
        public Guid Id { get; set; }

        public Guid UsuarioAId { get; set; }

        public Guid UsuarioBId { get; set; }

        public DateTime CreadoEl { get; set; }

        public bool Activo { get; set; }
    }
}
