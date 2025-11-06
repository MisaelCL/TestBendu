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

        public static bool EsActivo(string estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                return false;
            }

            return string.Equals(estado, EstadoAceptado, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsPendiente(string estado)
        {
            return string.Equals(estado, EstadoPendiente, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsPendienteDe(string estado, int perfilEmisor, int perfilId)
        {
            return EsPendiente(estado) && perfilEmisor == perfilId;
        }

        public static bool EsRechazado(string estado)
        {
            return string.Equals(estado, EstadoRechazado, StringComparison.OrdinalIgnoreCase);
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

        public static string ObtenerDescripcionPara(string estado, int perfilActualId, int otroPerfilId, int perfilEmisor)
        {
            if (EsActivo(estado))
            {
                return "¡Ya pueden chatear!";
            }

            if (EsPendienteDe(estado, perfilEmisor, perfilActualId))
            {
                return "Has enviado un corazón. Espera la respuesta.";
            }

            if (EsPendienteDe(estado, perfilEmisor, otroPerfilId))
            {
                return "Este perfil te envió un corazón.";
            }

            if (EsRechazado(estado) && perfilEmisor == perfilActualId)
            {
                return "Rechazaste este perfil.";
            }

            if (EsRechazado(estado) && perfilEmisor == otroPerfilId)
            {
                return "Este perfil rechazó tu corazón.";
            }

            if (EsRechazado(estado))
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
    }
}
