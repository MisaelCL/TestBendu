using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.View;

namespace C_C_Final.ViewModel
{
    // (La clase ChatResumenViewModel se mantiene igual que en tu archivo original)
    public sealed class ChatResumenViewModel : BaseViewModel
    {
        private int _matchId;
        private int _chatId;
        private string _nombreContacto = string.Empty;
        private ImageSource _fotoPerfil;
        private string _ultimoMensaje = string.Empty;
        private bool _isOnline;
        private int _mensajesNoLeidos;
        private DateTime _ultimoMensajeFecha;
        private bool _esActivo;

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

        public DateTime UltimoMensajeFecha
        {
            get => _ultimoMensajeFecha;
            set => EstablecerPropiedad(ref _ultimoMensajeFecha, value);
        }

        public bool EsActivo
        {
            get => _esActivo;
            set => EstablecerPropiedad(ref _esActivo, value);
        }
    }

    // --- Clase principal del ViewModel ---
    public sealed class HomeViewModel : BaseViewModel
    {
        private readonly IPerfilRepository _perfilRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly int _idPerfilActual;
        private readonly ObservableCollection<ChatResumenViewModel> _listaChats = new ObservableCollection<ChatResumenViewModel>();
        private readonly ICollectionView _chatsView;
        private string _filtroBusqueda = string.Empty;
        private Perfil _perfilActual;
        private bool _isMenuAbierto;

        public ICommand ComandoAbrirChat { get; }
        public ICommand ComandoAbrirPerfil { get; }
        public ICommand ComandoAlternarMenu { get; }
        public ICommand ComandoCerrarSesion { get; }
        public ICommand ComandoIrAConfiguracion { get; }

        public Perfil PerfilActual
        {
            get => _perfilActual;
            set => EstablecerPropiedad(ref _perfilActual, value);
        }

        public ICollectionView ChatsView => _chatsView;

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

        public bool IsMenuAbierto
        {
            get => _isMenuAbierto;
            set => EstablecerPropiedad(ref _isMenuAbierto, value);
        }

