using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    public class Cuenta
    {
        public int IdCuenta { get; set; }
        public string HashContrasena { get; set; }
        public int Matricula { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string EstadoCuenta { get; set; }
    }
}
