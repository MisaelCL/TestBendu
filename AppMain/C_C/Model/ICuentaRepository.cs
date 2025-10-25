using System.Threading;

namespace C_C.Model;

public interface ICuentaRepository
{
    Task<Cuenta?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<int> InsertAsync(Cuenta c, CancellationToken ct = default);
}
