using System;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;

namespace C_C.App.Repositories;

public interface IPerfilRepository : IRepositoryBase<PerfilModel>
{
    Task<PerfilModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PreferenciasModel?> GetPreferenciasByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SavePreferenciasAsync(PreferenciasModel preferencias, CancellationToken cancellationToken = default);
}
