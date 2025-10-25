using ProyectoEjemplo.Model;
using ProyectoEjemplo.Repositories;
using ProyectoEjemplo.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProyectoEjemplo.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        //Campos 
        private string _username; //almacena el usuario
        private SecureString _password; //almacena la contraseña de forma segura
        private string _errorMessage; //Guarda el mensaje de error si el login falla
        private bool _isViewVisible = true; //controla si la venta de login está visible
        private IUserRepository _userRepository;

        //Propiedades 
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public SecureString Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string ErrorMessage
        {
            get 
            { 
                return _errorMessage; 
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));            }
        }

        public bool IsViewVisible
        {
            get
            {
                return _isViewVisible;
            }
            set
            {
                _isViewVisible = value;
                OnPropertyChanged(nameof(IsViewVisible));
            }
        }

        //Comandos 
        public ICommand LoginCommand { get; } //Se ejecuta cuando el usuario hace click en login
        public ICommand ShowPasswordCommand { get; } //Mostrar/ocultar la contraseña
        public ICommand OpenRegistroCommand { get; }
        //Constructor
        public LoginViewModel()
        {
            _userRepository = new UserRepository();
            LoginCommand = new ViewModelCommand(
                ExecuteLoginCommand,
                CanExecuteLoginCommand);
            OpenRegistroCommand = new ViewModelCommand(
                ExecuteOpenRegistroCommand);
        }

        private bool CanExecuteLoginCommand(object obj)
        {
            bool validData;
            if (string.IsNullOrWhiteSpace(Username)
                || Username.Length < 3
                || Password == null
                || Password.Length < 3)
                validData = false;
            else
                validData = true;
            return validData;
        }

        private void ExecuteLoginCommand(object obj)
        {
            var isValidUser = _userRepository.AuthenticateUser(
                new System.Net.NetworkCredential(Username, Password));
            if (isValidUser)
            {
                Thread.CurrentPrincipal = new GenericPrincipal(
                    new GenericIdentity(Username), null);

                OpenRegistroView();
            }
            else
            {
                ErrorMessage = "* Invalid username or password";
            }
        }

        private void ExecuteOpenRegistroCommand(object obj)
        {
            OpenRegistroView();
        }

        private void OpenRegistroView()
        {
            var registroView = new RegistroView();

            Application.Current.MainWindow = registroView;

            registroView.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window != registroView)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
}
