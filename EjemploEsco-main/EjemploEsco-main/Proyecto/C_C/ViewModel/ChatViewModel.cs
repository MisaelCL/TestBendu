using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using C_C.Model;
using C_C.Services;

namespace C_C.ViewModel
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly ChatService _chatService;
        private readonly ObservableCollection<MensajeModel> _mensajes = new ObservableCollection<MensajeModel>();

        private ChatModel _chatActual;
        private string _mensajeNuevo;

        public ChatViewModel()
        {
            var chatRepository = new Repositories.ChatRepository();
            var mensajeRepository = new Repositories.MensajeRepository();
            _chatService = new ChatService(chatRepository, mensajeRepository);
            EnviarMensajeCommand = new ViewModelCommand(ExecuteEnviarMensaje, CanExecuteEnviarMensaje);
        }

        public ChatModel ChatActual
        {
            get => _chatActual;
            set
            {
                _chatActual = value;
                OnPropertyChanged(nameof(ChatActual));
                CargarMensajes();
            }
        }

        public string MensajeNuevo
        {
            get => _mensajeNuevo;
            set
            {
                _mensajeNuevo = value;
                OnPropertyChanged(nameof(MensajeNuevo));
            }
        }

        public ObservableCollection<MensajeModel> Mensajes => _mensajes;

        public ICommand EnviarMensajeCommand { get; }

        private void CargarMensajes()
        {
            _mensajes.Clear();
            if (ChatActual == null)
            {
                return;
            }

            var mensajes = _chatService.ObtenerMensajes(ChatActual.Id);
            foreach (var mensaje in mensajes)
            {
                _mensajes.Add(mensaje);
            }
        }

        private bool CanExecuteEnviarMensaje(object parameter)
        {
            return ChatActual != null && !string.IsNullOrWhiteSpace(MensajeNuevo);
        }

        private void ExecuteEnviarMensaje(object parameter)
        {
            _chatService.EnviarMensaje(ChatActual.Id, UserSession.Instance.CurrentUserId, MensajeNuevo);
            MensajeNuevo = string.Empty;
            CargarMensajes();
        }
    }
}
