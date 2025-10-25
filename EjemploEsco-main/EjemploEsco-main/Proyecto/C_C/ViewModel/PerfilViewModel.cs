using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using C_C.Model;
using C_C.Repositories;

namespace C_C.ViewModel
{
    public class PerfilViewModel : ViewModelBase
    {
        private readonly IPerfilRepository _perfilRepository;
        private PerfilModel _perfil;
        private ObservableCollection<string> _intereses;

        public PerfilViewModel()
        {
            _perfilRepository = new PerfilRepository();
            GuardarPerfilCommand = new ViewModelCommand(ExecuteGuardarPerfil, CanExecuteGuardarPerfil);
            CargarPerfil();
        }

        public PerfilModel Perfil
        {
            get => _perfil;
            set
            {
                _perfil = value;
                OnPropertyChanged(nameof(Perfil));
            }
        }

        public ObservableCollection<string> Intereses
        {
            get => _intereses;
            set
            {
                _intereses = value;
                OnPropertyChanged(nameof(Intereses));
            }
        }

        public ICommand GuardarPerfilCommand { get; }

        private void CargarPerfil()
        {
            Perfil = _perfilRepository.GetByUserId(UserSession.Instance.CurrentUserId)
                ?? new PerfilModel { UserId = UserSession.Instance.CurrentUserId, FechaNacimiento = DateTime.UtcNow.AddYears(-18) };
            Intereses = new ObservableCollection<string>(Perfil.Intereses);
        }

        private bool CanExecuteGuardarPerfil(object parameter)
        {
            return Perfil != null
                && !string.IsNullOrWhiteSpace(Perfil.Nombre)
                && Perfil.FechaNacimiento < DateTime.Today;
        }

        private void ExecuteGuardarPerfil(object parameter)
        {
            Perfil.Intereses = Intereses.ToList();
            _perfilRepository.Save(Perfil);
        }
    }
}
