using System;
using System.Data.SqlClient;
using System.Globalization;
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.Resources.Utils;

namespace C_C_Final.Services
{
    /// <summary>
    ///     DTO que encapsula la información necesaria para registrar un nuevo alumno junto con su cuenta y perfil.
    ///     Se mantiene deliberadamente simple para facilitar su construcción desde la capa de presentación.
    /// </summary>
    public sealed class RegisterAlumnoRequest
    {
        /// <summary>Correo institucional o personal que se utilizará como credencial de acceso.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Contraseña en texto plano recibida desde la UI (se cifrará antes de persistirla).</summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>Estado inicial de la cuenta (1 = activa).</summary>
        public byte EstadoCuenta { get; set; } = 1;

        /// <summary>Matrícula institucional ingresada durante el registro.</summary>
        public string Matricula { get; set; } = string.Empty;

        /// <summary>Nombre(s) del solicitante.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Primer apellido del solicitante.</summary>
        public string ApellidoPaterno { get; set; } = string.Empty;

        /// <summary>Segundo apellido del solicitante.</summary>
        public string ApellidoMaterno { get; set; } = string.Empty;

        /// <summary>Fecha de nacimiento utilizada para validar mayoría de edad.</summary>
        public DateTime FechaNacimiento { get; set; }

        /// <summary>Género declarado (M/F) que alimenta filtros iniciales.</summary>
        public char Genero { get; set; }

        /// <summary>Correo preferido para notificaciones o contacto.</summary>
        public string CorreoAlumno { get; set; } = string.Empty;

        /// <summary>Carrera seleccionada durante el registro.</summary>
        public string Carrera { get; set; } = string.Empty;

        /// <summary>Apodo deseado para el perfil público (opcional).</summary>
        public string Nikname { get; set; }

        /// <summary>Biografía inicial que se mostrará en el perfil.</summary>
        public string Biografia { get; set; }

        /// <summary>Imagen que se utilizará como foto de perfil.</summary>
        public byte[] FotoPerfil { get; set; }
    }

    /// <summary>
    /// Orquesta la creación de cuentas, perfiles y preferencias para nuevos alumnos.
    /// </summary>
    /// <summary>
    ///     Servicio de aplicación que orquesta la creación de la cuenta, alumno, perfil y preferencias dentro
    ///     de una transacción ADO.NET para garantizar atomicidad.
    /// </summary>
    public sealed class RegisterAlumnoService
    {
        private readonly CuentaRepository _cuentaRepository;
        private readonly PerfilRepository _perfilRepository;
        private readonly IPreferenciasRepository _preferenciasRepository;
        private readonly string _connectionString;

        /// <summary>
        ///     Construye el servicio con las dependencias necesarias. La cadena de conexión se normaliza para
        ///     reutilizar la infraestructura de <see cref="RepositoryBase"/>.
        /// </summary>
        /// <param name="cuentaRepository">Repositorio encargado de cuentas y alumnos.</param>
        /// <param name="perfilRepository">Repositorio para la tabla <c>Perfil</c>.</param>
        /// <param name="preferenciasRepository">Repositorio para la tabla <c>Preferencias</c>.</param>
        /// <param name="connectionString">Cadena de conexión opcional.</param>
        public RegisterAlumnoService(
            CuentaRepository cuentaRepository,
            PerfilRepository perfilRepository,
            IPreferenciasRepository preferenciasRepository,
            string connectionString = null)
        {
            _cuentaRepository = cuentaRepository ?? throw new ArgumentNullException(nameof(cuentaRepository));
            _perfilRepository = perfilRepository ?? throw new ArgumentNullException(nameof(perfilRepository));
            _preferenciasRepository = preferenciasRepository ?? throw new ArgumentNullException(nameof(preferenciasRepository));
            _connectionString = RepositoryBase.ResolverCadenaConexion(connectionString);
        }

