using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Repositories;
using C_C_Final.Services;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Administra la bandeja de coincidencias y sugerencias del usuario.
    /// </summary>
    public sealed class InboxViewModel : BaseViewModel
    {
        private readonly MatchRepository _matchRepository;
        private readonly PerfilRepository _perfilRepository;
        private readonly MatchService _matchService;
        private readonly ObservableCollection<SugerenciaItemViewModel> _sugerencias = [];
        private SugerenciaItemViewModel _perfilActual;
        private bool _isSettingsMenuOpen;
        private string _estadoMensaje;
        private int _perfilId;
        private int _currentIndex;
        private readonly int _pageSize = 20;

        public InboxViewModel(MatchRepository matchRepository, PerfilRepository perfilRepository, MatchService matchService)
        {
            _matchRepository = matchRepository;
            _perfilRepository = perfilRepository;
            _matchService = matchService;

            ComandoAlternarConfiguracion = new RelayCommand(_ => IsSettingsMenuOpen = !IsSettingsMenuOpen);
            ComandoIrAMiPerfil = new RelayCommand(_ => AbrirMiPerfil());
            ComandoBloquearPerfil = new RelayCommand(_ => BloquearActual());
            ComandoAnterior = new RelayCommand(_ => MoverAnterior());
            ComandoSiguiente = new RelayCommand(_ => MoverSiguiente());
            ComandoAceptar = new RelayCommand(_ => AceptarActual());
            ComandoRechazar = new RelayCommand(_ => RechazarActual());
        }

        public ObservableCollection<SugerenciaItemViewModel> Sugerencias => _sugerencias;

        public SugerenciaItemViewModel PerfilActual
        {
            get => _perfilActual;
            private set => EstablecerPropiedad(ref _perfilActual, value);
        }

        public bool IsSettingsMenuOpen
        {
            get => _isSettingsMenuOpen;
            set => EstablecerPropiedad(ref _isSettingsMenuOpen, value);
        }

        public string EstadoMensaje
        {
            get => _estadoMensaje;
            private set => EstablecerPropiedad(ref _estadoMensaje, value);
        }

        public event Action<int> MiPerfilRequested;

        public ICommand ComandoAlternarConfiguracion { get; }
        public ICommand ComandoIrAMiPerfil { get; }
        public ICommand ComandoBloquearPerfil { get; }
        public ICommand ComandoAnterior { get; }
        public ICommand ComandoSiguiente { get; }
        public ICommand ComandoAceptar { get; }
        public ICommand ComandoRechazar { get; }

        /// <summary>
        /// Carga los perfiles sugeridos y coincidencias para el usuario indicado.
        /// </summary>
        public void Cargar(int perfilId)
        {
            _perfilId = perfilId;
            _sugerencias.Clear();
            _currentIndex = 0;

            var matches = _matchRepository.ListarPorPerfil(perfilId, 0, _pageSize);
            var perfilesAgregados = new HashSet<int>();

            foreach (var match in matches.OrderByDescending(m => m.FechaMatch))
            {
                var otherPerfilId = match.PerfilEmisor == perfilId ? match.PerfilReceptor : match.PerfilEmisor;
                var perfil = _perfilRepository.ObtenerPorId(otherPerfilId);
                if (perfil == null)
                {
                    continue;
                }

                if (!perfilesAgregados.Add(otherPerfilId))
                {
                    continue;
                }

                var descripcionEstado = MatchEstadoHelper.ObtenerDescripcionPara(match.Estado, perfilId, otherPerfilId);

                var sugerencia = new SugerenciaItemViewModel
                {
                    MatchId = match.IdMatch,
                    PerfilId = otherPerfilId,
                    NombreEdad = perfil.Nikname,
                    CarreraTexto = string.IsNullOrWhiteSpace(descripcionEstado) ? perfil.Biografia : descripcionEstado,
                    FotoUrl = ConvertirAImagen(perfil.FotoPerfil),
                    EsPerfilRegistrado = false,
                    EstadoMatch = match.Estado
                };
                _sugerencias.Add(sugerencia);
            }

            var perfilesRegistrados = _perfilRepository.ListarTodos();
            foreach (var perfil in perfilesRegistrados.Where(p => p.IdPerfil != perfilId))
            {
                if (!perfilesAgregados.Add(perfil.IdPerfil))
                {
                    continue;
                }

                var descripcion = string.IsNullOrWhiteSpace(perfil.Biografia)
                    ? "Este perfil aún no tiene biografía."
                    : perfil.Biografia;

                var sugerencia = new SugerenciaItemViewModel
                {
                    MatchId = 0,
                    PerfilId = perfil.IdPerfil,
                    NombreEdad = perfil.Nikname,
                    CarreraTexto = descripcion,
                    FotoUrl = ConvertirAImagen(perfil.FotoPerfil),
                    EsPerfilRegistrado = true,
                    EstadoMatch = string.Empty
                };

                _sugerencias.Add(sugerencia);
            }

            PerfilActual = _sugerencias.FirstOrDefault();

            if (_sugerencias.Count == 0)
            {
                EstadoMensaje = "No hay perfiles registrados.";
            }
            else if (matches.Count > 0)
            {
                EstadoMensaje = "Selecciona un perfil para interactuar.";
            }
            else
            {
                EstadoMensaje = "Explora los perfiles registrados.";
            }
        }

        private void AbrirMiPerfil()
        {


            try
            {
                var perfil = _perfilRepository.ObtenerPorId(_perfilId);
                if (perfil == null)
                {
                    EstadoMensaje = "No se encontró tu perfil.";
                    return;
                }

                IsSettingsMenuOpen = false;
                EstadoMensaje = "Mostrando tu perfil.";
                MiPerfilRequested.Invoke(perfil.IdCuenta);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                EstadoMensaje = e.ToString();
                
            }
        }

        private void AceptarActual()
        {
            try
            {
                var otroPerfilId = PerfilActual.PerfilId;
                var match = PerfilActual.MatchId > 0
                    ? _matchRepository.ObtenerPorId(PerfilActual.MatchId)
                    : _matchRepository.ObtenerPorPerfiles(_perfilId, otroPerfilId);

                    PerfilActual.MatchId = match.IdMatch;
                    PerfilActual.EsPerfilRegistrado = false;
                    PerfilActual.EstadoMatch = match.Estado ?? string.Empty;
                    var estadoActual = PerfilActual.EstadoMatch ?? string.Empty;

                    if (MatchEstadoHelper.EsActivo(estadoActual))
                    {
                        _matchService.AsegurarChatParaMatch(match.IdMatch);
                        PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(estadoActual, _perfilId, otroPerfilId);
                        EstadoMensaje = $"Ya tienes un match activo con {PerfilActual.NombreEdad}.";
                        return;
                    }

                    if (MatchEstadoHelper.EsPendienteDe(estadoActual, _perfilId))
                    {
                        PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(estadoActual, _perfilId, otroPerfilId);
                        EstadoMensaje = $"Ya enviaste un corazón a {PerfilActual.NombreEdad}.";
                        return;
                    }

                    if (MatchEstadoHelper.EsPendienteDe(estadoActual, otroPerfilId)
                        || string.Equals(estadoActual, "pendiente", StringComparison.OrdinalIgnoreCase))
                    {
                        _matchRepository.ActualizarEstado(match.IdMatch, "activo");
                        PerfilActual.EstadoMatch = "activo";
                        PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara("activo", _perfilId, otroPerfilId);
                        _matchService.AsegurarChatParaMatch(match.IdMatch);
                        EstadoMensaje = $"¡Es un match con {PerfilActual.NombreEdad}! Ya pueden chatear.";
                        return;
                    }

                    if (MatchEstadoHelper.EsRechazadoPor(estadoActual, otroPerfilId)
                        || string.Equals(estadoActual, "rechazado", StringComparison.OrdinalIgnoreCase))
                    {
                        PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(estadoActual, _perfilId, otroPerfilId);
                        EstadoMensaje = $"{PerfilActual.NombreEdad} rechazó tu corazón.";
                        return;
                    }

                    if (MatchEstadoHelper.EsRechazadoPor(estadoActual, _perfilId))
                    {
                        var nuevoEstado = MatchEstadoHelper.ConstruirPendiente(_perfilId);
                        _matchRepository.ActualizarEstado(match.IdMatch, nuevoEstado);
                        PerfilActual.EstadoMatch = nuevoEstado;
                        PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(nuevoEstado, _perfilId, otroPerfilId);
                        EstadoMensaje = $"Has enviado un corazón a {PerfilActual.NombreEdad}.";
                        return;
                    }

                    var estadoPendiente = MatchEstadoHelper.ConstruirPendiente(_perfilId);
                    _matchRepository.ActualizarEstado(match.IdMatch, estadoPendiente);
                    PerfilActual.EstadoMatch = estadoPendiente;
                    PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(estadoPendiente, _perfilId, otroPerfilId);
                    EstadoMensaje = $"Has enviado un corazón a {PerfilActual.NombreEdad}. Espera su respuesta.";
                    return;

                var pendiente = MatchEstadoHelper.ConstruirPendiente(_perfilId);
                var nuevoMatchId = _matchRepository.CrearMatch(_perfilId, otroPerfilId, pendiente);
                PerfilActual.MatchId = nuevoMatchId;
                PerfilActual.EsPerfilRegistrado = false;
                PerfilActual.EstadoMatch = pendiente;
                PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(pendiente, _perfilId, otroPerfilId);
                EstadoMensaje = $"Has enviado un corazón a {PerfilActual.NombreEdad}. Espera su respuesta.";
            }
            catch (Exception ex)
            {
                EstadoMensaje = ex.ToString();
            }
        }

        private void RechazarActual()
        {
            if (PerfilActual == null)
            {
                EstadoMensaje = "No hay un perfil seleccionado.";
                return;
            }

            try
            {
                var otroPerfilId = PerfilActual.PerfilId;
                if (otroPerfilId == 0)
                {
                    EstadoMensaje = "No se pudo identificar el perfil seleccionado.";
                    return;
                }

                var match = PerfilActual.MatchId > 0
                    ? _matchRepository.ObtenerPorId(PerfilActual.MatchId)
                    : _matchRepository.ObtenerPorPerfiles(_perfilId, otroPerfilId);

                if (match == null)
                {
                    var estadoRechazado = MatchEstadoHelper.ConstruirRechazado(_perfilId);
                    var nuevoMatchId = _matchRepository.CrearMatch(_perfilId, otroPerfilId, estadoRechazado);
                    PerfilActual.MatchId = nuevoMatchId;
                    PerfilActual.EstadoMatch = estadoRechazado;
                    PerfilActual.EsPerfilRegistrado = false;
                    PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(estadoRechazado, _perfilId, otroPerfilId);
                    EstadoMensaje = $"Has rechazado a {PerfilActual.NombreEdad}";
                    return;
                }

                PerfilActual.MatchId = match.IdMatch;
                PerfilActual.EsPerfilRegistrado = false;
                var estadoActual = match.Estado ?? string.Empty;

                if (MatchEstadoHelper.EsRechazadoPor(estadoActual, _perfilId))
                {
                    PerfilActual.EstadoMatch = estadoActual;
                    PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(estadoActual, _perfilId, otroPerfilId);
                    EstadoMensaje = $"Ya habías rechazado a {PerfilActual.NombreEdad}.";
                    return;
                }

                var nuevoEstado = MatchEstadoHelper.ConstruirRechazado(_perfilId);
                _matchRepository.ActualizarEstado(match.IdMatch, nuevoEstado);
                PerfilActual.EstadoMatch = nuevoEstado;
                PerfilActual.CarreraTexto = MatchEstadoHelper.ObtenerDescripcionPara(nuevoEstado, _perfilId, otroPerfilId);
                EstadoMensaje = $"Has rechazado a {PerfilActual.NombreEdad}";
            }
            catch (Exception ex)
            {
                EstadoMensaje = $"No se pudo completar la acción: {ex.Message}";
            }
        }

        private void BloquearActual()
        {
            if (PerfilActual?.MatchId > 0)
            {
                _matchRepository.EliminarMatch(PerfilActual.MatchId);
            }
            _sugerencias.Remove(PerfilActual);
            PerfilActual = _sugerencias.Count > 0 ? _sugerencias[Math.Min(_currentIndex, _sugerencias.Count - 1)] : null;

            if (PerfilActual == null)
            {
                _currentIndex = 0;
            }
            else
            {
                _currentIndex = _sugerencias.IndexOf(PerfilActual);
            }

            EstadoMensaje = "El perfil se eliminó de tu bandeja.";
        }

        private void MoverAnterior()
        {
            if (_sugerencias.Count == 0)
            {
                return;
            }

            _currentIndex = (_currentIndex - 1 + _sugerencias.Count) % _sugerencias.Count;
            PerfilActual = _sugerencias[_currentIndex];
        }

        private void MoverSiguiente()
        {
            if (_sugerencias.Count == 0)
            {
                return;
            }

            _currentIndex = (_currentIndex + 1) % _sugerencias.Count;
            PerfilActual = _sugerencias[_currentIndex];
        }

        /// <summary>
        /// Convierte la imagen binaria de un perfil a un recurso visual.
        /// </summary>
        private static ImageSource ConvertirAImagen(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }

    public class SugerenciaItemViewModel : BaseViewModel
    {
        private int _matchId;
        private int _perfilId;
        private string _nombreEdad = string.Empty;
        private string _carreraTexto = string.Empty;
        private ImageSource _fotoUrl;
        private bool _esPerfilRegistrado;
        private string _estadoMatch = string.Empty;

        public int MatchId
        {
            get => _matchId;
            set => EstablecerPropiedad(ref _matchId, value);
        }

        public int PerfilId
        {
            get => _perfilId;
            set => EstablecerPropiedad(ref _perfilId, value);
        }

        public string NombreEdad
        {
            get => _nombreEdad;
            set => EstablecerPropiedad(ref _nombreEdad, value);
        }

        public string CarreraTexto
        {
            get => _carreraTexto;
            set => EstablecerPropiedad(ref _carreraTexto, value);
        }

        public ImageSource FotoUrl
        {
            get => _fotoUrl;
            set => EstablecerPropiedad(ref _fotoUrl, value);
        }

        public bool EsPerfilRegistrado
        {
            get => _esPerfilRegistrado;
            set => EstablecerPropiedad(ref _esPerfilRegistrado, value);
        }

        public string EstadoMatch
        {
            get => _estadoMatch;
            set => EstablecerPropiedad(ref _estadoMatch, value);
        }
    }
}
