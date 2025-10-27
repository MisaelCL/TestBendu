using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Model;
using C_C_Final.Resources.Utils;
using C_C_Final.View;

namespace C_C_Final.ViewModel
{
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

            LoginCommand = new RelayCommand(_ => Login(), _ => !IsBusy);
            OpenRegistroCommand = new RelayCommand(_ => AbrirRegistro(), _ => !IsBusy);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public bool IsViewVisible
        {
            get => _isViewVisible;
            set => SetProperty(ref _isViewVisible, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (OpenRegistroCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenRegistroCommand { get; }

        private void Login()
        {
            if (IsBusy)
            {
                return;
            }

            try
            {
                IsBusy = true;
                ErrorMessage = null;

                var email = Username?.Trim();
                var password = Password ?? string.Empty;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ErrorMessage = "Ingresa tu usuario y contraseña";
                    return;
                }

                var cuenta = _cuentaRepository.GetByEmail(email);
                if (cuenta == null)
                {
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    return;
                }

                var passwordHash = HashFunction.ComputeHash(password);
                if (!string.Equals(cuenta.HashContrasena, passwordHash, StringComparison.Ordinal))
                {
                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    return;
                }

                if (cuenta.EstadoCuenta == 0)
                {
                    ErrorMessage = "Tu cuenta está inactiva. Contacta al administrador.";
                    return;
                }

                var perfil = _perfilRepository.GetByCuentaId(cuenta.IdCuenta);
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
            finally
            {
                IsBusy = false;
            }
        }

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

        private static void AbrirHome(int perfilId)
        {
            var home = new HomeView(perfilId);
            home.Show();
        }

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
