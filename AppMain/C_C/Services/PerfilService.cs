using System;
using System.Threading;
using C_C.Model;
using Microsoft.Extensions.Logging;

namespace C_C.Services;

public sealed class PerfilService : IPerfilService
{
    private readonly IPerfilRepository _perfilRepository;
    private readonly ILogger<PerfilService> _logger;

    public PerfilService(IPerfilRepository perfilRepository, ILogger<PerfilService> logger)
    {
        _perfilRepository = perfilRepository;
        _logger = logger;
    }

    public Task<Perfil?> GetAsync(int ID_Perfil, CancellationToken ct = default)
    {
        return _perfilRepository.GetAsync(ID_Perfil, ct);
    }

    public Task<Perfil?> GetByNickAsync(string nik, CancellationToken ct = default)
    {
        return _perfilRepository.GetByNickAsync(nik, ct);
    }

    public async Task<int> UpdateAsync(Perfil p, CancellationToken ct = default)
    {
        var rows = await _perfilRepository.UpdateAsync(p, ct).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException("No se pudo actualizar el perfil.");
        }

        _logger.LogInformation("Perfil {PerfilId} actualizado", p.ID_Perfil);
        return rows;
    }
}
