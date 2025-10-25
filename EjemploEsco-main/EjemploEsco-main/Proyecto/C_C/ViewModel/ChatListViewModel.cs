using System;
using System.Collections.ObjectModel;
using C_C.Model;
using C_C.Services;

namespace C_C.ViewModel
{
    public class ChatListViewModel : ViewModelBase
    {
        private readonly ChatService _chatService;
        private readonly ObservableCollection<ChatModel> _chats = new ObservableCollection<ChatModel>();
        private ChatModel _chatSeleccionado;

        public ChatListViewModel()
        {
            var chatRepository = new Repositories.ChatRepository();
            var mensajeRepository = new Repositories.MensajeRepository();
            _chatService = new ChatService(chatRepository, mensajeRepository);
            CargarChats();
        }

        public ObservableCollection<ChatModel> Chats => _chats;

        public ChatModel ChatSeleccionado
        {
            get => _chatSeleccionado;
            set
            {
                _chatSeleccionado = value;
                OnPropertyChanged(nameof(ChatSeleccionado));
            }
        }

        private void CargarChats()
        {
            _chats.Clear();
            var chats = _chatService.ObtenerChats(UserSession.Instance.CurrentUserId);
            foreach (var chat in chats)
            {
                _chats.Add(chat);
            }
        }
    }
}
