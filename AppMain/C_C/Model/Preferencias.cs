using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    public class Preferencias
    {
        public int IdPreferencia { get; set; }
        public int IdPerfil { get; set; }
        public char PreferenciaGenero { get; set; }
        public int EdadMinima { get; set; }
        public int EdadMaxima { get; set; }
    }
}