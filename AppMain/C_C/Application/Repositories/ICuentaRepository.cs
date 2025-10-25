using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using C_C_Final.Domain.Models;

namespace C_C_Final.Application.Repositories
{
    public interface ICuentaRepository
    {
        Task<Cuenta?> GetByIdAsync(int idCuenta, CancellationToken ct = default);
        Task<Cuenta?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

        Task<int> CreateCuentaAsync(string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default);
        Task<int> CreateAlumnoAsync(Alumno alumno, CancellationToken ct = default);
        Task<bool> UpdatePasswordAsync(int idCuenta, string newPasswordHash, CancellationToken ct = default);
        Task<bool> DeleteCuentaAsync(int idCuenta, CancellationToken ct = default);

        Task<int> CreateCuentaAsync(SqlConnection cn, SqlTransaction? tx, string email, string passwordHash, byte estadoCuenta, CancellationToken ct = default);
        Task<int> CreateAlumnoAsync(SqlConnection cn, SqlTransaction? tx, Alumno alumno, CancellationToken ct = default);
    }
}
