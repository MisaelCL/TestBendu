using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using Microsoft.EntityFrameworkCore;

namespace C_C.App.Repositories;

public class MensajeRepository : RepositoryBase<MensajeModel>, IMensajeRepository
{
    public MensajeRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<IReadOnlyList<MensajeModel>> GetMessagesForChatAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return await Context.Mensajes
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.EnviadoEnUtc)
            .ToListAsync(cancellationToken);
    }
}
