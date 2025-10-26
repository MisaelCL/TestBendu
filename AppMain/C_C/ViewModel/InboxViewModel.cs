using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Model;
using C_C_Final.Services;

namespace C_C_Final.ViewModel
{
    public sealed class InboxViewModel : BaseViewModel
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPerfilRepository _perfilRepository;
        private readonly MatchService _matchService;
        private readonly ObservableCollection<SugerenciaItemViewModel> _sugerencias = new ObservableCollection<SugerenciaItemViewModel>();
        private SugerenciaItemViewModel _perfilActual;
        private bool _isSettingsMenuOpen;
        private string _estadoMensaje;
        private int _perfilId;
        private int _currentIndex;
        private readonly int _pageSize = 20;

        public InboxViewModel(IMatchRepository matchRepository, IPerfilRepository perfilRepository, MatchService matchService)
        {
            _matchRepository = matchRepository;
            _perfilRepository = perfilRepository;
            _matchService = matchService;

            OpenSettingsCommand = new RelayCommand(_ => IsSettingsMenuOpen = !IsSettingsMenuOpen);
            GoMiPerfilCommand = new RelayCommand(_ => AbrirMiPerfil());
            BloquearPerfilCommand = new RelayCommand(_ => BloquearActual());
            PrevCommand = new RelayCommand(_ => MoverAnterior());
            NextCommand = new RelayCommand(_ => MoverSiguiente());
            LikeCommand = new RelayCommand(_ => AceptarActual());
            DislikeCommand = new RelayCommand(_ => RechazarActual());
        }

        public ObservableCollection<SugerenciaItemViewModel> Sugerencias => _sugerencias;

        public SugerenciaItemViewModel PerfilActual
        {
            get => _perfilActual;
            private set => SetProperty(ref _perfilActual, value);
        }

        public bool IsSettingsMenuOpen
        {
            get => _isSettingsMenuOpen;
            set => SetProperty(ref _isSettingsMenuOpen, value);
        }

        public string EstadoMensaje
        {
            get => _estadoMensaje;
            private set => SetProperty(ref _estadoMensaje, value);
        }

        public event Action<int> MiPerfilRequested;

        public ICommand OpenSettingsCommand { get; }
        public ICommand GoMiPerfilCommand { get; }
        public ICommand BloquearPerfilCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand LikeCommand { get; }
        public ICommand DislikeCommand { get; }

        public void Load(int perfilId)
        {
            _perfilId = perfilId;
            _sugerencias.Clear();
            _currentIndex = 0;

            var matches = _matchRepository.ListByPerfil(perfilId, 0, _pageSize);
            foreach (var match in matches.OrderByDescending(m => m.FechaMatch))
            {
                var otherPerfilId = match.PerfilEmisor == perfilId ? match.PerfilReceptor : match.PerfilEmisor;
                var perfil = _perfilRepository.GetById(otherPerfilId);
                if (perfil == null)
                {
                    continue;
                }

                var sugerencia = new SugerenciaItemViewModel
                {
                    MatchId = match.IdMatch,
                    PerfilId = otherPerfilId,
                    NombreEdad = perfil.Nikname,
                    CarreraTexto = match.Estado,
                    FotoUrl = ConvertToImage(perfil.FotoPerfil)
                };
                _sugerencias.Add(sugerencia);
            }

            PerfilActual = _sugerencias.FirstOrDefault();
            EstadoMensaje = _sugerencias.Count == 0 ? "No hay chats disponibles" : "Selecciona un perfil para interactuar.";
        }

        private void AbrirMiPerfil()
        {
            if (_perfilId == 0)
            {
                EstadoMensaje = "No se ha cargado tu perfil.";
                return;
            }

            try
            {
                var perfil = _perfilRepository.GetById(_perfilId);
                if (perfil == null)
                {
                    EstadoMensaje = "No se encontró tu perfil.";
                    return;
                }

                IsSettingsMenuOpen = false;
                EstadoMensaje = "Mostrando tu perfil.";
                MiPerfilRequested?.Invoke(perfil.IdCuenta);
            }
            catch (Exception)
            {
                EstadoMensaje = "Ocurrió un error al abrir tu perfil.";
            }
        }

        private void AceptarActual()
        {
            if (PerfilActual == null)
            {
                return;
            }

            _matchRepository.UpdateEstado(PerfilActual.MatchId, "aceptado");
            _matchService.EnsureChatForMatch(PerfilActual.MatchId);
            EstadoMensaje = $"Has aceptado a {PerfilActual.NombreEdad}";
        }

        private void RechazarActual()
        {
            if (PerfilActual == null)
            {
                return;
            }

            _matchRepository.UpdateEstado(PerfilActual.MatchId, "rechazado");
            EstadoMensaje = $"Has rechazado a {PerfilActual.NombreEdad}";
        }

        private void BloquearActual()
        {
            if (PerfilActual == null)
            {
                return;
            }

            _matchRepository.DeleteMatch(PerfilActual.MatchId);
            _sugerencias.Remove(PerfilActual);
            PerfilActual = _sugerencias.Count > 0 ? _sugerencias[Math.Min(_currentIndex, _sugerencias.Count - 1)] : null;
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

        private static ImageSource ConvertToImage(byte[] bytes)
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

    public sealed class SugerenciaItemViewModel : BaseViewModel
    {
        public int MatchId { get; set; }
        public int PerfilId { get; set; }
        public string NombreEdad { get; set; } = string.Empty;
        public string CarreraTexto { get; set; } = string.Empty;
        public ImageSource FotoUrl { get; set; }
    }
}
