using System;
using System.Collections.Generic;
using System.Linq;
using C_C.Model;

namespace C_C.Repositories
{
    public class MensajeRepository : RepositoryBase, IMensajeRepository
    {
        private static readonly List<MensajeModel> Mensajes = new List<MensajeModel>();

        public IEnumerable<MensajeModel> GetByChatId(Guid chatId)
        {
            return Mensajes
                .Where(mensaje => mensaje.ChatId == chatId)
                .OrderBy(mensaje => mensaje.EnviadoEl);
        }

        public void Save(MensajeModel mensaje)
        {
            if (mensaje.Id == Guid.Empty)
            {
                mensaje.Id = Guid.NewGuid();
            }

            mensaje.EnviadoEl = mensaje.EnviadoEl == default ? DateTime.UtcNow : mensaje.EnviadoEl;
            Mensajes.Add(mensaje);
        }
    }
}
