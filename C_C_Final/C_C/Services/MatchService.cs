using System;
using System.Data.SqlClient;
using C_C_Final.Model;
using C_C_Final.Repositories;

namespace C_C_Final.Services
{
    /// <summary>
    /// Expone operaciones de alto nivel para administrar emparejamientos y mensajes.
    /// </summary>
    public sealed class MatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly string _connectionString;

        public MatchService(IMatchRepository matchRepository, string connectionString = null)
        {
            _matchRepository = matchRepository;
            _connectionString = RepositoryBase.ResolverCadenaConexion(connectionString);
        }

        /// <summary>
        /// Crea un nuevo emparejamiento (SIN chat).
        /// El chat debe asegurarse por separado cuando el match esté 'activo'.
        /// </summary>
        public int CrearMatch(int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Solo creamos el match
                var matchId = _matchRepository.CrearMatch(connection, transaction, idPerfilEmisor, idPerfilReceptor, estado);

                // --- CORRECCIÓN ---
                // Se elimina la llamada a AsegurarChatParaMatch.
                // El chat SÓLO debe crearse cuando el match sea "activo",
                // no cuando esté "pendiente".
                //
                // LÍNEA ELIMINADA:
                // _matchRepository.AsegurarChatParaMatch(connection, transaction, matchId);
                
                transaction.Commit();
                return matchId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Garantiza la existencia de un chat para un emparejamiento existente.
        /// (Este método es correcto y se llama desde el HomeViewModel cuando hay match)
        /// </summary>
        public int AsegurarChatParaMatch(int idMatch)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var chatId = _matchRepository.AsegurarChatParaMatch(connection, transaction, idMatch);
                transaction.Commit();
                return chatId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Envía un nuevo mensaje dentro de un chat existente.
        /// </summary>
        public long EnviarMensaje(int idChat, int idRemitentePerfil, string contenido)
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
                var mensajeId = _matchRepository.AgregarMensaje(connection, transaction, idChat, idRemitentePerfil, contenido, false);
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
