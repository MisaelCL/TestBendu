using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;

namespace C_C.App.Repositories;

public interface IMensajeRepository : IRepositoryBase<MensajeModel>
{
    Task<IReadOnlyList<MensajeModel>> GetMessagesForChatAsync(Guid chatId, CancellationToken cancellationToken = default);
}
