using C_C.Model;

namespace C_C.Services;

public interface IChatService
{
    Task<int> CrearChatPorMatchAsync(int ID_Match, CancellationToken ct = default);
    Task<(DateTime? LastAt, long? LastId)> ObtenerUltimoMensajeAsync(int ID_Chat, CancellationToken ct = default);
    Task<IReadOnlyList<Chat>> ListarChatsPorPerfilAsync(int ID_Perfil, int top, CancellationToken ct = default);
}
