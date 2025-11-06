using System;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Proporciona funciones utilitarias para interpretar y construir los estados de emparejamiento.
    /// </summary>
    internal static class MatchEstadoHelper
    {
        private const string EstadoAceptado = "aceptado";
        private const string EstadoPendiente = "pendiente";
        private const string EstadoRechazado = "rechazado";
        private const string EstadoRoto = "roto";

        private static string NormalizarEntrada(string estado)
        {
            return string.IsNullOrWhiteSpace(estado) ? string.Empty : estado.Trim();
        }

        public static bool EsActivo(string estado)
        {
            var estadoNormalizado = NormalizarEntrada(estado);

            if (string.IsNullOrEmpty(estadoNormalizado))
            {
                return false;
            }

            return string.Equals(estadoNormalizado, EstadoAceptado, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsPendiente(string estado)
        {
            var estadoNormalizado = NormalizarEntrada(estado);
            return string.Equals(estadoNormalizado, EstadoPendiente, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsPendienteDe(string estado, int perfilEmisor, int perfilId)
        {
            return EsPendiente(estado) && perfilEmisor == perfilId;
        }

        public static bool EsRechazado(string estado)
        {
            var estadoNormalizado = NormalizarEntrada(estado);
            return string.Equals(estadoNormalizado, EstadoRechazado, StringComparison.OrdinalIgnoreCase);
        }

        public static string ConstruirAceptado()
        {
            return EstadoAceptado;
        }

        public static string ConstruirPendiente()
        {
            return EstadoPendiente;
        }

        public static string ConstruirRechazado()
        {
            return EstadoRechazado;
        }

        public static string NormalizarParaPersistencia(string estado)
        {
            var estadoNormalizado = NormalizarEntrada(estado);

            if (string.Equals(estadoNormalizado, EstadoAceptado, StringComparison.OrdinalIgnoreCase))
            {
                return EstadoAceptado;
            }

            if (string.Equals(estadoNormalizado, EstadoPendiente, StringComparison.OrdinalIgnoreCase))
            {
                return EstadoPendiente;
            }

            if (string.Equals(estadoNormalizado, EstadoRechazado, StringComparison.OrdinalIgnoreCase))
            {
                return EstadoRechazado;
            }

            if (string.Equals(estadoNormalizado, EstadoRoto, StringComparison.OrdinalIgnoreCase))
            {
                return EstadoRoto;
            }

            return estadoNormalizado.ToLowerInvariant();
        }

        public static string ObtenerDescripcionPara(string estado, int perfilActualId, int otroPerfilId, int perfilEmisor)
        {
            var estadoNormalizado = NormalizarEntrada(estado);

            if (EsActivo(estadoNormalizado))
            {
                return "¡Ya pueden chatear!";
            }

            if (EsPendienteDe(estadoNormalizado, perfilEmisor, perfilActualId))
            {
                return "Has enviado un corazón. Espera la respuesta.";
            }

            if (EsPendienteDe(estadoNormalizado, perfilEmisor, otroPerfilId))
            {
                return "Este perfil te envió un corazón.";
            }

            if (EsRechazado(estadoNormalizado) && perfilEmisor == perfilActualId)
            {
                return "Rechazaste este perfil.";
            }

            if (EsRechazado(estadoNormalizado) && perfilEmisor == otroPerfilId)
            {
                return "Este perfil rechazó tu corazón.";
            }

            if (EsRechazado(estadoNormalizado))
            {
                return "El match fue rechazado.";
            }

            if (string.Equals(estadoNormalizado, EstadoRoto, StringComparison.OrdinalIgnoreCase))
            {
                return "El chat fue bloqueado.";
            }

            if (string.IsNullOrEmpty(estadoNormalizado))
            {
                return string.Empty;
            }

            return estadoNormalizado;
        }
    }
}
