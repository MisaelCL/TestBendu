using System;

namespace C_C_Final.Model
{
    /// <summary>
    ///     Representa los datos personales y académicos vinculados a una cuenta.
    /// </summary>
    public sealed class Alumno
    {
        /// <summary>Matrícula institucional utilizada como identificador de negocio.</summary>
        public string Matricula { get; set; } = string.Empty;

        /// <summary>Identificador de la cuenta a la que pertenece este alumno.</summary>
        public int IdCuenta { get; set; }

        /// <summary>Nombre(s) del alumno.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Primer apellido del alumno.</summary>
        public string ApellidoPaterno { get; set; } = string.Empty;

        /// <summary>Segundo apellido del alumno.</summary>
        public string ApellidoMaterno { get; set; } = string.Empty;

        /// <summary>Fecha de nacimiento (utilizada para validar mayoría de edad).</summary>
        public DateTime FechaNacimiento { get; set; }

        /// <summary>Sexo declarado (M/F) utilizado en filtros y estadísticas.</summary>
        public char Genero { get; set; }

        /// <summary>Carrera académica principal que cursa el alumno.</summary>
        public string Carrera { get; set; } = string.Empty;
    }
}
