namespace C_C.Services;

public interface IMatchService
{
    Task<int> CrearAsync(int Perfil_Emisor, int Perfil_Receptor, string Estado, CancellationToken ct = default);
    Task<int> ActualizarEstadoAsync(int ID_Match, string Estado, CancellationToken ct = default);
    Task<bool> ExisteParAsync(int a, int b, CancellationToken ct = default);
}
