using System;
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

    public sealed class RegisterAlumnoService
    {
        private readonly CuentaRepository _cuentaRepository;
        private readonly PerfilRepository _perfilRepository;
        private readonly SqlConnectionFactory _connectionFactory;

        public RegisterAlumnoService(CuentaRepository cuentaRepository, PerfilRepository perfilRepository, SqlConnectionFactory connectionFactory)
        {
            _cuentaRepository = cuentaRepository;
            _perfilRepository = perfilRepository;
            _connectionFactory = connectionFactory;
        }

        public int Register(RegisterAlumnoRequest request)
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

            using var unitOfWork = UnitOfWork.Create(_connectionFactory);
            try
            {
                var normalizedEmail = request.Email.Trim();

                if (_cuentaRepository.ExistsByEmail(normalizedEmail))
                {
                    throw new InvalidOperationException("El correo electrónico ya está registrado.");
                }

                var passwordHash = HashFunction.ComputeHash(request.Password);
                var cuentaId = _cuentaRepository.CreateCuenta(unitOfWork.Connection, unitOfWork.Transaction, normalizedEmail, passwordHash, request.EstadoCuenta);

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
                var alumnoId = _cuentaRepository.CreateAlumno(unitOfWork.Connection, unitOfWork.Transaction, alumno);

                if (alumnoId <= 0)
                {
                    throw new InvalidOperationException("No se pudo registrar la información del alumno.");
                }

                var perfil = new Perfil
                {
                    IdCuenta = cuentaId,
                    Nikname = string.IsNullOrWhiteSpace(request.Nikname)
                        ? GenerateNikname(alumno.Nombre, alumno.ApellidoPaterno)
                        : request.Nikname!.Trim(),
                    Biografia = request.Biografia ?? string.Empty,
                    FotoPerfil = request.FotoPerfil,
                    FechaCreacion = DateTime.UtcNow
                };
                var perfilId = _perfilRepository.CreatePerfil(unitOfWork.Connection, unitOfWork.Transaction, perfil);

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
                var preferenciasId = _perfilRepository.UpsertPreferencias(unitOfWork.Connection, unitOfWork.Transaction, preferencias);

                if (preferenciasId <= 0)
                {
                    throw new InvalidOperationException("No se pudieron guardar las preferencias del perfil.");
                }

                unitOfWork.Commit();
                return cuentaId;
            }
            catch
            {
                unitOfWork.Rollback();
                throw;
            }
        }

        private static string GenerateNikname(string nombre, string apellidoPaterno)
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
