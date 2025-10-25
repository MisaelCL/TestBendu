using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using C_C.Model;

namespace C_C.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {
        private static readonly List<UserModel> Users = new List<UserModel>();

        public UserRepository()
        {
            if (!Users.Any())
            {
                var seedUser = new UserModel
                {
                    Id = Guid.NewGuid(),
                    Username = "demo",
                    Email = "demo@claseycita.com",
                    PasswordHash = "demo",
                    Salt = string.Empty,
                    RegisteredAt = DateTime.UtcNow
                };
                Users.Add(seedUser);
            }
        }

        public void Add(UserModel userModel)
        {
            userModel.Id = userModel.Id == Guid.Empty ? Guid.NewGuid() : userModel.Id;
            userModel.RegisteredAt = DateTime.UtcNow;
            Users.Add(userModel);
        }

        public bool AuthenticateUser(NetworkCredential credential)
        {
            return Users.Any(user =>
                user.Username.Equals(credential.UserName, StringComparison.OrdinalIgnoreCase)
                && user.PasswordHash == credential.Password);
        }

        public IEnumerable<UserModel> GetAll()
        {
            return Users;
        }

        public UserModel GetByUsername(string username)
        {
            return Users.FirstOrDefault(user =>
                user.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public void Update(UserModel userModel)
        {
            var existing = Users.FirstOrDefault(user => user.Id == userModel.Id);
            if (existing == null)
            {
                return;
            }

            existing.Email = userModel.Email;
            existing.PasswordHash = userModel.PasswordHash;
            existing.Salt = userModel.Salt;
            existing.IsBlocked = userModel.IsBlocked;
        }
    }
}
