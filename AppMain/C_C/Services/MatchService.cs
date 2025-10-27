using System;
using System.Data.SqlClient;
using C_C_Final.Model;
using C_C_Final.Repositories;

namespace C_C_Final.Services
{
    public sealed class MatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly string _connectionString;

        public MatchService(IMatchRepository matchRepository, string connectionString = null)
        {
            _matchRepository = matchRepository;
            _connectionString = RepositoryBase.ResolveConnectionString(connectionString);
        }

        public int CreateMatch(int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var matchId = _matchRepository.CreateMatch(connection, transaction, idPerfilEmisor, idPerfilReceptor, estado);
                _matchRepository.EnsureChatForMatch(connection, transaction, matchId);
                transaction.Commit();
                return matchId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public int EnsureChatForMatch(int idMatch)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var chatId = _matchRepository.EnsureChatForMatch(connection, transaction, idMatch);
                transaction.Commit();
                return chatId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public long SendMessage(int idChat, int idRemitentePerfil, string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
            {
                throw new ArgumentException("El contenido del mensaje es obligatorio", nameof(contenido));
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var mensajeId = _matchRepository.AddMensaje(connection, transaction, idChat, idRemitentePerfil, contenido, false);
                transaction.Commit();
                return mensajeId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
