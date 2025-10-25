using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;

namespace C_C.App.Repositories;

public interface IMatchRepository : IRepositoryBase<MatchModel>
{
    Task<MatchModel?> FindExistingAsync(Guid userIdA, Guid userIdB, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MatchModel>> GetMatchesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
