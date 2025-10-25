using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    public class Mensaje
    {
        public int IdMensaje {  get; set; }
        public  int IdChat { get; set; }
        public DateTime Fecha { get; set; }
        public bool ConfirmacionLectura { get; set; 
    }
}
