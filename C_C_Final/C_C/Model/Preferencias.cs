namespace C_C_Final.Model
{
    /// <summary>
    ///     Configuración de filtros que determinan qué perfiles se sugieren al usuario.
    /// </summary>
    public sealed class Preferencias
    {
        /// <summary>Identificador del registro de preferencias.</summary>
        public int IdPreferencias { get; set; }

        /// <summary>Perfil al que pertenecen estas preferencias.</summary>
        public int IdPerfil { get; set; }

        /// <summary>Filtro de género (0 = todos, otros valores representan opciones específicas).</summary>
        public byte PreferenciaGenero { get; set; }

        /// <summary>Edad mínima aceptada para las sugerencias.</summary>
        public int EdadMinima { get; set; }

        /// <summary>Edad máxima aceptada para las sugerencias.</summary>
        public int EdadMaxima { get; set; }

        /// <summary>Carrera preferida (vacío significa cualquiera).</summary>
        public string PreferenciaCarrera { get; set; } = string.Empty;

        /// <summary>Intereses declarados por el usuario para refinar coincidencias.</summary>
        public string Intereses { get; set; } = string.Empty;
    }
}
