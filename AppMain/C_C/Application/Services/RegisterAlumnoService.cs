using C_C.Application.Repositories;
using C_C.Domain;
using C_C.Infrastructure.Common;
using C_C.Resources.utils;

namespace C_C.Application.Services;

public sealed record RegisterAlumnoRequest(
    string Email,
    string Password,
    int Matricula,
    string Nombre,
    string Apaterno,
    string Amaterno,
    DateTime FechaNacimiento,
    char Genero,
    string CorreoInstitucional,
    string Carrera,
    string? Nikname,
    string? Biografia,
    byte[]? FotoPerfil,
    byte PreferenciaGenero,
    int EdadMinima,
    int EdadMaxima,
    string PreferenciaCarrera,
    string? Intereses
);

public interface IRegisterAlumnoService
{
    Task<int> RegisterAsync(RegisterAlumnoRequest request, CancellationToken ct = default);
}

public sealed class RegisterAlumnoService : IRegisterAlumnoService
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IPerfilRepository _perfilRepository;
    private readonly SqlConnectionFactory _connectionFactory;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterAlumnoService(
        ICuentaRepository cuentaRepository,
        IPerfilRepository perfilRepository,
        SqlConnectionFactory connectionFactory,
        IPasswordHasher passwordHasher)
    {
        _cuentaRepository = cuentaRepository;
        _perfilRepository = perfilRepository;
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
    }

    public async Task<int> RegisterAsync(RegisterAlumnoRequest request, CancellationToken ct = default)
    {
        if (request.FechaNacimiento > DateTime.UtcNow.AddYears(-18))
        {
            throw new InvalidOperationException("El alumno debe ser mayor de 18 años.");
        }

        if (await _cuentaRepository.ExistsByEmailAsync(request.Email, ct).ConfigureAwait(false))
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        await using var unitOfWork = new UnitOfWork(_connectionFactory);
        await unitOfWork.BeginAsync(ct).ConfigureAwait(false);
        try
        {
            var hashedPassword = _passwordHasher.Hash(request.Password);
            var cuentaId = await _cuentaRepository.CreateCuentaAsync(unitOfWork.Connection!, unitOfWork.Transaction, request.Email, hashedPassword, 1, ct).ConfigureAwait(false);

            var alumno = new Alumno
            {
                Matricula = request.Matricula,
                ID_Cuenta = cuentaId,
                Nombre = request.Nombre,
                Apaterno = request.Apaterno,
                Amaterno = request.Amaterno,
                F_Nac = request.FechaNacimiento,
                Genero = request.Genero,
                Correo = request.CorreoInstitucional,
                Carrera = request.Carrera
            };
            await _cuentaRepository.CreateAlumnoAsync(unitOfWork.Connection!, unitOfWork.Transaction, alumno, ct).ConfigureAwait(false);

            var perfil = new Perfil
            {
                ID_Cuenta = cuentaId,
                Nikname = string.IsNullOrWhiteSpace(request.Nikname) ? request.Nombre : request.Nikname!,
                Biografia = request.Biografia,
                Foto_Perfil = request.FotoPerfil,
                Fecha_Creacion = DateTime.UtcNow
            };
            var perfilId = await _perfilRepository.CreatePerfilAsync(unitOfWork.Connection!, unitOfWork.Transaction, perfil, ct).ConfigureAwait(false);

            var preferencias = new Preferencias
            {
                ID_Perfil = perfilId,
                Preferencia_Genero = request.PreferenciaGenero,
                Edad_Minima = request.EdadMinima,
                Edad_Maxima = request.EdadMaxima,
                Preferencia_Carrera = request.PreferenciaCarrera,
                Intereses = request.Intereses
            };
            await _perfilRepository.UpsertPreferenciasAsync(unitOfWork.Connection!, unitOfWork.Transaction, preferencias, ct).ConfigureAwait(false);

            await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
            return cuentaId;
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }
}
