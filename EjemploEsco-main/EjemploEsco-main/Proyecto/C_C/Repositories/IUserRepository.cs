using System.Collections.Generic;
using System.Net;
using C_C.Model;

namespace C_C.Repositories
{
    public interface IUserRepository
    {
        bool AuthenticateUser(NetworkCredential credential);

        void Add(UserModel userModel);

        void Update(UserModel userModel);

        UserModel GetByUsername(string username);

        IEnumerable<UserModel> GetAll();
    }
}
