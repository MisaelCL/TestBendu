using System;
using System.Collections.Generic;
using C_C.Model;

namespace C_C.Repositories
{
    public interface IChatRepository
    {
        ChatModel GetByMatchId(Guid matchId);

        void Save(ChatModel chat);

        IEnumerable<ChatModel> GetChatsForUser(Guid userId);
    }
}
