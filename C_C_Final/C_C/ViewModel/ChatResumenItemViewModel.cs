using System.Windows.Media;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Representa un chat en la lista de conversaciones del perfil.
    /// </summary>
    public sealed class ChatResumenItemViewModel : BaseViewModel
    {
        private int _matchId;
        private int _chatId;
        private string _nombreContacto = string.Empty;
        private ImageSource? _fotoPerfil;
        private string _ultimoMensaje = string.Empty;
        private bool _isOnline;
        private int _mensajesNoLeidos;

        public int MatchId
        {
            get => _matchId;
            set => EstablecerPropiedad(ref _matchId, value);
        }

        public int ChatId
        {
            get => _chatId;
            set => EstablecerPropiedad(ref _chatId, value);
        }

        public string NombreContacto
        {
            get => _nombreContacto;
            set => EstablecerPropiedad(ref _nombreContacto, value);
        }

        public ImageSource? FotoPerfil
        {
            get => _fotoPerfil;
            set => EstablecerPropiedad(ref _fotoPerfil, value);
        }

        public string UltimoMensaje
        {
            get => _ultimoMensaje;
            set => EstablecerPropiedad(ref _ultimoMensaje, value);
        }

        public bool IsOnline
        {
            get => _isOnline;
            set => EstablecerPropiedad(ref _isOnline, value);
        }

        public int MensajesNoLeidos
        {
            get => _mensajesNoLeidos;
            set => EstablecerPropiedad(ref _mensajesNoLeidos, value);
        }
    }
}
