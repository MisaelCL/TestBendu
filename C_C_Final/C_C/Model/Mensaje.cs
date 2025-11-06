using System;

namespace C_C_Final.Model
{
    /// <summary>
    ///     Mensaje individual enviado dentro de un chat.
    /// </summary>
    public sealed class Mensaje
    {
        /// <summary>Identificador del mensaje (clave primaria).</summary>
        public long IdMensaje { get; set; }

        /// <summary>Identificador del chat al que pertenece el mensaje.</summary>
        public int IdChat { get; set; }

        /// <summary>Perfil remitente que envió el mensaje.</summary>
        public int IdRemitentePerfil { get; set; }

        /// <summary>Contenido textual del mensaje.</summary>
        public string Contenido { get; set; } = string.Empty;

        /// <summary>Fecha y hora de envío (UTC).</summary>
        public DateTime FechaEnvio { get; set; }

        /// <summary>Indica si el receptor marcó el mensaje como leído.</summary>
        public bool ConfirmacionLectura { get; set; }

        /// <summary>Bandera que indica si el mensaje fue editado.</summary>
        public bool IsEdited { get; set; }

        /// <summary>Fecha de la última edición (si aplica).</summary>
        public DateTime? EditedAtUtc { get; set; }

        /// <summary>Marca lógica de borrado.</summary>
        public bool IsDeleted { get; set; }
    }
}
