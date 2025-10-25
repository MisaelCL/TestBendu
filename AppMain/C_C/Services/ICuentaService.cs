using C_C.Model;

namespace C_C.Services;

public interface ICuentaService
{
    Task<Cuenta?> ObtenerPorEmailAsync(string email, CancellationToken ct = default);
    Task<int> RegistrarAsync(Cuenta cuenta, string plainPassword, CancellationToken ct = default);
}
