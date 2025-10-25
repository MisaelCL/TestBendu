using System;
using System.Windows.Input;
using C_C.Model;

namespace C_C.ViewModel
{
    public class PreferenciasViewModel : ViewModelBase
    {
        private readonly PreferenciasRepository _preferenciasRepository = new PreferenciasRepository();
        private PreferenciasModel _preferencias;

        public PreferenciasViewModel()
        {
            GuardarPreferenciasCommand = new ViewModelCommand(ExecuteGuardar, CanExecuteGuardar);
            CargarPreferencias();
        }

        public PreferenciasModel Preferencias
        {
            get => _preferencias;
            set
            {
                _preferencias = value;
                OnPropertyChanged(nameof(Preferencias));
            }
        }

        public ICommand GuardarPreferenciasCommand { get; }

        private void CargarPreferencias()
        {
            Preferencias = _preferenciasRepository.GetByUserId(UserSession.Instance.CurrentUserId)
                ?? new PreferenciasModel
                {
                    UserId = UserSession.Instance.CurrentUserId,
                    EdadMinima = 18,
                    EdadMaxima = 30,
                    DistanciaMaximaKm = 10
                };
        }

        private bool CanExecuteGuardar(object parameter)
        {
            return Preferencias != null
                && Preferencias.EdadMinima >= 18
                && Preferencias.EdadMaxima >= Preferencias.EdadMinima;
        }

        private void ExecuteGuardar(object parameter)
        {
            _preferenciasRepository.Save(Preferencias);
        }

        private class PreferenciasRepository
        {
            private static readonly System.Collections.Generic.List<PreferenciasModel> Preferencias = new System.Collections.Generic.List<PreferenciasModel>();

            public PreferenciasModel GetByUserId(Guid userId)
            {
                return Preferencias.Find(pref => pref.UserId == userId);
            }

            public void Save(PreferenciasModel preferencias)
            {
                var existing = GetByUserId(preferencias.UserId);
                if (existing == null)
                {
                    preferencias.Id = preferencias.Id == Guid.Empty ? Guid.NewGuid() : preferencias.Id;
                    Preferencias.Add(preferencias);
                    return;
                }

                existing.EdadMinima = preferencias.EdadMinima;
                existing.EdadMaxima = preferencias.EdadMaxima;
                existing.CarreraObjetivo = preferencias.CarreraObjetivo;
                existing.InteresesDeseados = preferencias.InteresesDeseados;
                existing.DistanciaMaximaKm = preferencias.DistanciaMaximaKm;
            }
        }
    }
}
