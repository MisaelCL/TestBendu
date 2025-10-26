using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace C_C_Final.Model
{
    public interface IPerfilRepository
    {
        Task<Perfil> GetByIdAsync(int idPerfil, CancellationToken ct = default);
        Task<Perfil> GetByCuentaIdAsync(int idCuenta, CancellationToken ct = default);
        Task<Preferencias> GetPreferenciasByPerfilAsync(int idPerfil, CancellationToken ct = default);

        Task<int> CreatePerfilAsync(Perfil perfil, CancellationToken ct = default);
        Task<bool> UpdatePerfilAsync(Perfil perfil, CancellationToken ct = default);
        Task<int> UpsertPreferenciasAsync(Preferencias prefs, CancellationToken ct = default);
        Task<bool> DeletePerfilAsync(int idPerfil, CancellationToken ct = default);

        Task<int> CreatePerfilAsync(SqlConnection cn, SqlTransaction tx, Perfil perfil, CancellationToken ct = default);
        Task<int> UpsertPreferenciasAsync(SqlConnection cn, SqlTransaction tx, Preferencias prefs, CancellationToken ct = default);
    }
}
