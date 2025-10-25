using System;
using System.Threading;
using System.Threading.Tasks;
using C_C_Final.Application.Repositories;
using C_C_Final.Infrastructure.Data;

namespace C_C_Final.Application.Services
{
    public sealed class MatchService
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
            using var unitOfWork = await UnitOfWork.CreateAsync(_connectionFactory, ct).ConfigureAwait(false);
            try
            {
                var matchId = await _matchRepository.CreateMatchAsync(unitOfWork.Connection, unitOfWork.Transaction, idPerfilEmisor, idPerfilReceptor, estado, ct).ConfigureAwait(false);
                await _matchRepository.EnsureChatForMatchAsync(unitOfWork.Connection, unitOfWork.Transaction, matchId, ct).ConfigureAwait(false);
                await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
                return matchId;
            }
            catch
            {
                await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<int> EnsureChatForMatchAsync(int idMatch, CancellationToken ct = default)
        {
            using var unitOfWork = await UnitOfWork.CreateAsync(_connectionFactory, ct).ConfigureAwait(false);
            try
            {
                var chatId = await _matchRepository.EnsureChatForMatchAsync(unitOfWork.Connection, unitOfWork.Transaction, idMatch, ct).ConfigureAwait(false);
                await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
                return chatId;
            }
            catch
            {
                await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }

        public async Task<long> SendMessageAsync(int idChat, int idRemitentePerfil, string contenido, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                throw new ArgumentException("El contenido del mensaje es obligatorio", nameof(contenido));
            }

            using var unitOfWork = await UnitOfWork.CreateAsync(_connectionFactory, ct).ConfigureAwait(false);
            try
            {
                var mensajeId = await _matchRepository.AddMensajeAsync(unitOfWork.Connection, unitOfWork.Transaction, idChat, idRemitentePerfil, contenido, false, ct).ConfigureAwait(false);
                await unitOfWork.CommitAsync(ct).ConfigureAwait(false);
                return mensajeId;
            }
            catch
            {
                await unitOfWork.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}
