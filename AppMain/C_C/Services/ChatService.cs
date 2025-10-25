using System.Linq;
using System.Threading;
using C_C.Model;
using Microsoft.Extensions.Logging;

namespace C_C.Services;

public sealed class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IChatRepository chatRepository, ILogger<ChatService> logger)
    {
        _chatRepository = chatRepository;
        _logger = logger;
    }

    public Task<int> CrearChatPorMatchAsync(int ID_Match, CancellationToken ct = default)
    {
        return _chatRepository.CrearChatPorMatchAsync(ID_Match, ct);
    }

    public Task<(DateTime? LastAt, long? LastId)> ObtenerUltimoMensajeAsync(int ID_Chat, CancellationToken ct = default)
    {
        return _chatRepository.ObtenerUltimoMensajeAsync(ID_Chat, ct);
    }

    public async Task<IReadOnlyList<Chat>> ListarChatsPorPerfilAsync(int ID_Perfil, int top, CancellationToken ct = default)
    {
        var registros = await _chatRepository.ListarChatsPorPerfilAsync(ID_Perfil, top, ct).ConfigureAwait(false);
        var chats = registros.Select(r => new Chat
        {
            ID_Chat = r.ID_Chat,
            ID_Match = r.ID_Match,
            Fecha_Creacion = r.Fecha_Creacion,
            LastMessageAtUtc = r.LastAt,
            LastMessageId = r.LastId
        }).ToList();
        _logger.LogInformation("Se cargaron {Count} chats para el perfil {PerfilId}", chats.Count, ID_Perfil);
        return chats;
    }
}
