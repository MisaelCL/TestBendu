using ProyectoEjemplo.Model;
using ProyectoEjemplo.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ProyectoEjemplo.ViewModel
{
    public class RegistroViewModel : ViewModelBase
    {
        private readonly RepositoryBase repositoryBase;

        private ObservableCollection<UserModel> _users;
        private UserModel _user;

        //Fields
       // private UserAccountModel _currenUserAccount;
        private IUserRepository userRepository;

        public UserModel User
        {
            get => _user;
            set
            {
                _user = value;
                OnPropertyChanged(nameof(User));
            }
        }

        public ObservableCollection<UserModel> Users
        {
            get => _users;
            set
            {
                if (_users != value)
                {
                    _users = value;
                    OnPropertyChanged(nameof(Users));
                }
            }
        }

        public RegistroViewModel()
        {
            userRepository = new UserRepository();
            _user = new UserModel();
        }

        public ICommand AddCommand
        {
            get
            {
                return new ViewModelCommand(AddExecute, AddCanExecute);
            }
        }

        public void AddExecute(object user)
        {
            //Validar campos vacíos 
            if (string.IsNullOrWhiteSpace(User.Username) ||
                string.IsNullOrWhiteSpace(User?.Username) ||
                string.IsNullOrWhiteSpace(User.Name) ||
                string.IsNullOrWhiteSpace(User.LastName) ||
                string.IsNullOrWhiteSpace(User.Email))
            {
                MessageBox.Show("Por favor, completa todos lo " +
                    "campos antes de guardar.",
                    "Campos incompletos",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
 
            //Validar que las contraseñas coincidan
            if (User.Password != User.ConfirmPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden. " +
                    "Por favor, verifica.",
                    "Error de contraseña",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            //Validar si el username ya existe usando GetByUsername()
            var existingUser = userRepository.GetByUsername(User.Username);
            if (existingUser != null)
            {
                MessageBox.Show("El nombre de usuario ya existe. " +
                    "Por favor, elige otro.",
                    "Usuario duplicado,",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            User.Id = Guid.NewGuid().ToString();
            userRepository.Add(User);
            MessageBox.Show("Usuario añadido correctamente. ",
                "Éxito",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

        }

        private bool AddCanExecute(object user)
        {
            return !string.IsNullOrWhiteSpace(User?.Username)  &&
                !string.IsNullOrWhiteSpace(User?.Password) &&
                !string.IsNullOrWhiteSpace(User?.Name) &&
                !string.IsNullOrWhiteSpace(User?.LastName) &&
                !string.IsNullOrWhiteSpace(User?.Email);
        }
    }
}
