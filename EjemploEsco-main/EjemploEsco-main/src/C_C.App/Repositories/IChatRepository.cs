using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;

namespace C_C.App.Repositories;

public interface IChatRepository : IRepositoryBase<ChatModel>
{
    Task<ChatModel?> GetChatBetweenUsersAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChatModel>> GetChatsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
