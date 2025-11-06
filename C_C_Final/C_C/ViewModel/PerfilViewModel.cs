using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Model;
using C_C_Final.View;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Administra la información y acciones del perfil del alumno en la interfaz.
    /// </summary>
    public sealed class PerfilViewModel : BaseViewModel
    {
        private readonly IPerfilRepository _perfilRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly ObservableCollection<ChatResumenViewModel> _listaChats = new ObservableCollection<ChatResumenViewModel>();
        private readonly ICollectionView _chatsView;
        private int _idPerfil;
        private int _idCuenta;
        private string _nikName = string.Empty;
        private string _descripcion = string.Empty;
        private ImageSource _fotoPerfil;
        private byte[] _fotoPerfilBytes;
        private bool _hasUnread;
        private int _unreadCount;
        private bool _isBusy;
        private bool _isSettingsMenuOpen;
        private DateTime _fechaCreacion;
        private string _filtroBusqueda = string.Empty;

        public PerfilViewModel(IPerfilRepository perfilRepository, IMatchRepository matchRepository)
        {
            _perfilRepository = perfilRepository;
            _matchRepository = matchRepository;
            _chatsView = CollectionViewSource.GetDefaultView(_listaChats);
            _chatsView.Filter = FiltrarChat;
            ComandoRegresar = new RelayCommand(_ => Regresar());
            ComandoAlternarConfiguracion = new RelayCommand(_ => IsSettingsMenuOpen = !IsSettingsMenuOpen);
            ComandoIrAConfiguracion = new RelayCommand(_ => AbrirConfiguracion());
            ComandoEditarPerfil = new RelayCommand(_ => EditarPerfil());
            ComandoSubirFoto = new RelayCommand(_ => SubirFoto());
            ComandoGuardarDescripcion = new RelayCommand(_ => GuardarCambios(), _ => !IsBusy);
            ComandoAbrirChat = new RelayCommand(param => AbrirChat(param as ChatResumenViewModel), param => param is ChatResumenViewModel chat && chat.MatchId != 0);
        }

        public ObservableCollection<ChatResumenViewModel> ListaChats => _listaChats;

        public ICollectionView ChatsView => _chatsView;

        public ICommand ComandoRegresar { get; }
        public ICommand ComandoAlternarConfiguracion { get; }
        public ICommand ComandoIrAConfiguracion { get; }
        public ICommand ComandoEditarPerfil { get; }
        public ICommand ComandoSubirFoto { get; }
        public ICommand ComandoGuardarDescripcion { get; }
        public ICommand ComandoAbrirChat { get; }

        public string NikName
        {
            get => _nikName;
            set => EstablecerPropiedad(ref _nikName, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set => EstablecerPropiedad(ref _descripcion, value);
        }

        public ImageSource FotoPerfilUrl
        {
            get => _fotoPerfil;
            private set => EstablecerPropiedad(ref _fotoPerfil, value);
        }

        public bool HasUnread
        {
            get => _hasUnread;
            private set => EstablecerPropiedad(ref _hasUnread, value);
        }

        public int UnreadCount
        {
            get => _unreadCount;
            private set => EstablecerPropiedad(ref _unreadCount, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (EstablecerPropiedad(ref _isBusy, value))
                {
                    (ComandoGuardarDescripcion as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        public bool IsSettingsMenuOpen
        {
            get => _isSettingsMenuOpen;
            set => EstablecerPropiedad(ref _isSettingsMenuOpen, value);
        }

        public string FiltroBusqueda
        {
            get => _filtroBusqueda;
            set
            {
                if (EstablecerPropiedad(ref _filtroBusqueda, value))
                {
                    _chatsView.Refresh();
                }
            }
        }

        /// <summary>
        /// Carga la información del perfil asociado a la cuenta indicada.
        /// </summary>
        /// 

        public void EditarPerfil()
        {
            if (_idCuenta == 0)
            {
                MessageBox.Show("No se ha cargado un perfil válido.", "Editar Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsSettingsMenuOpen = false;

            var configuracionView = new ConfiguracionView(_idCuenta);
            Window ventanaActual = null;
            var app = Application.Current;
            if (app != null)
            {
                foreach (Window window in app.Windows)
                {
                    if (ReferenceEquals(window.DataContext, this))
                    {
                        ventanaActual = window;
                        break;
                    }
                }
            }

            configuracionView.Show();
            ventanaActual?.Close();
        }

        private void AbrirConfiguracion()
        {
            EditarPerfil();
        }

        public void Cargar(int cuentaId)
        {
            _idCuenta = cuentaId;
            var perfil = _perfilRepository.ObtenerPorCuentaId(cuentaId);
            if (perfil == null)
            {
                return;
            }

            _idPerfil = perfil.IdPerfil;
            NikName = perfil.Nikname;
            Descripcion = perfil.Biografia;
            _fotoPerfilBytes = perfil.FotoPerfil;
            FotoPerfilUrl = ConvertirAImagen(_fotoPerfilBytes);
            _fechaCreacion = perfil.FechaCreacion;

            HasUnread = false;
            UnreadCount = 0;

            CargarChats();
        }

        /// <summary>
        /// Permite al usuario seleccionar una nueva foto de perfil.
        /// </summary>
        private void SubirFoto()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Seleccionar foto de perfil"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _fotoPerfilBytes = File.ReadAllBytes(dialog.FileName);
            FotoPerfilUrl = new BitmapImage(new Uri(dialog.FileName));
        }

        /// <summary>
        /// Guarda los cambios realizados sobre la descripción o imagen del perfil.
        /// </summary>
        private void GuardarCambios()
        {
            if (_idPerfil == 0)
            {
                MessageBox.Show("No se ha cargado un perfil", "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                var perfil = new Perfil
                {
                    IdPerfil = _idPerfil,
                    IdCuenta = _idCuenta,
                    Nikname = NikName,
                    Biografia = Descripcion,
                    FotoPerfil = _fotoPerfilBytes,
                    FechaCreacion = _fechaCreacion
                };

                var updated = _perfilRepository.ActualizarPerfil(perfil);
                if (updated)
                {
                    MessageBox.Show("Perfil actualizado", "Perfil", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No fue posible actualizar el perfil", "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Cierra la ventana actual y regresa a la vista anterior.
        /// </summary>
        private void Regresar()
        {
            if (_idPerfil == 0)
            {
                MessageBox.Show("No se ha cargado un perfil válido.", "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var homeView = new HomeView(_idPerfil);
            Window ventanaActual = null;
            var app = Application.Current;
            if (app != null)
            {
                foreach (Window window in app.Windows)
                {
                    if (ReferenceEquals(window.DataContext, this))
                    {
                        ventanaActual = window;
                        break;
                    }
                }
            }

            homeView.Show();
            ventanaActual?.Close();
        }

        private bool FiltrarChat(object obj)
        {
            if (string.IsNullOrWhiteSpace(FiltroBusqueda))
            {
                return true;
            }

            if (obj is not ChatResumenViewModel chat)
            {
                return false;
            }

            return chat.NombreContacto?.IndexOf(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) >= 0
                || chat.UltimoMensaje?.IndexOf(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CargarChats()
        {
            _listaChats.Clear();
            if (_idPerfil == 0)
            {
                return;
            }

            var matches = _matchRepository.ListarPorPerfil(_idPerfil, 0, 50) ?? Array.Empty<Match>();
            var acumuladorNoLeidos = 0;
            foreach (var match in matches)
            {
                if (!MatchEstadoHelper.EsActivo(match.Estado))
                {
                    continue;
                }

                var contactoId = match.PerfilEmisor == _idPerfil ? match.PerfilReceptor : match.PerfilEmisor;
                var contacto = _perfilRepository.ObtenerPorId(contactoId);

                var chat = _matchRepository.ObtenerChatPorMatchId(match.IdMatch);
                if (chat == null)
                {
                    var chatId = _matchRepository.AsegurarChatParaMatch(match.IdMatch);
                    chat = _matchRepository.ObtenerChatPorMatchId(match.IdMatch) ?? new Chat { IdChat = chatId, IdMatch = match.IdMatch };
                }

                var ultimoMensaje = "No hay mensajes aún";
                var mensajesNoLeidos = 0;

                if (chat.IdChat != 0)
                {
                    var mensajes = _matchRepository.ListarMensajes(chat.IdChat, 0, 20) ?? Array.Empty<Mensaje>();
                    var masReciente = mensajes.FirstOrDefault();
                    if (masReciente != null)
                    {
                        ultimoMensaje = masReciente.Contenido;
                    }

                    mensajesNoLeidos = mensajes.Count(m => !m.ConfirmacionLectura && m.IdRemitentePerfil != _idPerfil);
                }

                var resumen = new ChatResumenViewModel
                {
                    MatchId = match.IdMatch,
                    ChatId = chat.IdChat,
                    NombreContacto = contacto?.Nikname ?? "Contacto",
                    FotoPerfil = ConvertirAImagen(contacto?.FotoPerfil),
                    UltimoMensaje = ultimoMensaje,
                    IsOnline = MatchEstadoHelper.EsActivo(match.Estado),
                    MensajesNoLeidos = mensajesNoLeidos
                };

                _listaChats.Add(resumen);
                acumuladorNoLeidos += mensajesNoLeidos;
            }

            UnreadCount = acumuladorNoLeidos;
            HasUnread = acumuladorNoLeidos > 0;
            _chatsView.Refresh();
        }

        private void AbrirChat(ChatResumenViewModel chatResumen)
        {
            if (chatResumen == null)
            {
                return;
            }

            if (chatResumen.MatchId == 0)
            {
                MessageBox.Show("No fue posible abrir el chat seleccionado.", "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var chatView = new ChatView(chatResumen.MatchId, _idPerfil)
            {
                Owner = Application.Current?.Windows.Cast<Window>().FirstOrDefault(w => ReferenceEquals(w.DataContext, this))
            };

            chatView.Show();
        }

        /// <summary>
        /// Convierte un arreglo de bytes en una imagen para su visualización.
        /// </summary>
        private static ImageSource ConvertirAImagen(byte[] bytes)
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

    public sealed class ChatResumenViewModel : BaseViewModel
    {
        private int _matchId;
        private int _chatId;
        private string _nombreContacto = string.Empty;
        private ImageSource _fotoPerfil;
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

        public ImageSource FotoPerfil
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
