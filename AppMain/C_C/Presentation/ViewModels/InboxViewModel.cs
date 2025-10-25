using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Application.Repositories;
using C_C_Final.Application.Services;
using C_C_Final.Domain.Models;
using C_C_Final.Presentation.Commands;

namespace C_C_Final.Presentation.ViewModels
{
    public sealed class InboxViewModel : BaseViewModel
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPerfilRepository _perfilRepository;
        private readonly MatchService _matchService;
        private readonly ObservableCollection<SugerenciaItemViewModel> _sugerencias = new ObservableCollection<SugerenciaItemViewModel>();
        private SugerenciaItemViewModel? _perfilActual;
        private bool _isSettingsMenuOpen;
        private string? _estadoMensaje;
        private int _perfilId;
        private int _currentIndex;
        private readonly int _pageSize = 20;

        public InboxViewModel(IMatchRepository matchRepository, IPerfilRepository perfilRepository, MatchService matchService)
        {
            _matchRepository = matchRepository;
            _perfilRepository = perfilRepository;
            _matchService = matchService;

            OpenSettingsCommand = new RelayCommand(_ => IsSettingsMenuOpen = !IsSettingsMenuOpen);
            GoMiPerfilCommand = new RelayCommand(_ => EstadoMensaje = "Abre tu perfil para editar tu información");
            BloquearPerfilCommand = new RelayCommand(async _ => await BloquearActualAsync());
            PrevCommand = new RelayCommand(_ => MoverAnterior());
            NextCommand = new RelayCommand(_ => MoverSiguiente());
            LikeCommand = new RelayCommand(async _ => await AceptarActualAsync());
            DislikeCommand = new RelayCommand(async _ => await RechazarActualAsync());
        }

        public ObservableCollection<SugerenciaItemViewModel> Sugerencias => _sugerencias;

        public SugerenciaItemViewModel? PerfilActual
        {
            get => _perfilActual;
            private set => SetProperty(ref _perfilActual, value);
        }

        public bool IsSettingsMenuOpen
        {
            get => _isSettingsMenuOpen;
            set => SetProperty(ref _isSettingsMenuOpen, value);
        }

        public string? EstadoMensaje
        {
            get => _estadoMensaje;
            private set => SetProperty(ref _estadoMensaje, value);
        }

        public ICommand OpenSettingsCommand { get; }
        public ICommand GoMiPerfilCommand { get; }
        public ICommand BloquearPerfilCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand LikeCommand { get; }
        public ICommand DislikeCommand { get; }

        public async Task LoadAsync(int perfilId, CancellationToken ct = default)
        {
            _perfilId = perfilId;
            _sugerencias.Clear();
            _currentIndex = 0;

            var matches = await _matchRepository.ListByPerfilAsync(perfilId, 0, _pageSize, ct);
            foreach (var match in matches.OrderByDescending(m => m.FechaMatch))
            {
                var otherPerfilId = match.PerfilEmisor == perfilId ? match.PerfilReceptor : match.PerfilEmisor;
                var perfil = await _perfilRepository.GetByIdAsync(otherPerfilId, ct);
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

        private async Task AceptarActualAsync()
        {
            if (PerfilActual == null)
            {
                return;
            }

            await _matchRepository.UpdateEstadoAsync(PerfilActual.MatchId, "aceptado", CancellationToken.None);
            await _matchService.EnsureChatForMatchAsync(PerfilActual.MatchId, CancellationToken.None);
            EstadoMensaje = $"Has aceptado a {PerfilActual.NombreEdad}";
        }

        private async Task RechazarActualAsync()
        {
            if (PerfilActual == null)
            {
                return;
            }

            await _matchRepository.UpdateEstadoAsync(PerfilActual.MatchId, "rechazado", CancellationToken.None);
            EstadoMensaje = $"Has rechazado a {PerfilActual.NombreEdad}";
        }

        private async Task BloquearActualAsync()
        {
            if (PerfilActual == null)
            {
                return;
            }

            await _matchRepository.DeleteMatchAsync(PerfilActual.MatchId, CancellationToken.None);
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

        private static ImageSource? ConvertToImage(byte[]? bytes)
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

    public sealed class SugerenciaItemViewModel : BaseViewModel
    {
        public int MatchId { get; set; }
        public int PerfilId { get; set; }
        public string NombreEdad { get; set; } = string.Empty;
        public string CarreraTexto { get; set; } = string.Empty;
        public ImageSource? FotoUrl { get; set; }
    }
}
