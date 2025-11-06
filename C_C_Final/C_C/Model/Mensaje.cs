using System;

namespace C_C_Final.Model
{
    public sealed class Mensaje
    {
        public long IdMensaje { get; set; }
        public int IdChat { get; set; }
        public int IdRemitentePerfil { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool ConfirmacionLectura { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAtUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
