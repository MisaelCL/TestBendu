using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Presentation.Commands;
using C_C_Final.Presentation.Helpers;
using C_C_Final.View;

namespace C_C_Final.Presentation.ViewModels
{
    public sealed class LoginViewModel : BaseViewModel
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage;
        private bool _isViewVisible = true;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(async _ => await LoginAsync());
            OpenRegistroCommand = new RelayCommand(_ => AbrirRegistro());
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

        public ICommand LoginCommand { get; }
        public ICommand OpenRegistroCommand { get; }

        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = null;
                await Task.Delay(300);
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Ingresa tu usuario y contraseña";
                    return;
                }

                MessageBox.Show("Autenticación simulada. Implementa la verificación real contra la BD.", "Login", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void AbrirRegistro()
        {
            var registro = new RegistroView
            {
                DataContext = AppBootstrapper.CreateRegistroViewModel()
            };
            registro.Show();
        }
    }
}
