using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using Microsoft.EntityFrameworkCore;

namespace C_C.App.Repositories;

public class MatchRepository : RepositoryBase<MatchModel>, IMatchRepository
{
    public MatchRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<MatchModel?> FindExistingAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default)
    {
        var ordered = Normalize(userIdA, userIdB);
        return Context.Matches
            .Include(m => m.Chat)
            .ThenInclude(c => c!.Mensajes)
            .FirstOrDefaultAsync(m => m.UserIdA == ordered.A && m.UserIdB == ordered.B, cancellationToken);
    }

    public async Task<IReadOnlyList<MatchModel>> GetMatchesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.Matches
            .Include(m => m.Chat)
            .ThenInclude(c => c!.Mensajes)
            .Where(m => m.UserIdA == userId || m.UserIdB == userId)
            .ToListAsync(cancellationToken);
    }

    public override async Task<MatchModel> AddAsync(MatchModel entity, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(entity.UserIdA, entity.UserIdB);
        entity.UserIdA = normalized.A;
        entity.UserIdB = normalized.B;
        return await base.AddAsync(entity, cancellationToken);
    }

    public override async Task UpdateAsync(MatchModel entity, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(entity.UserIdA, entity.UserIdB);
        entity.UserIdA = normalized.A;
        entity.UserIdB = normalized.B;
        await base.UpdateAsync(entity, cancellationToken);
    }

    private static (Guid A, Guid B) Normalize(Guid first, Guid second)
    {
        return first.CompareTo(second) <= 0 ? (first, second) : (second, first);
    }
}
