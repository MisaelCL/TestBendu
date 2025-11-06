using System;

namespace C_C_Final.Model
{
    /// <summary>
    ///     Representa la entidad de autenticación. Contiene la información privada que permite validar
    ///     la identidad del alumno (correo y contraseña cifrada), así como el estado de la cuenta.
    /// </summary>
    public sealed class Cuenta
    {
        /// <summary>
        ///     Identificador único generado por la base de datos para relacionar la cuenta con otras tablas
        ///     (Alumno, Perfil, Match, etc.).
        /// </summary>
        public int IdCuenta { get; set; }

        /// <summary>
        ///     Correo electrónico utilizado para iniciar sesión. Es único y sirve como punto de contacto
        ///     principal con el usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        ///     Contraseña del usuario transformada mediante el algoritmo PBKDF2 para evitar almacenar texto
        ///     plano y mejorar la seguridad.
        /// </summary>
        public string HashContrasena { get; set; } = string.Empty;

        /// <summary>
        ///     Valor aleatorio (salt) asociado al hash para reforzar la protección ante ataques de diccionario.
        /// </summary>
        public string SaltContrasena { get; set; } = string.Empty;

        /// <summary>
        ///     Estado de la cuenta (activa, bloqueada, etc.) representado como byte para mapear directamente
        ///     los valores de la base de datos.
        /// </summary>
        public byte EstadoCuenta { get; set; }

        /// <summary>
        ///     Fecha y hora de creación de la cuenta. Permite realizar auditorías y métricas de actividad.
        /// </summary>
        public DateTime FechaRegistro { get; set; }
    }
}
