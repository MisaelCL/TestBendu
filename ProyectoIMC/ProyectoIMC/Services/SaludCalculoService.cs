using System;
using ProyectoIMC.Model;

namespace ProyectoIMC.Services
{
    public static class SaludCalculoService
    {
        // Calcula el IMC clásico. Si falta peso o estatura devuelve 0 para evitar crashes raros.
        public static double CalcularImc(Paciente p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (p.EstaturaCm <= 0 || p.PesoKg <= 0) return 0;

            var estaturaM = p.EstaturaCm / 100.0;
            return p.PesoKg / (estaturaM * estaturaM);
        }

        // Devuelve una etiqueta amigable según el IMC calculado.
        public static string ClasificarImc(double imc)
        {
            if (imc <= 0) return "Sin datos";
            if (imc < 18.5) return "Bajo peso";
            if (imc < 25.0) return "Normal";
            if (imc < 30.0) return "Sobrepeso";
            if (imc < 35.0) return "Obesidad grado I";
            if (imc < 40.0) return "Obesidad grado II";
            return "Obesidad grado III";
        }

        private static bool EsHombre(Paciente p)
        {
            return string.Equals(p.Sexo, "M", StringComparison.OrdinalIgnoreCase);
        }


        // Calcula un estimado rápido de porcentaje de grasa usando IMC, edad y sexo.
        public static double CalcularPorcentajeGrasa(Paciente p, double imc)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (imc <= 0 || p.Edad <= 0) return 0;

            var sexoNum = EsHombre(p) ? 1 : 0;
            return 1.2 * imc + 0.23 * p.Edad - 10.8 * sexoNum - 5.4;
        }

        // Usa la fórmula de Broca modificada para dar un peso ideal aproximado.
        public static double CalcularPesoIdeal(Paciente p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (p.EstaturaCm <= 0) return 0;

            var h = p.EstaturaCm;
            if (EsHombre(p))
            {
                return h - 100.0 - ((h - 150.0) / 4.0);
            }

            return h - 100.0 - ((h - 150.0) / 2.5);
        }

        // Calcula el metabolismo basal con Mifflin-St Jeor, devolviendo 0 si faltan datos.
        public static double CalcularBmr(Paciente p)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (p.PesoKg <= 0 || p.EstaturaCm <= 0 || p.Edad <= 0) return 0;

            var h = p.EstaturaCm;
            var w = p.PesoKg;
            var edad = p.Edad;

            var baseBmr = (h * 6.25) + (w * 9.99) - (edad * 4.92);

            return EsHombre(p)
                ? baseBmr + 5
                : baseBmr - 161;
        }




        // Selecciona el multiplicador de actividad según el nivel elegido en pantalla.
        public static double ObtenerFactorActividad(Paciente p)
        {
            return p.NivelActividad switch
            {
                1 => 1.2,
                2 => 1.375,
                3 => 1.55,
                4 => 1.725,
                5 => 1.9,
                _ => 1.2
            };
        }

        // Multiplica el BMR por el factor de actividad para obtener calorías diarias.
        public static double CalcularTdee(Paciente p)
        {
            var bmr = CalcularBmr(p);
            var factor = ObtenerFactorActividad(p);
            return bmr * factor;
        }
    }
}
