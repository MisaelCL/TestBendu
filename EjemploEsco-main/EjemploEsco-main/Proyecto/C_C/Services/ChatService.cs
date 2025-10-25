using System;
using System.Collections.Generic;
using C_C.Model;
using C_C.Repositories;

namespace C_C.Services
{
    public class ChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IMensajeRepository _mensajeRepository;

        public ChatService(IChatRepository chatRepository, IMensajeRepository mensajeRepository)
        {
            _chatRepository = chatRepository;
            _mensajeRepository = mensajeRepository;
        }

        public IEnumerable<ChatModel> ObtenerChats(Guid usuarioId)
        {
            return _chatRepository.GetChatsForUser(usuarioId);
        }

        public IEnumerable<MensajeModel> ObtenerMensajes(Guid chatId)
        {
            return _mensajeRepository.GetByChatId(chatId);
        }

        public void EnviarMensaje(Guid chatId, Guid remitenteId, string contenido)
        {
            var mensaje = new MensajeModel
            {
                ChatId = chatId,
                RemitenteId = remitenteId,
                Contenido = contenido
            };

            _mensajeRepository.Save(mensaje);
        }
    }
}
