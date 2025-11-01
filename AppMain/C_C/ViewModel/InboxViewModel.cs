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
        private readonly ObservableCollection<SugerenciaItemViewModel> _sugerencias = new ObservableCollection<SugerenciaItemViewModel>();
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

                var sugerencia = new SugerenciaItemViewModel
                {
                    MatchId = match.IdMatch,
                    PerfilId = otherPerfilId,
                    NombreEdad = perfil.Nikname,
                    CarreraTexto = match.Estado,
                    FotoUrl = ConvertirAImagen(perfil.FotoPerfil),
                    EsPerfilRegistrado = false
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
                    EsPerfilRegistrado = true
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
            catch (Exception)
            {
                EstadoMensaje = "Ocurrió un error al abrir tu perfil.";
            }
        }

        private void AceptarActual()
        {
            _matchRepository.ActualizarEstado(PerfilActual.MatchId, "aceptado");
            _matchService.AsegurarChatParaMatch(PerfilActual.MatchId);
            EstadoMensaje = $"Has aceptado a {PerfilActual.NombreEdad}";
        }

        private void RechazarActual()
        {
            _matchRepository.ActualizarEstado(PerfilActual.MatchId, "rechazado");
            EstadoMensaje = $"Has rechazado a {PerfilActual.NombreEdad}";
        }

        private void BloquearActual()
        {
            _matchRepository.EliminarMatch(PerfilActual.MatchId);
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
        public int MatchId { get; set; }
        public int PerfilId { get; set; }
        public string NombreEdad { get; set; } = string.Empty;
        public string CarreraTexto { get; set; } = string.Empty;
        public ImageSource FotoUrl { get; set; }
        public bool EsPerfilRegistrado { get; set; }
    }
}
