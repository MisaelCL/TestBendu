using System;
using System.Net;
using C_C.Model;
using C_C.Repositories;

namespace C_C.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public bool Login(string username, string password)
        {
            var credential = new NetworkCredential(username, password);
            return _userRepository.AuthenticateUser(credential);
        }

        public bool Register(UserModel user, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (_userRepository.GetByUsername(user.Username) != null)
            {
                errorMessage = "El usuario ya existe";
                return false;
            }

            user.PasswordHash = string.IsNullOrWhiteSpace(user.PasswordHash)
                ? throw new ArgumentException("La contraseña es obligatoria")
                : user.PasswordHash;

            _userRepository.Add(user);
            return true;
        }
    }
}
