using System;
using System.Data.SqlClient;
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.ViewModel;

namespace C_C_Final.Services
{
    /// <summary>
    /// Expone operaciones de alto nivel para administrar emparejamientos y mensajes.
    /// </summary>
    public sealed class MatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly string _connectionString;

        /// <summary>
        ///     Inicializa el servicio con el repositorio que conoce los detalles de persistencia.
        /// </summary>
        /// <param name="matchRepository">Repositorio especializado en matches, chats y mensajes.</param>
        /// <param name="connectionString">Cadena de conexión opcional.</param>
        public MatchService(IMatchRepository matchRepository, string connectionString = null)
        {
            _matchRepository = matchRepository;
            _connectionString = RepositoryBase.ResolverCadenaConexion(connectionString);
        }

        /// <summary>
        /// Crea un nuevo emparejamiento (SIN chat).
        /// El chat debe asegurarse por separado cuando el match esté 'aceptado'.
        /// </summary>
        public int CrearMatch(int idPerfilEmisor, int idPerfilReceptor, string estado)
        {
            var estadoNormalizado = MatchEstadoHelper.NormalizarParaPersistencia(estado);

            if (string.IsNullOrEmpty(estadoNormalizado))
            {
                estadoNormalizado = MatchEstadoHelper.ConstruirPendiente();
            }

            // Se inicia una transacción corta ya que solo se crea el match (sin chat).
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                // Solo creamos el match
                var matchId = _matchRepository.CrearMatch(connection, transaction, idPerfilEmisor, idPerfilReceptor, estadoNormalizado);

                // --- CORRECCIÓN ---
                // Se elimina la llamada a AsegurarChatParaMatch.
                // El chat SÓLO debe crearse cuando el match sea "aceptado",
                // no cuando esté "pendiente".
                //
                // LÍNEA ELIMINADA:
                // _matchRepository.AsegurarChatParaMatch(connection, transaction, matchId);
                
                // Al finalizar se confirma el cambio. El chat se generará posteriormente cuando ambos acepten.
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
            // Este método se invoca exclusivamente cuando el match ya es mutuo.
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

            // Cada envío de mensaje se encapsula en su propia transacción para asegurar que la actualización
            // del chat (last message) se ejecute junto con la inserción del mensaje.
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
