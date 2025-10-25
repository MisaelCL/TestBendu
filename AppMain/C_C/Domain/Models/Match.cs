using System;

namespace C_C_Final.Domain.Models
{
    public sealed class Match
    {
        public int IdMatch { get; set; }
        public int PerfilEmisor { get; set; }
        public int PerfilReceptor { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaMatch { get; set; }
    }
}
