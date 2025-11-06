using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Services;
using System.Net.Mail;
using C_C_Final.View;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Gestiona el proceso de registro de nuevos alumnos y valida la información capturada.
    /// </summary>
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
        private string _nombre = string.Empty;
        private string _apellidoPaterno = string.Empty;
        private string _apellidoMaterno = string.Empty;
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
            // Los comandos exponen las acciones principales de la vista y se bloquean mientras IsBusy es true.
            ComandoRegistrar = new RelayCommand(_ => Registrar(), _ => !IsBusy);
            ComandoRegresar = new RelayCommand(_ => RegresarALogin(), _ => !IsBusy);
        }

        public ObservableCollection<string> Carreras => _carreras;

        public string CarreraSeleccionada
        {
            get => _carreraSeleccionada;
            set => EstablecerPropiedad(ref _carreraSeleccionada, value);
        }

        public string Matricula
        {
            get => _matricula;
            set => EstablecerPropiedad(ref _matricula, value);
        }

        public string Nombre
        {
            get => _nombre;
            set => EstablecerPropiedad(ref _nombre, value);
        }

        public string ApellidoPaterno
        {
            get => _apellidoPaterno;
            set => EstablecerPropiedad(ref _apellidoPaterno, value);
        }

        public string ApellidoMaterno
        {
            get => _apellidoMaterno;
            set => EstablecerPropiedad(ref _apellidoMaterno, value);
        }

        public ObservableCollection<string> Generos => _generos;

        public string GeneroSeleccionado
        {
            get => _generoSeleccionado;
            set => EstablecerPropiedad(ref _generoSeleccionado, value);
        }

        public DateTime FechaNacimiento
        {
            get => (DateTime)_fechaNacimiento;
            set => EstablecerPropiedad(ref _fechaNacimiento, value);
        }

        public string Correo
        {
            get => _correo;
            set => EstablecerPropiedad(ref _correo, value);
        }

        public string Password
        {
            get => _password;
            set => EstablecerPropiedad(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => EstablecerPropiedad(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => EstablecerPropiedad(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (EstablecerPropiedad(ref _isBusy, value))
                {
                    (ComandoRegistrar as RelayCommand)?.NotificarCambioPuedeEjecutar();
                    (ComandoRegresar as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        public ICommand ComandoRegistrar { get; }
        public ICommand ComandoRegresar { get; }

        private void Registrar()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                // 1. Validaciones de datos mínimos.
                if (string.IsNullOrWhiteSpace(Correo))
                {
                    ErrorMessage = "El correo es obligatorio";
                    return;
                }

                if (!_fechaNacimiento.HasValue)
                {
                    ErrorMessage = "La fecha de nacimiento es obligatoria";
                    return;
                }

                // 2. Comprobaciones de formato y reglas de negocio adicionales.
                if (!EsCorreoValido(Correo))
                {
                    ErrorMessage = "El formato del correo electrónico no es válido (ej. usuario@dominio.com)";
                    return;
                }

                
                int edad = CalcularEdad(_fechaNacimiento.Value);
                if (edad < 18)
                {
                    ErrorMessage = "Debes ser mayor de 18 años para registrarte.";
                    return;
                }

                if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
                {
                    ErrorMessage = "Las contraseñas no coinciden";
                    return;
                }

                
                var genero = GeneroSeleccionado?.StartsWith("M", StringComparison.OrdinalIgnoreCase) == true ? 'M' : 'F';
                var nombre = Nombre?.Trim() ?? string.Empty;
                var apaterno = ApellidoPaterno?.Trim() ?? string.Empty;
                var amaterno = ApellidoMaterno?.Trim() ?? string.Empty;

                // 3. Construcción del DTO que el servicio utilizará para persistir la información.
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

                // 4. Invocación del servicio transaccional; devuelve el ID de cuenta creado.
                var cuentaId = _registerAlumnoService.Registrar(request);

                if (cuentaId <= 0)
                {
                    ErrorMessage = "No se pudo completar el registro.";
                    return;
                }

                // 5. Retroalimentación visual y limpieza de campos sensibles.
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                MessageBox.Show("Registro completado", "Registro", MessageBoxButton.OK, MessageBoxImage.Information);
                AbrirLoginYCerrarVentanaActual();
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

        /// <summary>
        ///     Maneja la acción de regresar a la pantalla previa sin completar el registro.
        /// </summary>
        private void RegresarALogin()
        {
            AbrirLoginYCerrarVentanaActual();
        }

        /// <summary>
        ///     Vuelve a la pantalla de inicio de sesión reutilizando las dependencias configuradas en <see cref="App"/>.
        /// </summary>
        private void AbrirLoginYCerrarVentanaActual()
        {
            var app = App.Current;
            if (app == null)
            {
                return;
            }

            var login = new LoginView();
            if (login.DataContext is not LoginViewModel)
            {
                // Se reutilizan los repositorios instanciados en el composition root.
                login.DataContext = new LoginViewModel(app.CuentaRepository, app.PerfilRepository);
            }

            app.MainWindow = login;
            login.Show();
            CerrarVentanaAsociada();
        }

        /// <summary>
        ///     Cierra la ventana cuya DataContext coincide con este ViewModel.
        /// </summary>
        private void CerrarVentanaAsociada()
        {
            var app = Application.Current;
            if (app == null)
            {
                return;
            }

            foreach (Window window in app.Windows)
            {
                if (Equals(window.DataContext, this))
                {
                    window.Close();
                    break;
                }
            }
        }



        /// <summary>
        ///     Valida el formato general de un correo electrónico utilizando <see cref="MailAddress"/>.
        /// </summary>
        private bool EsCorreoValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        
        /// <summary>
        ///     Calcula la edad a partir de la fecha de nacimiento, considerando si el cumpleaños ya ocurrió este año.
        /// </summary>
        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var today = DateTime.Today;
            var edad = today.Year - fechaNacimiento.Year;

            
            if (fechaNacimiento.Date > today.AddYears(-edad))
            {
                edad--;
            }
            return edad;
        }


        
    }
}
