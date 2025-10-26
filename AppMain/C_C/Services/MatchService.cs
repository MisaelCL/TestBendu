using System;
using C_C_Final.Model;
using C_C_Final.Repositories;

namespace C_C_Final.Services
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

        public int CreateMatch(int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            using var unitOfWork = UnitOfWork.Create(_connectionFactory);
            try
            {
                var matchId = _matchRepository.CreateMatch(unitOfWork.Connection, unitOfWork.Transaction, idPerfilEmisor, idPerfilReceptor, estado);
                _matchRepository.EnsureChatForMatch(unitOfWork.Connection, unitOfWork.Transaction, matchId);
                unitOfWork.Commit();
                return matchId;
            }
            catch
            {
                unitOfWork.Rollback();
                throw;
            }
        }

        public int EnsureChatForMatch(int idMatch)
        {
            using var unitOfWork = UnitOfWork.Create(_connectionFactory);
            try
            {
                var chatId = _matchRepository.EnsureChatForMatch(unitOfWork.Connection, unitOfWork.Transaction, idMatch);
                unitOfWork.Commit();
                return chatId;
            }
            catch
            {
                unitOfWork.Rollback();
                throw;
            }
        }

        public long SendMessage(int idChat, int idRemitentePerfil, string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                throw new ArgumentException("El contenido del mensaje es obligatorio", nameof(contenido));
            }

            using var unitOfWork = UnitOfWork.Create(_connectionFactory);
            try
            {
                var mensajeId = _matchRepository.AddMensaje(unitOfWork.Connection, unitOfWork.Transaction, idChat, idRemitentePerfil, contenido, false);
                unitOfWork.Commit();
                return mensajeId;
            }
            catch
            {
                unitOfWork.Rollback();
                throw;
            }
        }
    }
}
