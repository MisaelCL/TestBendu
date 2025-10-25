using System;
using System.Collections.Generic;

namespace C_C.Model
{
    public class PerfilModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Nombre { get; set; }

        public DateTime FechaNacimiento { get; set; }

        public string Carrera { get; set; }

        public string Biografia { get; set; }

        public string FotoPrincipal { get; set; }

        public List<string> Intereses { get; set; } = new List<string>();
    }
}
