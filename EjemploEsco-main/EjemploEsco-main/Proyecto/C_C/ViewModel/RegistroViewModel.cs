using System;
using System.Windows;
using System.Windows.Input;
using C_C.Model;
using C_C.Repositories;
using C_C.Services;
using C_C.View;

namespace C_C.ViewModel
{
    public class RegistroViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;

        private UserModel _usuario = new UserModel();
        private string _confirmPassword;
        private string _errorMessage;

        public RegistroViewModel()
        {
            var userRepository = new UserRepository();
            _authService = new AuthService(userRepository);
            _navigationService = new NavigationService();
            RegistrarCommand = new ViewModelCommand(ExecuteRegistrar, CanExecuteRegistrar);
            VolverLoginCommand = new ViewModelCommand(ExecuteVolverLogin);
        }

        public UserModel Usuario
        {
            get => _usuario;
            set
            {
                _usuario = value;
                OnPropertyChanged(nameof(Usuario));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public ICommand RegistrarCommand { get; }

        public ICommand VolverLoginCommand { get; }

        private bool CanExecuteRegistrar(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Usuario.Username)
                && !string.IsNullOrWhiteSpace(Usuario.Email)
                && !string.IsNullOrWhiteSpace(Usuario.PasswordHash)
                && !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private void ExecuteRegistrar(object parameter)
        {
            if (Usuario.PasswordHash != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden";
                return;
            }

            if (!_authService.Register(Usuario, out var error))
            {
                ErrorMessage = error;
                return;
            }

            ErrorMessage = string.Empty;
            MessageBox.Show("Cuenta creada con éxito", "Registro", MessageBoxButton.OK, MessageBoxImage.Information);
            ExecuteVolverLogin(parameter);
        }

        private void ExecuteVolverLogin(object parameter)
        {
            var login = new LoginView();
            _navigationService.Navigate(Application.Current.MainWindow, login);
            Application.Current.MainWindow = login;
        }
    }
}
