using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using Microsoft.EntityFrameworkCore;

namespace C_C.App.Repositories;

public class UserRepository : RepositoryBase<UserModel>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return Context.Users
            .Include(u => u.Perfil)
            .Include(u => u.Preferencias)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}