        public HomeViewModel(int idPerfil)
        {
            _idPerfilActual = idPerfil;
            _perfilRepository = new PerfilRepository();
            _matchRepository = new MatchRepository();
            
            _chatsView = CollectionViewSource.GetDefaultView(_listaChats);
            _chatsView.Filter = FiltrarChat;
            _chatsView.SortDescriptions.Add(new SortDescription(nameof(ChatResumenViewModel.UltimoMensajeFecha), ListSortDirection.Descending));

            ComandoAbrirChat = new RelayCommand(param => AbrirChat(param as ChatResumenViewModel), param => param is ChatResumenViewModel);
            ComandoAbrirPerfil = new RelayCommand(_ => AbrirMiPerfil());
            ComandoAlternarMenu = new RelayCommand(_ => IsMenuAbierto = !IsMenuAbierto);
            ComandoCerrarSesion = new RelayCommand(_ => CerrarSesion());
            ComandoIrAConfiguracion = new RelayCommand(_ => AbrirConfiguracion());

            CargarPerfil();
            CargarChats();
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

            return chat.NombreContacto?.IndexOf(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void CargarPerfil()
        {
            if (_idPerfilActual == 0) return;
            PerfilActual = _perfilRepository.ObtenerPorId(_idPerfilActual);
        }

        private Window ObtenerVentanaActual()
        {
            var app = Application.Current;
            if (app == null) return null;
            
            foreach (Window window in app.Windows)
            {
                if (ReferenceEquals(window.DataContext, this))
                {
                    return window;
                }
            }
            return null;
        }

        private void AbrirMiPerfil()
        {
            if (PerfilActual == null) return;
            
            var perfilView = new PerfilView(PerfilActual.IdCuenta);
            var ventanaActual = ObtenerVentanaActual();
            perfilView.Show();
            ventanaActual?.Close();
        }

        // Este es el método que será llamado por el SettingsButton_Click
        public void AbrirConfiguracion()
        {
            if (PerfilActual == null) return;
            
            var configView = new ConfiguracionView(PerfilActual.IdCuenta);
            var ventanaActual = ObtenerVentanaActual();
            configView.Show();
            ventanaActual?.Close();
        }

        private void CerrarSesion()
        {
            var loginView = new LoginView();
            var ventanaActual = ObtenerVentanaActual();
            loginView.Show();
            ventanaActual?.Close();
        }

        private void AbrirChat(ChatResumenViewModel chatResumen)
        {
            if (chatResumen == null || !chatResumen.EsActivo)
            {
                MessageBox.Show("No se puede abrir este chat. El match ya no está activo.", "Chat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var chatView = new ChatView(chatResumen.MatchId, _idPerfilActual)
            {
                Owner = ObtenerVentanaActual()
            };
            chatView.ShowDialog(); // Usamos ShowDialog para bloquear hasta que se cierre

            // Recargar la lista de chats para actualizar el último mensaje y no leídos
            CargarChats();
        }

        private void CargarChats()
        {
            // --- CORRECCIÓN (CS0162) ---
            // Se mueve _listaChats.Clear() al inicio del método.
            _listaChats.Clear();
            
            if (_idPerfilActual == 0)
            {
                return;
            }

            try
            {
                var matches = _matchRepository.ListarPorPerfil(_idPerfilActual, 0, 100) ?? Array.Empty<Match>();
                
                foreach (var match in matches)
                {
                    var esActivo = MatchEstadoHelper.EsActivo(match.Estado);
                    
                    var contactoId = match.PerfilEmisor == _idPerfilActual ? match.PerfilReceptor : match.PerfilEmisor;
                    var contacto = _perfilRepository.ObtenerPorId(contactoId);
                    if (contacto == null) continue;

                    var chat = _matchRepository.ObtenerChatPorMatchId(match.IdMatch);
                    
                    if (!esActivo && chat == null)
                    {
                        // Si no está activo y nunca chatearon, no lo mostramos
                        continue;
                    }
                    
                    if (esActivo && chat == null)
                    {
                        // Si es activo pero no tiene chat, lo creamos para mostrarlo
                        var chatId = _matchRepository.AsegurarChatParaMatch(match.IdMatch);
                        chat = new Chat { IdChat = chatId, IdMatch = match.IdMatch, FechaCreacion = DateTime.UtcNow };
                    }
                    
                    string ultimoMensaje = esActivo ? "¡Es un match! Envía un mensaje." : "Match no activo";
                    int mensajesNoLeidos = 0;
                    var fechaUltimoMensaje = chat?.FechaCreacion ?? match.FechaMatch;

                    if (chat != null && chat.IdChat != 0)
                    {
                        var mensajes = _matchRepository.ListarMensajes(chat.IdChat, 0, 1);
                        var masReciente = mensajes.FirstOrDefault();
                        if (masReciente != null)
                        {
                            ultimoMensaje = (masReciente.IdRemitentePerfil == _idPerfilActual ? "Tú: " : "") + masReciente.Contenido;
                            fechaUltimoMensaje = masReciente.FechaEnvio;
                            
                            // Recalcular no leídos (esto es costoso, idealmente se hace en otro lado)
                            var todosMensajes = _matchRepository.ListarMensajes(chat.IdChat, 0, 50);
                            mensajesNoLeidos = todosMensajes.Count(m => !m.ConfirmacionLectura && m.IdRemitentePerfil != _idPerfilActual);
                        }
                    }

                    _listaChats.Add(new ChatResumenViewModel
                    {
                        MatchId = match.IdMatch,
                        ChatId = chat?.IdChat ?? 0,
                        NombreContacto = contacto.Nikname,
                        FotoPerfil = PerfilViewModel.ConvertirAImagen(contacto.FotoPerfil),
                        UltimoMensaje = ultimoMensaje,
                        MensajesNoLeidos = mensajesNoLeidos,
                        UltimoMensajeFecha = fechaUltimoMensaje,
                        EsActivo = esActivo
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return; 
                // _listaChats.Clear(); // <-- ESTE ERA EL CÓDIGO INALCANZABLE
            }
            finally
            {
                _chatsView.Refresh();
            }
        }
    }
}
