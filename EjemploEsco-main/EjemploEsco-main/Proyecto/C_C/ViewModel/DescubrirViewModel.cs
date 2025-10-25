using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using C_C.Model;
using C_C.Repositories;
using C_C.Services;

namespace C_C.ViewModel
{
    public class DescubrirViewModel : ViewModelBase
    {
        private readonly DiscoveryService _discoveryService;
        private readonly MatchService _matchService;
        private readonly ObservableCollection<PerfilModel> _perfilesDisponibles = new ObservableCollection<PerfilModel>();

        private PerfilModel _perfilActual;
        private string _mensajeEstado;

        public DescubrirViewModel()
        {
            var userRepository = new UserRepository();
            var perfilRepository = new PerfilRepository();
            var matchRepository = new MatchRepository();
            var chatRepository = new ChatRepository();

            _discoveryService = new DiscoveryService(perfilRepository, userRepository);
            _matchService = new MatchService(matchRepository, chatRepository);

            LikeCommand = new ViewModelCommand(ExecuteLike, CanExecutePerfilActual);
            SkipCommand = new ViewModelCommand(ExecuteSkip, CanExecutePerfilActual);

            CargarPerfiles();
        }

        public PerfilModel PerfilActual
        {
            get => _perfilActual;
            set
            {
                _perfilActual = value;
                OnPropertyChanged(nameof(PerfilActual));
            }
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set
            {
                _mensajeEstado = value;
                OnPropertyChanged(nameof(MensajeEstado));
            }
        }

        public ICommand LikeCommand { get; }

        public ICommand SkipCommand { get; }

        private void CargarPerfiles()
        {
            _perfilesDisponibles.Clear();
            var sugerencias = _discoveryService.ObtenerSugerencias(UserSession.Instance.CurrentUserId);
            foreach (var perfil in sugerencias)
            {
                _perfilesDisponibles.Add(perfil);
            }

            PerfilActual = _perfilesDisponibles.FirstOrDefault();
        }

        private bool CanExecutePerfilActual(object parameter)
        {
            return PerfilActual != null;
        }

        private void ExecuteLike(object parameter)
        {
            var match = _matchService.RegistrarLike(UserSession.Instance.CurrentUserId, PerfilActual.UserId);
            MensajeEstado = match != null ? "¡Es un match!" : string.Empty;
            AvanzarPerfil();
        }

        private void ExecuteSkip(object parameter)
        {
            MensajeEstado = "Has omitido este perfil";
            AvanzarPerfil();
        }

        private void AvanzarPerfil()
        {
            if (PerfilActual != null)
            {
                _perfilesDisponibles.Remove(PerfilActual);
            }

            PerfilActual = _perfilesDisponibles.FirstOrDefault();
        }
    }
}