        /// <summary>
        /// Registra un alumno nuevo junto con su cuenta, perfil y preferencias iniciales.
        /// </summary>
        /// <param name="request">Datos necesarios para el registro.</param>
        /// <returns>Identificador de la cuenta creada.</returns>
        public int Registrar(RegisterAlumnoRequest request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("El correo electrónico es obligatorio", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("La contraseña es obligatoria", nameof(request));
            }

            if (request.Genero != 'M' && request.Genero != 'F')
            {
                throw new ArgumentException("El género debe ser 'M' o 'F'", nameof(request));
            }

            // Se trabaja con ADO.NET puro para tener control explícito sobre la transacción.
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var normalizedEmail = request.Email.Trim();
                var hasher = new PasswordHasher();

                if (_cuentaRepository.ExistePorCorreo(normalizedEmail))
                {
                    throw new InvalidOperationException("El correo electrónico ya está registrado.");
                }

                // 1. Se genera el hash y salt de la contraseña en memoria antes de insertarlo.
                var (passwordHash, passwordSalt) = hasher.HashPassword(request.Password);
                var cuentaId = _cuentaRepository.CrearCuenta(connection, transaction, normalizedEmail, passwordHash, passwordSalt, request.EstadoCuenta);

                if (cuentaId <= 0)
                {
                    throw new InvalidOperationException("No se pudo crear la cuenta del alumno.");
                }

                var correoAlumno = string.IsNullOrWhiteSpace(request.CorreoAlumno)
                    ? normalizedEmail
                    : request.CorreoAlumno.Trim();

                // 2. Se registra la información personal en la tabla Alumno.
                var alumno = new Alumno
                {
                    Matricula = request.Matricula?.Trim() ?? string.Empty,
                    IdCuenta = cuentaId,
                    Nombre = request.Nombre?.Trim() ?? string.Empty,
                    ApellidoPaterno = request.ApellidoPaterno?.Trim() ?? string.Empty,
                    ApellidoMaterno = request.ApellidoMaterno?.Trim() ?? string.Empty,
                    FechaNacimiento = request.FechaNacimiento,
                    Genero = request.Genero,
                    Correo = correoAlumno,
                    Carrera = request.Carrera?.Trim() ?? string.Empty
                };
                var rowsAffected = _cuentaRepository.CrearAlumno(connection, transaction, alumno);

                if (rowsAffected <= 0)
                {
                    throw new InvalidOperationException("No se pudo registrar la información del alumno.");
                }

                // 3. Se crea el perfil público asociado, generando un apodo automático si no se proporcionó.
                var perfil = new Perfil
                {
                    IdCuenta = cuentaId,
                    Nikname = string.IsNullOrWhiteSpace(request.Nikname)
                        ? GenerarApodo(alumno.Nombre, alumno.ApellidoPaterno)
                        : request.Nikname!.Trim(),
                    Biografia = request.Biografia ?? string.Empty,
                    FotoPerfil = request.FotoPerfil,
                    FechaCreacion = DateTime.UtcNow
                };
                var perfilId = _perfilRepository.CrearPerfil(connection, transaction, perfil);

                if (perfilId <= 0)
                {
                    throw new InvalidOperationException("No se pudo crear el perfil del alumno.");
                }

                // 4. Finalmente se inicializan las preferencias con valores por defecto que habilitan el uso inmediato.
                var preferencias = new Preferencias
                {
                    IdPerfil = perfilId,
                    PreferenciaGenero = 0,
                    EdadMinima = 18,
                    EdadMaxima = 35,
                    PreferenciaCarrera = string.Empty,
                    Intereses = string.Empty
                };
                var preferenciasId = _preferenciasRepository.CrearPreferencias(connection, transaction, preferencias);

                if (preferenciasId <= 0)
                {
                    throw new InvalidOperationException("No se pudieron guardar las preferencias del perfil.");
                }

                // Si todas las operaciones fueron exitosas, se confirma la transacción para hacer persistentes los cambios.
                transaction.Commit();
                return cuentaId;
            }
            catch
            {
                // Ante cualquier fallo se revierte la transacción para mantener la consistencia.
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Genera un apodo legible basado en el nombre y apellido del alumno.
        /// </summary>
        /// <param name="nombre">Nombre del alumno.</param>
        /// <param name="apellidoPaterno">Apellido paterno del alumno.</param>
        /// <returns>Apodo sugerido.</returns>
        private static string GenerarApodo(string nombre, string apellidoPaterno)
        {
            var safeNombre = string.IsNullOrWhiteSpace(nombre) ? "user" : nombre;
            var safeApellido = string.IsNullOrWhiteSpace(apellidoPaterno) ? "cc" : apellidoPaterno;
            var nombrePart = safeNombre.Length <= 3 ? safeNombre : safeNombre.Substring(0, 3);
            var apellidoPart = safeApellido.Length <= 3 ? safeApellido : safeApellido.Substring(0, 3);
            var baseName = string.Concat(nombrePart, apellidoPart).ToLowerInvariant();
            var suffix = DateTime.UtcNow.ToString("ddHHmm", CultureInfo.InvariantCulture);
            return $"{baseName}{suffix}";
        }
    }
}
