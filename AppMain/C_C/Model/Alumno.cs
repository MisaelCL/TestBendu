using System;

namespace C_C_Final.Model
{
    public sealed class Alumno
    {
        public string Matricula { get; set; } = string.Empty;
        public int IdCuenta { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public char Genero { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
    }
}
