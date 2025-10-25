using System;
using System.Collections.Generic;
using System.Linq;
using C_C.Model;

namespace C_C.Repositories
{
    public class MatchRepository : RepositoryBase, IMatchRepository
    {
        private static readonly List<MatchModel> Matches = new List<MatchModel>();

        public IEnumerable<MatchModel> GetMatchesForUser(Guid userId)
        {
            return Matches.Where(match => match.UsuarioAId == userId || match.UsuarioBId == userId);
        }

        public MatchModel GetMatch(Guid usuarioAId, Guid usuarioBId)
        {
            return Matches.FirstOrDefault(match =>
                (match.UsuarioAId == usuarioAId && match.UsuarioBId == usuarioBId)
                || (match.UsuarioAId == usuarioBId && match.UsuarioBId == usuarioAId));
        }

        public void Save(MatchModel match)
        {
            if (match.Id == Guid.Empty)
            {
                match.Id = Guid.NewGuid();
            }

            var existing = GetMatch(match.UsuarioAId, match.UsuarioBId);
            if (existing == null)
            {
                match.CreadoEl = DateTime.UtcNow;
                match.Activo = true;
                Matches.Add(match);
                return;
            }

            existing.Activo = match.Activo;
            existing.CreadoEl = match.CreadoEl;
        }
    }
}
