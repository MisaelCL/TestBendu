using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Repositories;

namespace C_C.App.Services;

public class ChatService
{
    private readonly IMensajeRepository _mensajeRepository;
    private readonly IChatRepository _chatRepository;
    private readonly ConcurrentDictionary<Guid, List<DateTime>> _messageTracker = new();
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);
    private const int MaxMessagesPerWindow = 30;

    public ChatService(IMensajeRepository mensajeRepository, IChatRepository chatRepository)
    {
        _mensajeRepository = mensajeRepository;
        _chatRepository = chatRepository;
    }

    public async Task<IReadOnlyList<MensajeModel>> GetConversationAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return await _mensajeRepository.GetMessagesForChatAsync(chatId, cancellationToken);
    }

    public async Task<MensajeModel> SendMessageAsync(Guid chatId, Guid senderId, string content, CancellationToken cancellationToken = default)
    {
        EnforceRateLimit(senderId);
        var sanitized = content.Trim();
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            throw new InvalidOperationException("El mensaje no puede estar vacío");
        }

        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null || (chat.UserIdA != senderId && chat.UserIdB != senderId))
        {
            throw new InvalidOperationException("No tienes acceso a este chat");
        }

        var message = new MensajeModel
        {
            ChatId = chatId,
            RemitenteId = senderId,
            Contenido = sanitized,
            EnviadoEnUtc = DateTime.UtcNow,
            Leido = false
        };

        return await _mensajeRepository.AddAsync(message, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
    {
        var messages = await _mensajeRepository.GetMessagesForChatAsync(chatId, cancellationToken);
        foreach (var message in messages.Where(m => m.RemitenteId != userId && !m.Leido))
        {
            message.Leido = true;
            await _mensajeRepository.UpdateAsync(message, cancellationToken);
        }
    }

    private void EnforceRateLimit(Guid senderId)
    {
        var now = DateTime.UtcNow;
        var bucket = _messageTracker.GetOrAdd(senderId, _ => new List<DateTime>());
        lock (bucket)
        {
            bucket.RemoveAll(timestamp => now - timestamp > Window);
            if (bucket.Count >= MaxMessagesPerWindow)
            {
                throw new InvalidOperationException("Has superado el límite de mensajes por minuto. Intenta nuevamente en breve.");
            }

            bucket.Add(now);
        }
    }
}
