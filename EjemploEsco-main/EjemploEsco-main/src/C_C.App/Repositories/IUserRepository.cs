using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;

namespace C_C.App.Repositories;

public interface IUserRepository : IRepositoryBase<UserModel>
{
    Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
