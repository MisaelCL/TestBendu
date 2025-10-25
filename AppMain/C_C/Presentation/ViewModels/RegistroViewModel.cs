using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Application.Services;
using C_C_Final.Presentation.Commands;

namespace C_C_Final.Presentation.ViewModels
{
    public sealed class RegistroViewModel : BaseViewModel
    {
        private readonly RegisterAlumnoService _registerAlumnoService;
        private readonly ObservableCollection<string> _carreras = new ObservableCollection<string>(new[]
        {
            "Ingeniería en Sistemas",
            "Ingeniería Industrial",
            "Ingeniería Civil",
            "Licenciatura en Administración",
            "Licenciatura en Mercadotecnia"
        });

        private readonly ObservableCollection<string> _generos = new ObservableCollection<string>(new[] { "Masculino", "Femenino" });
        private string _carreraSeleccionada;
        private string _matricula = string.Empty;
        private string _nombreCompleto = string.Empty;
        private string _generoSeleccionado;
        private DateTime? _fechaNacimiento = DateTime.Today;
        private string _correo = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage;
        private bool _isBusy;

        public RegistroViewModel(RegisterAlumnoService registerAlumnoService)
        {
            _registerAlumnoService = registerAlumnoService;
            RegisterCommand = new RelayCommand(async _ => await RegistrarAsync(), _ => !IsBusy);
        }

        public ObservableCollection<string> Carreras => _carreras;

        public string CarreraSeleccionada
        {
            get => _carreraSeleccionada;
            set => SetProperty(ref _carreraSeleccionada, value);
        }

        public string Matricula
        {
            get => _matricula;
            set => SetProperty(ref _matricula, value);
        }

        public string NombreCompleto
        {
            get => _nombreCompleto;
            set => SetProperty(ref _nombreCompleto, value);
        }

        public ObservableCollection<string> Generos => _generos;

        public string GeneroSeleccionado
        {
            get => _generoSeleccionado;
            set => SetProperty(ref _generoSeleccionado, value);
        }

        public DateTime FechaNacimiento
        {
            get => (DateTime)_fechaNacimiento;
            set => SetProperty(ref _fechaNacimiento, value);
        }

        public string Correo
        {
            get => _correo;
            set => SetProperty(ref _correo, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand RegisterCommand { get; }

        private async Task RegistrarAsync()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                if (string.IsNullOrWhiteSpace(Correo))
                {
                    ErrorMessage = "El correo es obligatorio";
                    return;
                }

                if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
                {
                    ErrorMessage = "Las contraseñas no coinciden";
                    return;
                }

                if (!_fechaNacimiento.HasValue)
                {
                    ErrorMessage = "La fecha de nacimiento es obligatoria";
                    return;
                }

                var genero = GeneroSeleccionado?.StartsWith("M", StringComparison.OrdinalIgnoreCase) == true ? 'M' : 'F';
                var (nombre, apaterno, amaterno) = ParseNombre(NombreCompleto);

                var request = new RegisterAlumnoRequest
                {
                    Email = Correo,
                    Password = Password,
                    EstadoCuenta = 1,
                    Matricula = Matricula,
                    Nombre = nombre,
                    ApellidoPaterno = apaterno,
                    ApellidoMaterno = amaterno,
                    FechaNacimiento = _fechaNacimiento.Value,
                    Genero = genero,
                    CorreoAlumno = Correo,
                    Carrera = CarreraSeleccionada ?? string.Empty,
                    Biografia = string.Empty
                };

                await _registerAlumnoService.RegisterAsync(request, CancellationToken.None);

                Password = string.Empty;
                ConfirmPassword = string.Empty;
                MessageBox.Show("Registro completado", "Registro", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static (string Nombre, string Apaterno, string Amaterno) ParseNombre(string nombreCompleto)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
            {
                return ("", "", "");
            }

            var partes = nombreCompleto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0)
            {
                return ("", "", "");
            }

            var nombre = partes[0];
            var apaterno = partes.Length > 1 ? partes[1] : string.Empty;
            var amaterno = partes.Length > 2 ? partes[2] : string.Empty;
            return (nombre, apaterno, amaterno);
        }
    }
}
