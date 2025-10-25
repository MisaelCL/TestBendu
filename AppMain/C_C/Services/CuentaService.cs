using System;
using System.Threading;
using C_C.Model;
using C_C.Resources.utils;
using Microsoft.Extensions.Logging;

namespace C_C.Services;

public sealed class CuentaService : ICuentaService
{
    private readonly ICuentaRepository _cuentaRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CuentaService> _logger;

    public CuentaService(ICuentaRepository cuentaRepository, IPasswordHasher passwordHasher, ILogger<CuentaService> logger)
    {
        _cuentaRepository = cuentaRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public Task<Cuenta?> ObtenerPorEmailAsync(string email, CancellationToken ct = default)
    {
        return _cuentaRepository.GetByEmailAsync(email, ct);
    }

    public async Task<int> RegistrarAsync(Cuenta cuenta, string plainPassword, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cuenta);
        if (string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new ArgumentException("La contraseña es obligatoria", nameof(plainPassword));
        }

        cuenta.PasswordHash = _passwordHasher.Hash(plainPassword);
        cuenta.Fecha_Registro = DateTime.UtcNow;
        cuenta.Ultimo_Acceso = cuenta.Ultimo_Acceso ?? cuenta.Fecha_Registro;
        var id = await _cuentaRepository.InsertAsync(cuenta, ct).ConfigureAwait(false);
        _logger.LogInformation("Cuenta {CuentaId} registrada", id);
        return id;
    }
}
