using System.Threading;

namespace C_C.Model;

public interface IPerfilRepository
{
    Task<Perfil?> GetAsync(int ID_Perfil, CancellationToken ct = default);
    Task<Perfil?> GetByNickAsync(string nik, CancellationToken ct = default);
    Task<int> UpdateAsync(Perfil p, CancellationToken ct = default);
}
