using C_C.Model;

namespace C_C.Services;

public interface IPerfilService
{
    Task<Perfil?> GetAsync(int ID_Perfil, CancellationToken ct = default);
    Task<Perfil?> GetByNickAsync(string nik, CancellationToken ct = default);
    Task<int> UpdateAsync(Perfil p, CancellationToken ct = default);
}
