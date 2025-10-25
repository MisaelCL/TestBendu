using System;
using System.Collections.Generic;
using System.Linq;
using C_C.Model;

namespace C_C.Repositories
{
    public class ChatRepository : RepositoryBase, IChatRepository
    {
        private static readonly List<ChatModel> Chats = new List<ChatModel>();

        public ChatModel GetByMatchId(Guid matchId)
        {
            return Chats.FirstOrDefault(chat => chat.MatchId == matchId);
        }

        public IEnumerable<ChatModel> GetChatsForUser(Guid userId)
        {
            return Chats.Where(chat => chat.UsuarioAId == userId || chat.UsuarioBId == userId);
        }

        public void Save(ChatModel chat)
        {
            if (chat.Id == Guid.Empty)
            {
                chat.Id = Guid.NewGuid();
                Chats.Add(chat);
                return;
            }

            var existing = Chats.FirstOrDefault(c => c.Id == chat.Id);
            if (existing == null)
            {
                Chats.Add(chat);
                return;
            }

            existing.MatchId = chat.MatchId;
            existing.UsuarioAId = chat.UsuarioAId;
            existing.UsuarioBId = chat.UsuarioBId;
        }
    }
}
