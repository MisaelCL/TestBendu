using System;
using System.Data.SqlClient;
using System.Globalization;
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.Resources.Utils;

namespace C_C_Final.Services
{
    public sealed class RegisterAlumnoRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public byte EstadoCuenta { get; set; } = 1;
        public string Matricula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public char Genero { get; set; }
        public string CorreoAlumno { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Nikname { get; set; }
        public string Biografia { get; set; }
        public byte[] FotoPerfil { get; set; }
    }

    /// <summary>
    /// Orquesta la creación de cuentas, perfiles y preferencias para nuevos alumnos.
    /// </summary>
    public sealed class RegisterAlumnoService
    {
        private readonly CuentaRepository _cuentaRepository;
        private readonly PerfilRepository _perfilRepository;
        private readonly string _connectionString;

        public RegisterAlumnoService(CuentaRepository cuentaRepository, PerfilRepository perfilRepository, string connectionString = null)
        {
            _cuentaRepository = cuentaRepository;
            _perfilRepository = perfilRepository;
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

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var hasher = new PasswordHasher();
                var normalizedEmail = request.Email.Trim();

                if (_cuentaRepository.ExistePorCorreo(normalizedEmail))
                {
                    throw new InvalidOperationException("El correo electrónico ya está registrado.");
                }

                var (passwordHash, passwordSalt) = hasher.HashPassword(request.Password);
                var cuentaId = _cuentaRepository.CrearCuenta(connection, transaction, normalizedEmail, passwordHash, passwordSalt, request.EstadoCuenta);

                if (cuentaId <= 0)
                {
                    throw new InvalidOperationException("No se pudo crear la cuenta del alumno.");
                }

                var correoAlumno = string.IsNullOrWhiteSpace(request.CorreoAlumno)
                    ? normalizedEmail
                    : request.CorreoAlumno.Trim();

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

                var preferencias = new Preferencias
                {
                    IdPerfil = perfilId,
                    PreferenciaGenero = 0,
                    EdadMinima = 18,
                    EdadMaxima = 35,
                    PreferenciaCarrera = string.Empty,
                    Intereses = string.Empty
                };
                var preferenciasId = _perfilRepository.InsertarOActualizarPreferencias(connection, transaction, preferencias);

                if (preferenciasId < 0)
                {
                    throw new InvalidOperationException("No se pudieron guardar las preferencias del perfil.");
                }

                transaction.Commit();
                return cuentaId;
            }
            catch
            {
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
