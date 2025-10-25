using System.Collections.Generic;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace C_C.Model;

public interface IChatRepository
{
    Task<int> CrearChatPorMatchAsync(int ID_Match, CancellationToken ct = default);
    Task<(DateTime? LastAt, long? LastId)> ObtenerUltimoMensajeAsync(int ID_Chat, CancellationToken ct = default);
    Task<IEnumerable<(int ID_Chat, int ID_Match, DateTime Fecha_Creacion, DateTime? LastAt, long? LastId)>>
        ListarChatsPorPerfilAsync(int ID_Perfil, int top, CancellationToken ct = default);
    Task<(int PerfilA, int PerfilB, int ID_Match)?> ObtenerParticipantesAsync(int ID_Chat, CancellationToken ct = default);
    Task<int> ActualizarUltimoMensajeAsync(int ID_Chat, long mensajeId, DateTime fechaUtc, CancellationToken ct = default);
    Task<int> ActualizarUltimoMensajeAsync(int ID_Chat, long mensajeId, DateTime fechaUtc, SqlConnection connection, SqlTransaction? transaction, CancellationToken ct = default);
}
