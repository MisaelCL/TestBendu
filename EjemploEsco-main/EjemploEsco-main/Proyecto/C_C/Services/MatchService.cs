using System;
using System.Linq;
using C_C.Model;
using C_C.Repositories;

namespace C_C.Services
{
    public class MatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IChatRepository _chatRepository;

        public MatchService(IMatchRepository matchRepository, IChatRepository chatRepository)
        {
            _matchRepository = matchRepository;
            _chatRepository = chatRepository;
        }

        public MatchModel RegistrarLike(Guid userId, Guid targetUserId)
        {
            var existingMatch = _matchRepository.GetMatch(userId, targetUserId);
            if (existingMatch != null)
            {
                if (!existingMatch.Activo)
                {
                    existingMatch.Activo = true;
                    existingMatch.CreadoEl = DateTime.UtcNow;
                    _matchRepository.Save(existingMatch);
                }

                GarantizarChat(existingMatch);
                return existingMatch;
            }

            var match = new MatchModel
            {
                UsuarioAId = userId,
                UsuarioBId = targetUserId,
                CreadoEl = DateTime.UtcNow,
                Activo = true
            };

            _matchRepository.Save(match);
            GarantizarChat(match);
            return match;
        }

        private void GarantizarChat(MatchModel match)
        {
            var chat = _chatRepository.GetByMatchId(match.Id);
            if (chat != null)
            {
                return;
            }

            var nuevoChat = new ChatModel
            {
                MatchId = match.Id,
                UsuarioAId = match.UsuarioAId,
                UsuarioBId = match.UsuarioBId
            };

            _chatRepository.Save(nuevoChat);
        }
    }
}
