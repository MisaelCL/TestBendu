using System;
using System.Windows;
using System.Windows.Input;
using C_C.Model;
using C_C.Repositories;
using C_C.Services;
using C_C.View;

namespace C_C.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;
        private readonly IUserRepository _userRepository;

        private string _username;
        private string _password;
        private string _errorMessage;

        public LoginViewModel()
        {
            _userRepository = new UserRepository();
            _authService = new AuthService(_userRepository);
            _navigationService = new NavigationService();

            LoginCommand = new ViewModelCommand(ExecuteLogin, CanExecuteLogin);
            OpenRegistroCommand = new ViewModelCommand(ExecuteOpenRegistro);
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
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

        public ICommand LoginCommand { get; }

        public ICommand OpenRegistroCommand { get; }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Password);
        }

        private void ExecuteLogin(object parameter)
        {
            var isValid = _authService.Login(Username, Password);
            if (!isValid)
            {
                ErrorMessage = "Credenciales inválidas";
                return;
            }

            ErrorMessage = string.Empty;
            var usuario = _userRepository.GetByUsername(Username);
            UserSession.Instance.CurrentUserId = usuario.Id;

            var homeView = new HomeView();
            _navigationService.Navigate(Application.Current.MainWindow, homeView);
            Application.Current.MainWindow = homeView;
        }

        private void ExecuteOpenRegistro(object parameter)
        {
            var registroView = new RegistroView();
            _navigationService.Navigate(Application.Current.MainWindow, registroView);
            Application.Current.MainWindow = registroView;
        }
    }
}
