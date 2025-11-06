using System;
using System.Collections.ObjectModel;
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

        private readonly ObservableCollection<PerfilSugerenciaViewModel> _sugerencias = new ObservableCollection<PerfilSugerenciaViewModel>();
        private readonly RelayCommand _comandoAceptar;
        private readonly RelayCommand _comandoRechazar;
        private readonly RelayCommand _comandoSiguiente;
        private readonly RelayCommand _comandoAnterior;
        private readonly RelayCommand _comandoAlternarConfiguracion;
        private readonly RelayCommand _comandoIrAMiPerfil;
        private readonly RelayCommand _comandoBloquearPerfil;

        private PerfilSugerenciaViewModel? _perfilActual;
        private bool _isSettingsMenuOpen;
        private string _estadoMensaje = string.Empty;

        // --- Constructor ---
        public HomeViewModel(int idPerfilLogueado)
        {
            _perfilRepository = new PerfilRepository();
            _matchRepository = new MatchRepository();
            _matchService = new MatchService(_matchRepository);
            _idPerfilActual = idPerfilLogueado;

            _comandoAceptar = new RelayCommand(_ => RegistrarInteraccion(true), _ => PerfilActual != null);
            _comandoRechazar = new RelayCommand(_ => RegistrarInteraccion(false), _ => PerfilActual != null);
            _comandoSiguiente = new RelayCommand(_ => CargarSiguientePerfil(), _ => PerfilActual != null);
            _comandoAnterior = new RelayCommand(_ => { }, _ => false);
            _comandoAlternarConfiguracion = new RelayCommand(_ => IsSettingsMenuOpen = !IsSettingsMenuOpen);
            _comandoIrAMiPerfil = new RelayCommand(_ => NavegarAConfiguracion());
            _comandoBloquearPerfil = new RelayCommand(_ => BloquearPerfilActual(), _ => PerfilActual != null);

            // --- CORRECCIÓN ---
            // Carga el primer perfil al iniciar
            CargarSiguientePerfil();
        }

        public ObservableCollection<PerfilSugerenciaViewModel> Sugerencias => _sugerencias;

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
                    _comandoAnterior.NotificarCambioPuedeEjecutar();
                }
            }
        }

        public string EstadoMensaje
        {
            get => _estadoMensaje;
            private set => EstablecerPropiedad(ref _estadoMensaje, value);
        }

        public bool IsSettingsMenuOpen
        {
            get => _isSettingsMenuOpen;
            set => EstablecerPropiedad(ref _isSettingsMenuOpen, value);
        }

        public ICommand ComandoAceptar => _comandoAceptar;
        public ICommand ComandoRechazar => _comandoRechazar;
        public ICommand ComandoSiguiente => _comandoSiguiente;
        public ICommand ComandoAnterior => _comandoAnterior;
        public ICommand ComandoAlternarConfiguracion => _comandoAlternarConfiguracion;
        public ICommand ComandoIrAMiPerfil => _comandoIrAMiPerfil;
        public ICommand ComandoBloquearPerfil => _comandoBloquearPerfil;

        // --- LÓGICA DE NAVEGACIÓN ---
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
        private void RegistrarInteraccion(bool esLike)
        {
            if (PerfilActual?.Perfil == null)
            {
                return;
            }

            int idPerfilDestino = PerfilActual.Perfil.IdPerfil;

            try
            {
                var matchExistente = _matchRepository.ObtenerPorPerfiles(_idPerfilActual, idPerfilDestino);

                if (matchExistente == null)
                {
                    var estadoInicial = esLike
                        ? MatchEstadoHelper.ConstruirPendiente()
                        : MatchEstadoHelper.ConstruirRechazado();

                    _matchService.CrearMatch(_idPerfilActual, idPerfilDestino, estadoInicial);
                }
                else
                {
                    if (esLike)
                    {
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
                            // Permite reintentar enviando un nuevo corazón.
                            _matchRepository.ActualizarEstado(matchExistente.IdMatch, MatchEstadoHelper.ConstruirPendiente());
                            _matchRepository.ActualizarParticipantes(matchExistente.IdMatch, _idPerfilActual, idPerfilDestino);
                        }
                    }
                    else
                    {
                        _matchRepository.ActualizarEstado(matchExistente.IdMatch, MatchEstadoHelper.ConstruirRechazado());
                        _matchRepository.ActualizarParticipantes(matchExistente.IdMatch, _idPerfilActual, idPerfilDestino);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la interacción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Cargar el siguiente perfil
            CargarSiguientePerfil();
        }

        // --- CORRECCIÓN ---
        private void CargarSiguientePerfil()
        {
            try
            {
                // Llama al nuevo método del repositorio
                var perfil = _perfilRepository.ObtenerSiguientePerfilPara(_idPerfilActual);

                _sugerencias.Clear();

                if (perfil == null)
                {
                    // Opcional: Mostrar un mensaje o un estado de "No hay más perfiles"
                    PerfilActual = null;
                    EstadoMensaje = "¡Has visto todos los perfiles por ahora!";
                    return;
                }

                var vista = CrearSugerencia(perfil);
                _sugerencias.Add(vista);
                PerfilActual = vista;

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
                _sugerencias.Clear();
                EstadoMensaje = "Ocurrió un error al cargar perfiles.";
            }
        }

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
