using System;

namespace C_C_Final.Model
{
    public sealed class Perfil
    {
        public int IdPerfil { get; set; }
        public int IdCuenta { get; set; }
        public string Nikname { get; set; } = string.Empty;
        public string Biografia { get; set; } = string.Empty;
        public byte[] FotoPerfil { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
