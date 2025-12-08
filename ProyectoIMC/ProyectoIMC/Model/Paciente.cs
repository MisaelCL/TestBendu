using SQLite;

namespace ProyectoIMC.Model
{
    [Table("Pacientes")]
    public sealed class Paciente
    {
        [PrimaryKey, AutoIncrement]
        public int IdPaciente { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;

        public int Edad { get; set; }

        // Peso en kilogramos
        public double PesoKg { get; set; }

        // Estatura en centímetros
        public double EstaturaCm { get; set; }

        // "M" = Masculino, "F" = Femenino
        public string Sexo { get; set; } = "M";

        // 1 = Sedentario, 2 = Ligero, 3 = Moderado, 4 = Intenso, 5 = Muy intenso
        public int NivelActividad { get; set; } = 1;
    }
}
