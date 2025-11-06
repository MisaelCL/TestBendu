using System;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Proporciona funciones utilitarias para interpretar y construir los estados de emparejamiento.
    /// </summary>
    internal static class MatchEstadoHelper
    {
        private const string EstadoActivo = "activo";
        private const string EstadoAceptado = "aceptado";
        private const string EstadoPendientePlano = "pendiente";
        private const string EstadoRechazadoPlano = "rechazado";
        private const string EstadoRoto = "roto";
        private const string PrefijoPendiente = "pendiente:";
        private const string PrefijoRechazado = "rechazado:";

        public static bool EsActivo(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return false;
            }

            return string.Equals(estado, EstadoActivo, StringComparison.OrdinalIgnoreCase)
                || string.Equals(estado, EstadoAceptado, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsPendiente(string estado)
        {
            return string.Equals(estado, EstadoPendientePlano, StringComparison.OrdinalIgnoreCase)
                || TryObtenerIdDesdePrefijo(estado, PrefijoPendiente, out _);
        }

        public static bool EsPendienteDe(string estado, int perfilId)
        {
            return TryObtenerIdDesdePrefijo(estado, PrefijoPendiente, out var id) && id == perfilId;
        }

        public static bool EsRechazado(string estado)
        {
            return string.Equals(estado, EstadoRechazadoPlano, StringComparison.OrdinalIgnoreCase)
                || TryObtenerIdDesdePrefijo(estado, PrefijoRechazado, out _);
        }

        public static bool EsRechazadoPor(string estado, int perfilId)
        {
            return TryObtenerIdDesdePrefijo(estado, PrefijoRechazado, out var id) && id == perfilId;
        }

        public static string ConstruirPendiente(int perfilId)
        {
            return $"{PrefijoPendiente}{perfilId}";
        }

        public static string ConstruirRechazado(int perfilId)
        {
            return $"{PrefijoRechazado}{perfilId}";
        }

        public static string ObtenerDescripcionPara(string estado, int perfilActualId, int otroPerfilId)
        {
            if (EsActivo(estado))
            {
                return "¡Ya pueden chatear!";
            }

            if (EsPendienteDe(estado, perfilActualId))
            {
                return "Has enviado un corazón. Espera la respuesta.";
            }

            if (EsPendienteDe(estado, otroPerfilId))
            {
                return "Este perfil te envió un corazón.";
            }

            if (EsRechazadoPor(estado, perfilActualId))
            {
                return "Rechazaste este perfil.";
            }

            if (EsRechazadoPor(estado, otroPerfilId))
            {
                return "Este perfil rechazó tu corazón.";
            }

            if (string.Equals(estado, EstadoRechazadoPlano, StringComparison.OrdinalIgnoreCase))
            {
                return "El match fue rechazado.";
            }

            if (string.Equals(estado, EstadoRoto, StringComparison.OrdinalIgnoreCase))
            {
                return "El chat fue bloqueado.";
            }

            if (string.IsNullOrWhiteSpace(estado))
            {
                return string.Empty;
            }

            return estado;
        }

        private static bool TryObtenerIdDesdePrefijo(string estado, string prefijo, out int perfilId)
        {
            perfilId = 0;
            if (string.IsNullOrWhiteSpace(estado))
            {
                return false;
            }

            var estadoNormalizado = estado.Trim();
            if (!estadoNormalizado.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var segmento = estadoNormalizado.Substring(prefijo.Length);
            return int.TryParse(segmento, out perfilId);
        }
    }
}
