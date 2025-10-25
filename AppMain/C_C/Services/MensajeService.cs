using System;
using System.Linq;
using System.Threading;
using C_C.Model;
using C_C.Resources.utils;
using Microsoft.Extensions.Logging;

namespace C_C.Services;

public sealed class MensajeService : IMensajeService
{
    private readonly IMensajeRepository _mensajeRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<MensajeService> _logger;

    public MensajeService(
        IMensajeRepository mensajeRepository,
        IChatRepository chatRepository,
        IConnectionFactory connectionFactory,
        ILogger<MensajeService> logger)
    {
        _mensajeRepository = mensajeRepository;
        _chatRepository = chatRepository;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<long> EnviarAsync(Mensaje msg, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(msg);

        var participantes = await _chatRepository.ObtenerParticipantesAsync(msg.ID_Chat, ct).ConfigureAwait(false);
        if (participantes is null)
        {
            _logger.LogWarning("No se encontró el chat {ChatId}", msg.ID_Chat);
            throw new InvalidOperationException("El chat no existe.");
        }

        if (msg.Remitente != participantes.Value.PerfilA && msg.Remitente != participantes.Value.PerfilB)
        {
            _logger.LogWarning("El perfil {PerfilId} intentó enviar mensaje a chat {ChatId} sin pertenecer", msg.Remitente, msg.ID_Chat);
            throw new InvalidOperationException("El remitente no pertenece al chat.");
        }

        msg.IsDeleted = false;
        msg.IsEdited = false;
        msg.Confirmacion_Lectura = false;
        msg.Fecha_Envio = DateTime.UtcNow;

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            var mensajeId = await _mensajeRepository.EnviarAsync(msg, connection, transaction, ct).ConfigureAwait(false);
            await _chatRepository.ActualizarUltimoMensajeAsync(msg.ID_Chat, mensajeId, msg.Fecha_Envio, connection, transaction, ct).ConfigureAwait(false);
            await transaction.CommitAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("Mensaje {MensajeId} enviado en chat {ChatId}", mensajeId, msg.ID_Chat);
            return mensajeId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct).ConfigureAwait(false);
            _logger.LogError(ex, "Error al enviar mensaje en chat {ChatId}", msg.ID_Chat);
            throw;
        }
    }

    public async Task EditarAsync(long ID_Mensaje, string nuevo, CancellationToken ct = default)
    {
        var rows = await _mensajeRepository.EditarContenidoAsync(ID_Mensaje, nuevo, ct).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException("No se pudo editar el mensaje solicitado.");
        }
    }

    public async Task BorrarLogicoAsync(long ID_Mensaje, CancellationToken ct = default)
    {
        var rows = await _mensajeRepository.BorradoLogicoAsync(ID_Mensaje, ct).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException("No se pudo eliminar el mensaje solicitado.");
        }
    }

    public async Task MarcarLeidoAsync(long ID_Mensaje, CancellationToken ct = default)
    {
        var rows = await _mensajeRepository.MarcarLeidoAsync(ID_Mensaje, ct).ConfigureAwait(false);
        if (rows == 0)
        {
            throw new InvalidOperationException("No se pudo marcar como leído el mensaje solicitado.");
        }
    }

    public async Task<IReadOnlyList<Mensaje>> ObtenerUltimosAsync(int ID_Chat, int top, CancellationToken ct = default)
    {
        var items = await _mensajeRepository.ObtenerPaginaAsync(ID_Chat, top, null, null, ct).ConfigureAwait(false);
        return items.OrderBy(m => m.Fecha_Envio).ThenBy(m => m.ID_Mensaje).ToList();
    }

    public async Task<IReadOnlyList<Mensaje>> ObtenerPaginaAnteriorAsync(int ID_Chat, int top, DateTime anchorFecha, long anchorId, CancellationToken ct = default)
    {
        var items = await _mensajeRepository.ObtenerPaginaAsync(ID_Chat, top, anchorFecha, anchorId, ct).ConfigureAwait(false);
        return items.OrderBy(m => m.Fecha_Envio).ThenBy(m => m.ID_Mensaje).ToList();
    }
}
