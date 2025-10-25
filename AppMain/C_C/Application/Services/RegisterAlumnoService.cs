using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using C_C_Final.Application.Repositories;
using C_C_Final.Domain.Models;
using C_C_Final.Infrastructure.Data;

namespace C_C_Final.Application.Services
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
        public string? Nikname { get; set; }
        public string? Biografia { get; set; }
        public byte[]? FotoPerfil { get; set; }
    }

    public sealed class RegisterAlumnoService
    {
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IPerfilRepository _perfilRepository;
        private readonly SqlConnectionFactory _connectionFactory;

        public RegisterAlumnoService(ICuentaRepository cuentaRepository, IPerfilRepository perfilRepository, SqlConnectionFactory connectionFactory)
        {
            _cuentaRepository = cuentaRepository;
            _perfilRepository = perfilRepository;
            _connectionFactory = connectionFactory;
        }

        public async Task<int> RegisterAsync(RegisterAlumnoRequest request, CancellationToken ct = default)
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

            using var unitOfWork = await UnitOfWork.CreateAsync(_connectionFactory, ct).ConfigureAwait(false);
            try
            {
                var passwordHash = ComputeHash(request.Password);
                var cuentaId = await _cuentaRepository.CreateCuentaAsync(unitOfWork.Connection, unitOfWork.Transaction, request.Email, passwordHash, request.EstadoCuenta, ct).ConfigureAwait(false);

                var alumno = new Alumno
                {
                    Matricula = request.Matricula,
                    IdCuenta = cuentaId,
                    Nombre = request.Nombre,
                    ApellidoPaterno = request.ApellidoPaterno,
                    ApellidoMaterno = request.ApellidoMaterno,
                    FechaNacimiento = request.FechaNacimiento,
                    Genero = request.Genero,
                    Correo = request.CorreoAlumno,
                    Carrera = request.Carrera
                };
                await _cuentaRepository.CreateAlumnoAsync(unitOfWork.Connection, unitOfWork.Transaction, alumno, ct).ConfigureAwait(false);

                var perfil = new Perfil
                {
                    IdCuenta = cuentaId,
                    Nikname = string.IsNullOrWhiteSpace(request.Nikname) ? GenerateNikname(request.Nombre, request.ApellidoPaterno) : request.Nikname!,
                    Biografia = request.Biografia ?? string.Empty,
                    FotoPerfil = request.FotoPerfil,
                    FechaCreacion = DateTime.UtcNow
                };
                var perfilId = await _perfilRepository.CreatePerfilAsync(unitOfWork.Connection, unitOfWork.Transaction, perfil, ct).ConfigureAwait(false);

                var preferencias = new Preferencias
                {
                    IdPerfil = perfilId,
                    PreferenciaGenero = 0,
                    EdadMinima = 18,
                    EdadMaxima = 35,
                    PreferenciaCarrera = string.Empty,
                    Intereses = string.Empty
                };
                await _perfilRepository.UpsertPreferenciasAsync(unitOfWork.Connection, unitOfWork.Transaction, preferencias, ct).ConfigureAwait(false);

                await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
                return cuentaId;
            }
            catch
            {
                await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        private static string ComputeHash(string value)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(value);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
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
