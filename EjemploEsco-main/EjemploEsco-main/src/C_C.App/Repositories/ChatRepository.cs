using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using Microsoft.EntityFrameworkCore;

namespace C_C.App.Repositories;

public class ChatRepository : RepositoryBase<ChatModel>, IChatRepository
{
    public ChatRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<ChatModel?> GetChatBetweenUsersAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(userIdA, userIdB);
        return Context.Chats
            .Include(c => c.Mensajes)
            .FirstOrDefaultAsync(c => (c.UserIdA == normalized.A && c.UserIdB == normalized.B) || (c.UserIdA == normalized.B && c.UserIdB == normalized.A), cancellationToken);
    }

    public async Task<IReadOnlyList<ChatModel>> GetChatsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Chats
            .Include(c => c.Mensajes)
            .Where(c => c.UserIdA == userId || c.UserIdB == userId)
            .ToListAsync(cancellationToken);
    }

    private static (Guid A, Guid B) Normalize(Guid first, Guid second)
    {
        return first.CompareTo(second) <= 0 ? (first, second) : (second, first);
    }
}
