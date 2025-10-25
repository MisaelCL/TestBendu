using System;
using System.Collections.Generic;
using C_C.Model;

namespace C_C.Repositories
{
    public interface IMensajeRepository
    {
        IEnumerable<MensajeModel> GetByChatId(Guid chatId);

        void Save(MensajeModel mensaje);
    }
}
