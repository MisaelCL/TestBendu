using C_C.Application.Repositories;
using C_C.Infrastructure.Common;

namespace C_C.Application.Services;

public interface IMatchService
{
    Task<int> CreateMatchAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default);
    Task<long> SendMessageAsync(int idMatch, int idRemitentePerfil, string contenido, bool confirmarLectura, CancellationToken ct = default);
    Task<(int MatchId, int ChatId, long? FirstMessageId)> CreateMatchWithFirstMessageAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, string? contenidoInicial, int remitenteInicial, CancellationToken ct = default);
}

public sealed class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRepository;
    private readonly SqlConnectionFactory _connectionFactory;

    public MatchService(IMatchRepository matchRepository, SqlConnectionFactory connectionFactory)
    {
        _matchRepository = matchRepository;
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateMatchAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default)
    {
        await using var unitOfWork = new UnitOfWork(_connectionFactory);
        await unitOfWork.BeginAsync(ct).ConfigureAwait(false);
        try
        {
            var matchId = await _matchRepository.CreateMatchAsync(unitOfWork.Connection!, unitOfWork.Transaction, idPerfilEmisor, idPerfilReceptor, estado, ct).ConfigureAwait(false);
            await _matchRepository.EnsureChatForMatchAsync(unitOfWork.Connection!, unitOfWork.Transaction, matchId, ct).ConfigureAwait(false);
            await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
            return matchId;
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<long> SendMessageAsync(int idMatch, int idRemitentePerfil, string contenido, bool confirmarLectura, CancellationToken ct = default)
    {
        await using var unitOfWork = new UnitOfWork(_connectionFactory);
        await unitOfWork.BeginAsync(ct).ConfigureAwait(false);
        try
        {
            var chatId = await _matchRepository.EnsureChatForMatchAsync(unitOfWork.Connection!, unitOfWork.Transaction, idMatch, ct).ConfigureAwait(false);
            var messageId = await _matchRepository.AddMensajeAsync(unitOfWork.Connection!, unitOfWork.Transaction, chatId, idRemitentePerfil, contenido, confirmarLectura, ct).ConfigureAwait(false);
            await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
            return messageId;
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }

    public async Task<(int MatchId, int ChatId, long? FirstMessageId)> CreateMatchWithFirstMessageAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, string? contenidoInicial, int remitenteInicial, CancellationToken ct = default)
    {
        await using var unitOfWork = new UnitOfWork(_connectionFactory);
        await unitOfWork.BeginAsync(ct).ConfigureAwait(false);
        try
        {
            var matchId = await _matchRepository.CreateMatchAsync(unitOfWork.Connection!, unitOfWork.Transaction, idPerfilEmisor, idPerfilReceptor, estado, ct).ConfigureAwait(false);
            var chatId = await _matchRepository.EnsureChatForMatchAsync(unitOfWork.Connection!, unitOfWork.Transaction, matchId, ct).ConfigureAwait(false);
            long? messageId = null;
            if (!string.IsNullOrWhiteSpace(contenidoInicial))
            {
                messageId = await _matchRepository.AddMensajeAsync(unitOfWork.Connection!, unitOfWork.Transaction, chatId, remitenteInicial, contenidoInicial, false, ct).ConfigureAwait(false);
            }

            await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
            return (matchId, chatId, messageId);
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }
    }
}
