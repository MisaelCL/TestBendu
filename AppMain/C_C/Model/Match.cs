using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    public class Match
    {
        public int Id { get; set; }
        public int IdPerfilEmisor { get; set; }
        public int IdPerfilReceptor { get; set; }
        public DateTime FechaMatch { get; set; }
        public string EstadoMatch { get; set; }
    }
}
