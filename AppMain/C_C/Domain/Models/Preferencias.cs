namespace C_C_Final.Domain.Models
{
    public sealed class Preferencias
    {
        public int IdPreferencias { get; set; }
        public int IdPerfil { get; set; }
        public byte PreferenciaGenero { get; set; }
        public int EdadMinima { get; set; }
        public int EdadMaxima { get; set; }
        public string PreferenciaCarrera { get; set; } = string.Empty;
        public string Intereses { get; set; } = string.Empty;
    }
}
