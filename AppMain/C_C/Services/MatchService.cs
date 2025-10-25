using System;
using System.Threading;
using C_C.Model;
using Microsoft.Extensions.Logging;

namespace C_C.Services;

public sealed class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<MatchService> _logger;

    public MatchService(IMatchRepository matchRepository, ILogger<MatchService> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<int> CrearAsync(int Perfil_Emisor, int Perfil_Receptor, string Estado, CancellationToken ct = default)
    {
        var id = await _matchRepository.CrearAsync(Perfil_Emisor, Perfil_Receptor, Estado, ct).ConfigureAwait(false);
        _logger.LogInformation("Match {MatchId} creado", id);
        return id;
    }

    public async Task<int> ActualizarEstadoAsync(int ID_Match, string Estado, CancellationToken ct = default)
    {
        var rows = await _matchRepository.ActualizarEstadoAsync(ID_Match, Estado, ct).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException("No se pudo actualizar el match.");
        }

        return rows;
    }

    public Task<bool> ExisteParAsync(int a, int b, CancellationToken ct = default)
    {
        return _matchRepository.ExisteParAsync(a, b, ct);
    }
}
