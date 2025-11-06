using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.Services;
using C_C_Final.View;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// ViewModel para la vista principal (Home), donde se muestran
    /// y se interactúa con otros perfiles (like/rechazo).
    /// </summary>
    public sealed class HomeViewModel : BaseViewModel
    {
        // --- Repositorios y Servicios ---
        private readonly IPerfilRepository _perfilRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly MatchService _matchService;
        private readonly int _idPerfilActual;

        private readonly RelayCommand _comandoAceptar;
        private readonly RelayCommand _comandoRechazar;
        private readonly RelayCommand _comandoSiguiente;
        private readonly RelayCommand _comandoAlternarConfiguracion;
        private readonly RelayCommand _comandoIrAMiPerfil;
        private readonly RelayCommand _comandoBloquearPerfil;

        private PerfilSugerenciaViewModel? _perfilActual;
        private bool _isSettingsMenuOpen;
        private string _estadoMensaje = string.Empty;
        private int _perfilesDisponibles;

        // --- Constructor ---
        public HomeViewModel(int idPerfilLogueado)
        {
            // El ViewModel crea sus dependencias requeridas para la lectura de perfiles y gestión de matches.
            _perfilRepository = new PerfilRepository();
            _matchRepository = new MatchRepository();
            _matchService = new MatchService(_matchRepository);
            _idPerfilActual = idPerfilLogueado;

            _comandoAceptar = new RelayCommand(_ => RegistrarInteraccion(true), _ => PerfilActual != null);
            _comandoRechazar = new RelayCommand(_ => RegistrarInteraccion(false), _ => PerfilActual != null);
            _comandoSiguiente = new RelayCommand(_ => CargarSiguientePerfil(), _ => PerfilActual != null);
            _comandoAlternarConfiguracion = new RelayCommand(_ => IsSettingsMenuOpen = !IsSettingsMenuOpen);
            _comandoIrAMiPerfil = new RelayCommand(_ => NavegarAConfiguracion());
            _comandoBloquearPerfil = new RelayCommand(_ => BloquearPerfilActual(), _ => PerfilActual != null);

            // --- CORRECCIÓN ---
            // Carga el primer perfil al iniciar para que la UI muestre contenido inmediatamente.
            CargarSiguientePerfil();
        }

        // --- Propiedad para el Perfil en Pantalla ---
        public PerfilSugerenciaViewModel? PerfilActual
        {
            get => _perfilActual;
            set
            {
                if (EstablecerPropiedad(ref _perfilActual, value))
                {
                    _comandoAceptar.NotificarCambioPuedeEjecutar();
                    _comandoRechazar.NotificarCambioPuedeEjecutar();
                    _comandoBloquearPerfil.NotificarCambioPuedeEjecutar();
                    _comandoSiguiente.NotificarCambioPuedeEjecutar();
                }
            }
        }

        public string EstadoMensaje
        {
            get => _estadoMensaje;
            private set => EstablecerPropiedad(ref _estadoMensaje, value);
        }

        public int PerfilesDisponibles
        {
            get => _perfilesDisponibles;
            private set => EstablecerPropiedad(ref _perfilesDisponibles, value);
        }

        public bool IsSettingsMenuOpen
        {
            get => _isSettingsMenuOpen;
            set => EstablecerPropiedad(ref _isSettingsMenuOpen, value);
        }

        public ICommand ComandoAceptar => _comandoAceptar;
        public ICommand ComandoRechazar => _comandoRechazar;
        public ICommand ComandoSiguiente => _comandoSiguiente;
        public ICommand ComandoAlternarConfiguracion => _comandoAlternarConfiguracion;
        public ICommand ComandoIrAMiPerfil => _comandoIrAMiPerfil;
        public ICommand ComandoBloquearPerfil => _comandoBloquearPerfil;

        // --- LÓGICA DE NAVEGACIÓN ---
        /// <summary>
        ///     Abre la vista de configuración del usuario autenticado cerrando la ventana actual para evitar duplicados.
        /// </summary>
        public void NavegarAConfiguracion()
        {
            try
            {
                IsSettingsMenuOpen = false;
                var perfilActual = _perfilRepository.ObtenerPorId(_idPerfilActual);
                if (perfilActual == null || perfilActual.IdCuenta == 0)
                {
                    MessageBox.Show("No se pudo cargar la información de la cuenta.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idCuenta = perfilActual.IdCuenta;
                var configuracionView = new ConfiguracionView(idCuenta);

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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la configuración: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- LÓGICA DE "LIKE" Y "RECHAZO" ---
        /// <summary>
        ///     Registra la interacción del usuario (like o rechazo) y gestiona la creación/actualización de matches.
        /// </summary>
        /// <param name="esLike">Indica si la acción es un like (true) o un rechazo (false).</param>
        private void RegistrarInteraccion(bool esLike)
        {
            if (PerfilActual?.Perfil == null)
            {
                return;
            }

            int idPerfilDestino = PerfilActual.Perfil.IdPerfil;

            try
            {
                // 1. Consultar si ya existe alguna interacción previa entre los perfiles.
                var matchExistente = _matchRepository.ObtenerPorPerfiles(_idPerfilActual, idPerfilDestino);

                if (matchExistente == null)
                {
                    // 2a. No existe interacción: se crea un nuevo registro con estado pendiente o rechazado.
                    var estadoInicial = esLike
                        ? MatchEstadoHelper.ConstruirPendiente()
                        : MatchEstadoHelper.ConstruirRechazado();

                    _matchService.CrearMatch(_idPerfilActual, idPerfilDestino, estadoInicial);
                }
                else
                {
                    if (esLike)
                    {
                        // 2b. Ya existe un match pendiente emitido por el otro perfil: al aceptar se crea el chat.
                        if (MatchEstadoHelper.EsPendiente(matchExistente.Estado)
                            && matchExistente.PerfilEmisor == idPerfilDestino)
                        {
                            // ¡Es un match mutuo!
                            _matchRepository.ActualizarEstado(matchExistente.IdMatch, MatchEstadoHelper.ConstruirAceptado());

                            // ¡AHORA SÍ, creamos el chat!
                            _matchService.AsegurarChatParaMatch(matchExistente.IdMatch);

                            MessageBox.Show($"¡Hiciste match con {PerfilActual.NombreEdad}!", "¡Es un Match!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (MatchEstadoHelper.EsRechazado(matchExistente.Estado))
                        {
                            // Permite reintentar enviando un nuevo corazón reactivando el estado pendiente.
                            _matchRepository.ActualizarEstado(matchExistente.IdMatch, MatchEstadoHelper.ConstruirPendiente());
                            _matchRepository.ActualizarParticipantes(matchExistente.IdMatch, _idPerfilActual, idPerfilDestino);
                        }
                    }
                    else
                    {
                        // 2c. Rechazo explícito: actualiza estado y asegura que el perfil actual sea el emisor.
                        _matchRepository.ActualizarEstado(matchExistente.IdMatch, MatchEstadoHelper.ConstruirRechazado());
                        _matchRepository.ActualizarParticipantes(matchExistente.IdMatch, _idPerfilActual, idPerfilDestino);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la interacción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // 3. Tras completar la interacción se carga la siguiente sugerencia.
            CargarSiguientePerfil();
        }

        // --- CORRECCIÓN ---
        /// <summary>
        ///     Solicita al repositorio la siguiente sugerencia disponible, limpia la colección y actualiza los mensajes de estado.
        /// </summary>
        private void CargarSiguientePerfil()
        {
            try
            {
                // Llama al nuevo método del repositorio
                var perfil = _perfilRepository.ObtenerSiguientePerfilPara(_idPerfilActual);

                if (perfil == null)
                {
                    // Opcional: Mostrar un mensaje o un estado de "No hay más perfiles"
                    PerfilActual = null;
                    EstadoMensaje = "¡Has visto todos los perfiles por ahora!";
                    PerfilesDisponibles = 0;
                    return;
                }

                var vista = CrearSugerencia(perfil);
                PerfilActual = vista;
                PerfilesDisponibles = 1;

                var match = _matchRepository.ObtenerPorPerfiles(_idPerfilActual, perfil.IdPerfil);
                if (match != null && MatchEstadoHelper.EsPendiente(match.Estado) && match.PerfilEmisor == perfil.IdPerfil)
                {
                    EstadoMensaje = $"{vista.NombreEdad} te envió un corazón. ¡Devuélvelo para hacer match!";
                }
                else
                {
                    EstadoMensaje = string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el siguiente perfil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PerfilActual = null;
                EstadoMensaje = "Ocurrió un error al cargar perfiles.";
                PerfilesDisponibles = 0;
            }
        }

        /// <summary>
        ///     Marca un perfil como bloqueado reutilizando la lógica de rechazo y proporciona feedback visual.
        /// </summary>
        private void BloquearPerfilActual()
        {
            if (PerfilActual?.Perfil == null)
            {
                return;
            }

            IsSettingsMenuOpen = false;
            var resultado = MessageBox.Show(
                $"¿Deseas bloquear a {PerfilActual.NombreEdad}?", "Bloquear perfil",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                RegistrarInteraccion(false);
                EstadoMensaje = "Perfil bloqueado.";
            }
        }

        /// <summary>
        ///     Transforma la entidad <see cref="Perfil"/> en un objeto listo para mostrar en la tarjeta de sugerencia.
        /// </summary>
        private static PerfilSugerenciaViewModel CrearSugerencia(Perfil perfil)
        {
            var nombre = string.IsNullOrWhiteSpace(perfil.Nikname)
                ? "Usuario desconocido"
                : perfil.Nikname;

            var descripcion = string.IsNullOrWhiteSpace(perfil.Biografia)
                ? "Sin biografía disponible"
                : perfil.Biografia;

            return new PerfilSugerenciaViewModel(perfil, ConvertirAImagen(perfil.FotoPerfil), nombre, descripcion);
        }

        /// <summary>
        ///     Convierte un arreglo de bytes almacenado en la base de datos en una imagen compatible con WPF.
        /// </summary>
        private static ImageSource? ConvertirAImagen(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            try
            {
                using var ms = new MemoryStream(bytes);
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = ms;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     ViewModel auxiliar que encapsula los datos visuales de una tarjeta de sugerencia.
        /// </summary>
        public sealed class PerfilSugerenciaViewModel
        {
            public PerfilSugerenciaViewModel(Perfil perfil, ImageSource? fotoUrl, string nombreEdad, string carreraTexto)
            {
                Perfil = perfil;
                FotoUrl = fotoUrl;
                NombreEdad = nombreEdad;
                CarreraTexto = carreraTexto;
            }

            public Perfil Perfil { get; }
            public ImageSource? FotoUrl { get; }
            public string NombreEdad { get; }
            public string CarreraTexto { get; }
        }
    }
}
