using System;

namespace C_C_Final.Model
{
    /// <summary>
    ///     Información pública visible para otros usuarios dentro de la aplicación.
    /// </summary>
    public sealed class Perfil
    {
        /// <summary>Identificador interno del perfil.</summary>
        public int IdPerfil { get; set; }

        /// <summary>Referencia a la cuenta propietaria de este perfil.</summary>
        public int IdCuenta { get; set; }

        /// <summary>Apodo mostrado en las tarjetas y chats.</summary>
        public string Nikname { get; set; } = string.Empty;

        /// <summary>Descripción breve proporcionada por el usuario.</summary>
        public string Biografia { get; set; } = string.Empty;

        /// <summary>Imagen de perfil almacenada como arreglo de bytes.</summary>
        public byte[] FotoPerfil { get; set; }

        /// <summary>Marca de tiempo de creación del perfil.</summary>
        public DateTime FechaCreacion { get; set; }
    }
}
