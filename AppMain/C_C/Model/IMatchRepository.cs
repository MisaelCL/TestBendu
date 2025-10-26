using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace C_C_Final.Model
{
    public interface IMatchRepository
    {
        Task<Match> GetByIdAsync(int idMatch, CancellationToken ct = default);
        Task<bool> ExistsAsync(int idPerfilA, int idPerfilB, CancellationToken ct = default);
        Task<IReadOnlyList<Match>> ListByPerfilAsync(int idPerfil, int page, int pageSize, CancellationToken ct = default);

        Task<int> CreateMatchAsync(int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default);
        Task<bool> UpdateEstadoAsync(int idMatch, string nuevoEstado, CancellationToken ct = default);
        Task<bool> DeleteMatchAsync(int idMatch, CancellationToken ct = default);

        Task<int> EnsureChatForMatchAsync(int idMatch, CancellationToken ct = default);
        Task<Chat> GetChatByMatchIdAsync(int idMatch, CancellationToken ct = default);

        Task<long> AddMensajeAsync(int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default);
        Task<IReadOnlyList<Mensaje>> ListMensajesAsync(int idChat, int page, int pageSize, CancellationToken ct = default);

        Task<int> CreateMatchAsync(SqlConnection cn, SqlTransaction tx, int idPerfilEmisor, int idPerfilReceptor, string estado, CancellationToken ct = default);
        Task<int> EnsureChatForMatchAsync(SqlConnection cn, SqlTransaction tx, int idMatch, CancellationToken ct = default);
        Task<long> AddMensajeAsync(SqlConnection cn, SqlTransaction tx, int idChat, int idRemitentePerfil, string contenido, bool confirmacionLectura, CancellationToken ct = default);
    }
}
