using C_C.Model;

namespace C_C.Services;

public interface IMensajeService
{
    Task<long> EnviarAsync(Mensaje msg, CancellationToken ct = default);
    Task EditarAsync(long ID_Mensaje, string nuevo, CancellationToken ct = default);
    Task BorrarLogicoAsync(long ID_Mensaje, CancellationToken ct = default);
    Task MarcarLeidoAsync(long ID_Mensaje, CancellationToken ct = default);
    Task<IReadOnlyList<Mensaje>> ObtenerUltimosAsync(int ID_Chat, int top, CancellationToken ct = default);
    Task<IReadOnlyList<Mensaje>> ObtenerPaginaAnteriorAsync(int ID_Chat, int top, DateTime anchorFecha, long anchorId, CancellationToken ct = default);
}
