using System;
using System.Collections.Generic;
using C_C.Model;

namespace C_C.Repositories
{
    public interface IMatchRepository
    {
        MatchModel GetMatch(Guid usuarioAId, Guid usuarioBId);

        void Save(MatchModel match);

        IEnumerable<MatchModel> GetMatchesForUser(Guid userId);
    }
}
