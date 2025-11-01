using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Model;
using C_C_Final.Resources.Utils;
using C_C_Final.View;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Gestiona el proceso de inicio de sesión y navegación desde la vista de acceso.
    /// </summary>
    public sealed class LoginViewModel : BaseViewModel
    {
        private readonly ICuentaRepository _cuentaRepository;
        private readonly IPerfilRepository _perfilRepository;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage;
        private bool _isViewVisible = true;
        private bool _isBusy;

        public LoginViewModel(ICuentaRepository cuentaRepository, IPerfilRepository perfilRepository)
        {
            _cuentaRepository = cuentaRepository ?? throw new ArgumentNullException(nameof(cuentaRepository));
            _perfilRepository = perfilRepository ?? throw new ArgumentNullException(nameof(perfilRepository));

            ComandoIniciarSesion = new RelayCommand(_ => IniciarSesion(), _ => !IsBusy);
            ComandoAbrirRegistro = new RelayCommand(_ => AbrirRegistro(), _ => !IsBusy);
        }

        public string Username
        {
            get => _username;
            set => EstablecerPropiedad(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => EstablecerPropiedad(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => EstablecerPropiedad(ref _errorMessage, value);
        }

        public bool IsViewVisible
        {
            get => _isViewVisible;
            set => EstablecerPropiedad(ref _isViewVisible, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (EstablecerPropiedad(ref _isBusy, value))
                {
                    (ComandoIniciarSesion as RelayCommand)?.NotificarCambioPuedeEjecutar();
                    (ComandoAbrirRegistro as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        public ICommand ComandoIniciarSesion { get; }
        public ICommand ComandoAbrirRegistro { get; }

        /// <summary>
        /// Valida las credenciales proporcionadas e inicia sesión en la aplicación.
        /// </summary>
        private void IniciarSesion()
        {
            try
            {
                ErrorMessage = null;

                var email = Username?.Trim();
                var password = Password ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage = "Ingresa tu usuario y contraseña";
                    return;
                }

                var cuenta = _cuentaRepository.ObtenerPorCorreo(email);
                if (cuenta == null)
                {
                    ErrorMessage = "El usuario no existe.";
                    return;
                }

                var passwordHash = HashFunction.ComputeHash(password);
                if (cuenta.HashContrasena != passwordHash)
                {
                    ErrorMessage = "Contraseña incorrecta.";
                    return;
                }

                if (cuenta.EstadoCuenta == 0)
                {
                    ErrorMessage = "Tu cuenta está inactiva. Contacta al administrador.";
                    return;
                }

                var perfil = _perfilRepository.ObtenerPorCuentaId(cuenta.IdCuenta);
                if (perfil == null)
                {
                    ErrorMessage = "No se encontró un perfil asociado a la cuenta.";
                    return;
                }

                Username = email;
                Password = string.Empty;
                ErrorMessage = null;
                IsViewVisible = false;

                AbrirHome(perfil.IdPerfil);
                CerrarVentanaAsociada();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Abre la ventana de registro de nuevos alumnos.
        /// </summary>
        private void AbrirRegistro()
        {
            var app = App.Current;
            if (app == null)
            {
                return;
            }

            var registro = new RegistroView
            {
                DataContext = new RegistroViewModel(app.RegisterAlumnoService)
            };
            registro.Show();
        }

        /// <summary>
        /// Abre la ventana principal del usuario autenticado.
        /// </summary>
        private static void AbrirHome(int perfilId)
        {
            var home = new HomeView(perfilId);
            home.Show();
        }

        /// <summary>
        /// Cierra la ventana asociada a este modelo de vista.
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
    }
}
