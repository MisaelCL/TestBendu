using System;

namespace C_C_Final.Model
{
    /// <summary>
    ///     Representa una interacción registrada entre dos perfiles (pendiente, aceptada, rechazada o rota).
    /// </summary>
    public sealed class Match
    {
        /// <summary>Identificador del registro de match.</summary>
        public int IdMatch { get; set; }

        /// <summary>Perfil que inició la interacción.</summary>
        public int PerfilEmisor { get; set; }

        /// <summary>Perfil que recibió la interacción.</summary>
        public int PerfilReceptor { get; set; }

        /// <summary>Estado actual del match (pendiente/aceptado/rechazado/roto).</summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>Fecha de la última actualización o creación del match.</summary>
        public DateTime FechaMatch { get; set; }
    }
}
