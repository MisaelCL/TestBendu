using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Model;
using C_C_Final.Services;

namespace C_C_Final.ViewModel
{
    public sealed class ChatViewModel : BaseViewModel
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPerfilRepository _perfilRepository;
        private readonly MatchService _matchService;
        private readonly ObservableCollection<MensajeItemViewModel> _mensajes = new ObservableCollection<MensajeItemViewModel>();
        private ImageSource _contactoAvatarUrl;
        private string _contactoNombre = string.Empty;
        private string _nuevoMensaje = string.Empty;
        private bool _isChatMenuOpen;
        private bool _isThreadMenuOpen;
        private int _matchId;
        private int _chatId;
        private int _perfilActualId;

        public ChatViewModel(IMatchRepository matchRepository, IPerfilRepository perfilRepository, MatchService matchService)
        {
            _matchRepository = matchRepository;
            _perfilRepository = perfilRepository;
            _matchService = matchService;

            GoBackCommand = new RelayCommand(_ => CloseWindow());
            OpenMenuCommand = new RelayCommand(_ => IsChatMenuOpen = !IsChatMenuOpen);
            OpenThreadMenuCommand = new RelayCommand(_ => IsThreadMenuOpen = !IsThreadMenuOpen);
            EnviarMensajeCommand = new RelayCommand(_ => EnviarMensaje());
            VerPerfilCommand = new RelayCommand(_ => MessageBox.Show("Abrir perfil del contacto", "Chat", MessageBoxButton.OK, MessageBoxImage.Information));
            BloquearContactoCommand = new RelayCommand(_ => BloquearContacto());
            EliminarChatCommand = new RelayCommand(_ => EliminarChat());
            SilenciarChatCommand = new RelayCommand(_ => MessageBox.Show("Chat silenciado", "Chat", MessageBoxButton.OK, MessageBoxImage.Information));
            MarcarNoLeidoCommand = new RelayCommand(_ => MessageBox.Show("Chat marcado como no leído", "Chat", MessageBoxButton.OK, MessageBoxImage.Information));
        }

        public ObservableCollection<MensajeItemViewModel> Mensajes => _mensajes;

        public ImageSource ContactoAvatarUrl
        {
            get => _contactoAvatarUrl;
            private set => SetProperty(ref _contactoAvatarUrl, value);
        }

        public string ContactoNombre
        {
            get => _contactoNombre;
            private set => SetProperty(ref _contactoNombre, value);
        }

        public string NuevoMensaje
        {
            get => _nuevoMensaje;
            set => SetProperty(ref _nuevoMensaje, value);
        }

        public bool IsChatMenuOpen
        {
            get => _isChatMenuOpen;
            set => SetProperty(ref _isChatMenuOpen, value);
        }

        public bool IsThreadMenuOpen
        {
            get => _isThreadMenuOpen;
            set => SetProperty(ref _isThreadMenuOpen, value);
        }

        public ICommand GoBackCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand OpenThreadMenuCommand { get; }
        public ICommand EnviarMensajeCommand { get; }
        public ICommand VerPerfilCommand { get; }
        public ICommand BloquearContactoCommand { get; }
        public ICommand EliminarChatCommand { get; }
        public ICommand SilenciarChatCommand { get; }
        public ICommand MarcarNoLeidoCommand { get; }

        public void Load(int matchId, int perfilActualId)
        {
            _matchId = matchId;
            _perfilActualId = perfilActualId;
            Mensajes.Clear();

            var match = _matchRepository.GetById(matchId);
            if (match == null)
            {
                throw new InvalidOperationException("El match especificado no existe");
            }

            var contactoId = match.PerfilEmisor == perfilActualId ? match.PerfilReceptor : match.PerfilEmisor;
            var perfilContacto = _perfilRepository.GetById(contactoId);
            if (perfilContacto != null)
            {
                ContactoNombre = perfilContacto.Nikname;
                ContactoAvatarUrl = ConvertToImage(perfilContacto.FotoPerfil);
            }

            var chat = _matchRepository.GetChatByMatchId(matchId) ?? new Chat();
            if (chat.IdChat == 0)
            {
                var chatId = _matchRepository.EnsureChatForMatch(matchId);
                chat = _matchRepository.GetChatByMatchId(matchId) ?? new Chat { IdChat = chatId };
            }

            _chatId = chat.IdChat;

            var mensajes = _matchRepository.ListMensajes(_chatId, 0, 50);
            foreach (var mensaje in mensajes.OrderBy(m => m.FechaEnvio))
            {
                Mensajes.Add(MapMensaje(mensaje));
            }
        }

        private void EnviarMensaje()
        {
            if (string.IsNullOrWhiteSpace(NuevoMensaje) || _chatId == 0)
            {
                return;
            }

            var contenido = NuevoMensaje.Trim();
            NuevoMensaje = string.Empty;
            var mensajeId = _matchService.SendMessage(_chatId, _perfilActualId, contenido);
            var mensaje = new Mensaje
            {
                IdMensaje = mensajeId,
                IdChat = _chatId,
                IdRemitentePerfil = _perfilActualId,
                Contenido = contenido,
                FechaEnvio = DateTime.UtcNow,
                ConfirmacionLectura = false,
                IsEdited = false,
                IsDeleted = false
            };
            Mensajes.Add(MapMensaje(mensaje));
        }

        private void BloquearContacto()
        {
            if (_matchId == 0)
            {
                return;
            }

            _matchRepository.UpdateEstado(_matchId, "roto");
            MessageBox.Show("El contacto fue bloqueado", "Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EliminarChat()
        {
            if (_matchId == 0)
            {
                return;
            }

            _matchRepository.DeleteMatch(_matchId);
            Application.Current?.Windows[Application.Current.Windows.Count - 1]?.Close();
        }

        private void CloseWindow()
        {
            Application.Current?.Windows[Application.Current.Windows.Count - 1]?.Close();
        }

        private MensajeItemViewModel MapMensaje(Mensaje mensaje)
        {
            var esPropio = mensaje.IdRemitentePerfil == _perfilActualId;
            return new MensajeItemViewModel
            {
                MensajeId = mensaje.IdMensaje,
                Contenido = mensaje.Contenido,
                AvatarColumn = esPropio ? 2 : 0,
                BubbleColumn = 1,
                BubbleAlign = esPropio ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                BubbleColor = esPropio ? (Brush)new SolidColorBrush(Color.FromRgb(5, 175, 198)) : Brushes.White,
                AvatarUrl = esPropio ? null : ContactoAvatarUrl
            };
        }

        private static ImageSource ConvertToImage(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }

    public sealed class MensajeItemViewModel : BaseViewModel
    {
        public long MensajeId { get; set; }
        public string Contenido { get; set; } = string.Empty;
        public int AvatarColumn { get; set; }
        public int BubbleColumn { get; set; }
        public HorizontalAlignment BubbleAlign { get; set; } = HorizontalAlignment.Left;
        public Brush BubbleColor { get; set; } = Brushes.White;
        public ImageSource AvatarUrl { get; set; }
    }
}
