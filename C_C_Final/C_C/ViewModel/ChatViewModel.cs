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
    /// <summary>
    /// Coordina la visualización y acciones dentro de una conversación de chat.
    /// </summary>
    public sealed class ChatViewModel : BaseViewModel
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPerfilRepository _perfilRepository;
        private readonly MatchService _matchService;
        private readonly ObservableCollection<MensajeItemViewModel> _mensajes = new ObservableCollection<MensajeItemViewModel>();
        private ImageSource? _contactoAvatarUrl;
        private string _contactoNombre = string.Empty;
        private string _nuevoMensaje = string.Empty;
        private bool _isChatMenuOpen;
        private bool _isThreadMenuOpen;
        private int _matchId;
        private int _chatId;
        private int _perfilActualId;

        /// <summary>
        ///     Inicializa el chat con las dependencias necesarias para consultar el historial y enviar mensajes.
        /// </summary>
        public ChatViewModel(IMatchRepository matchRepository, IPerfilRepository perfilRepository, MatchService matchService)
        {
            _matchRepository = matchRepository;
            _perfilRepository = perfilRepository;
            _matchService = matchService;

            // Comandos principales de la conversación. Se enlazan directamente con la interfaz XAML.
            ComandoRegresar = new RelayCommand(_ => CerrarVentana());
            ComandoAlternarMenuChat = new RelayCommand(_ => IsChatMenuOpen = !IsChatMenuOpen);
            ComandoAlternarMenuConversacion = new RelayCommand(_ => IsThreadMenuOpen = !IsThreadMenuOpen);
            ComandoEnviarMensaje = new RelayCommand(_ => EnviarMensaje());
            ComandoVerPerfil = new RelayCommand(_ => MessageBox.Show("Abrir perfil del contacto", "Chat", MessageBoxButton.OK, MessageBoxImage.Information));
            ComandoBloquearContacto = new RelayCommand(_ => BloquearContacto());
            ComandoEliminarChat = new RelayCommand(_ => EliminarChat());
            ComandoSilenciarChat = new RelayCommand(_ => MessageBox.Show("Chat silenciado", "Chat", MessageBoxButton.OK, MessageBoxImage.Information));
            ComandoMarcarNoLeido = new RelayCommand(_ => MessageBox.Show("Chat marcado como no leído", "Chat", MessageBoxButton.OK, MessageBoxImage.Information));
        }

        public ObservableCollection<MensajeItemViewModel> Mensajes => _mensajes;

        public ImageSource? ContactoAvatarUrl
        {
            get => _contactoAvatarUrl;
            private set => EstablecerPropiedad(ref _contactoAvatarUrl, value);
        }

        public string ContactoNombre
        {
            get => _contactoNombre;
            private set => EstablecerPropiedad(ref _contactoNombre, value);
        }

        public string NuevoMensaje
        {
            get => _nuevoMensaje;
            set => EstablecerPropiedad(ref _nuevoMensaje, value);
        }

        public bool IsChatMenuOpen
        {
            get => _isChatMenuOpen;
            set => EstablecerPropiedad(ref _isChatMenuOpen, value);
        }

        public bool IsThreadMenuOpen
        {
            get => _isThreadMenuOpen;
            set => EstablecerPropiedad(ref _isThreadMenuOpen, value);
        }

        public ICommand ComandoRegresar { get; }
        public ICommand ComandoAlternarMenuChat { get; }
        public ICommand ComandoAlternarMenuConversacion { get; }
        public ICommand ComandoEnviarMensaje { get; }
        public ICommand ComandoVerPerfil { get; }
        public ICommand ComandoBloquearContacto { get; }
        public ICommand ComandoEliminarChat { get; }
        public ICommand ComandoSilenciarChat { get; }
        public ICommand ComandoMarcarNoLeido { get; }

        /// <summary>
        /// Carga los datos del chat asociado al emparejamiento indicado.
        /// </summary>
        public void Cargar(int matchId, int perfilActualId)
        {
            _matchId = matchId;
            _perfilActualId = perfilActualId;
            Mensajes.Clear();

            // 1. Recuperar información del match y determinar quién es el contacto.
            var match = _matchRepository.ObtenerPorId(matchId);
            if (match == null)
            {
                throw new InvalidOperationException("El match especificado no existe");
            }

            var contactoId = match.PerfilEmisor == perfilActualId ? match.PerfilReceptor : match.PerfilEmisor;
            var perfilContacto = _perfilRepository.ObtenerPorId(contactoId);
            if (perfilContacto != null)
            {
                ContactoNombre = perfilContacto.Nikname;
                ContactoAvatarUrl = ConvertirAImagen(perfilContacto.FotoPerfil);
            }

            // 2. Asegurar que exista un chat físico. Si aún no se creó se hace en este momento.
            var chat = _matchRepository.ObtenerChatPorMatchId(matchId) ?? new Chat();
            if (chat.IdChat == 0)
            {
                var chatId = _matchRepository.AsegurarChatParaMatch(matchId);
                chat = _matchRepository.ObtenerChatPorMatchId(matchId) ?? new Chat { IdChat = chatId };
            }

            _chatId = chat.IdChat;

            // 3. Cargar los mensajes en orden cronológico ascendente para mostrarlos en la interfaz.
            var mensajes = _matchRepository.ListarMensajes(_chatId, 0, 50);
            foreach (var mensaje in mensajes.OrderBy(m => m.FechaEnvio))
            {
                Mensajes.Add(MapearMensaje(mensaje));
            }
        }

        /// <summary>
        /// Envía un mensaje al chat actual.
        /// </summary>
        private void EnviarMensaje()
        {
            if (string.IsNullOrWhiteSpace(NuevoMensaje) || _chatId == 0)
            {
                return;
            }

            var contenido = NuevoMensaje.Trim();
            NuevoMensaje = string.Empty;
            // La capa de servicio se encarga de las transacciones y la actualización de metadatos del chat.
            var mensajeId = _matchService.EnviarMensaje(_chatId, _perfilActualId, contenido);
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
            Mensajes.Add(MapearMensaje(mensaje));
        }

        /// <summary>
        ///     Cambia el estado del match a "roto" para impedir futuros mensajes del contacto.
        /// </summary>
        private void BloquearContacto()
        {
            if (_matchId == 0)
            {
                return;
            }

            _matchRepository.ActualizarEstado(_matchId, "roto");
            MessageBox.Show("El contacto fue bloqueado", "Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        ///     Elimina el match (y por cascada el chat) y cierra la ventana actual.
        /// </summary>
        private void EliminarChat()
        {
            if (_matchId == 0)
            {
                return;
            }

            _matchRepository.EliminarMatch(_matchId);
            Application.Current?.Windows[Application.Current.Windows.Count - 1]?.Close();
        }

        /// <summary>
        ///     Cierra la ventana activa del chat.
        /// </summary>
        private void CerrarVentana()
        {
            Application.Current?.Windows[Application.Current.Windows.Count - 1]?.Close();
        }

        /// <summary>
        /// Construye el modelo de vista para representar un mensaje en pantalla.
        /// </summary>
        private MensajeItemViewModel MapearMensaje(Mensaje mensaje)
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

        /// <summary>
        /// Convierte datos binarios en una imagen utilizable por la interfaz.
        /// </summary>
        private static ImageSource? ConvertirAImagen(byte[] bytes)
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

    /// <summary>
    ///     Representa la información necesaria para dibujar una burbuja de chat en la interfaz.
    /// </summary>
    public sealed class MensajeItemViewModel : BaseViewModel
    {
        /// <summary>Identificador del mensaje (útil para editar o borrar).</summary>
        public long MensajeId { get; set; }

        /// <summary>Contenido textual del mensaje que se muestra en la burbuja.</summary>
        public string Contenido { get; set; } = string.Empty;

        /// <summary>Columna donde se coloca el avatar dentro del grid (0 izquierda, 2 derecha).</summary>
        public int AvatarColumn { get; set; }

        /// <summary>Columna que ocupará la burbuja para alinear correctamente la conversación.</summary>
        public int BubbleColumn { get; set; }

        /// <summary>Alineación horizontal de la burbuja.</summary>
        public HorizontalAlignment BubbleAlign { get; set; } = HorizontalAlignment.Left;

        /// <summary>Color de fondo de la burbuja (diferencia mensajes propios y ajenos).</summary>
        public Brush BubbleColor { get; set; } = Brushes.White;

        /// <summary>Imagen del contacto cuando el mensaje es de la otra persona.</summary>
        public ImageSource? AvatarUrl { get; set; }
    }
}
