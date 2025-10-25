using System;

namespace C_C_Final.Domain.Models
{
    public sealed class Cuenta
    {
        public int IdCuenta { get; set; }
        public string Email { get; set; } = string.Empty;
        public string HashContrasena { get; set; } = string.Empty;
        public byte EstadoCuenta { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
