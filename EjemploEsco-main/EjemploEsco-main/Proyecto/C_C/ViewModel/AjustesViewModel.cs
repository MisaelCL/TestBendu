using System.Windows.Input;
using C_C.Services;

namespace C_C.ViewModel
{
    public class AjustesViewModel : ViewModelBase
    {
        private bool _notificacionesHabilitadas = true;
        private bool _modoOscuro;

        public AjustesViewModel()
        {
            GuardarCommand = new ViewModelCommand(_ => { });
            CerrarSesionCommand = new ViewModelCommand(_ => UserSession.Instance.CurrentUserId = System.Guid.Empty);
        }

        public bool NotificacionesHabilitadas
        {
            get => _notificacionesHabilitadas;
            set
            {
                _notificacionesHabilitadas = value;
                OnPropertyChanged(nameof(NotificacionesHabilitadas));
            }
        }

        public bool ModoOscuro
        {
            get => _modoOscuro;
            set
            {
                _modoOscuro = value;
                OnPropertyChanged(nameof(ModoOscuro));
            }
        }

        public ICommand GuardarCommand { get; }

        public ICommand CerrarSesionCommand { get; }
    }
}
