using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C_C.Model
{
    public class Alumno
    {
        public int Matricula { get; set; }
        public string Nombre { get; set; }
        public string Amaterno { get; set; }
        public string Apaterno { get; set; }
        public int Edad { get; set; }
        public Char Genero { get; set; }
        public string Carrera { get; set; }
        public string Email { get; set; }
    }
}
