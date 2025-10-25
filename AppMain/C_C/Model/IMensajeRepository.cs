using System.Collections.Generic;
using System.Threading;
using Microsoft.Data.SqlClient;

namespace C_C.Model;

public interface IMensajeRepository
{
    Task<long> EnviarAsync(Mensaje msg, CancellationToken ct = default);
    Task<long> EnviarAsync(Mensaje msg, SqlConnection connection, SqlTransaction? transaction, CancellationToken ct = default);
    Task<int> MarcarLeidoAsync(long ID_Mensaje, CancellationToken ct = default);
    Task<int> EditarContenidoAsync(long ID_Mensaje, string nuevo, CancellationToken ct = default);
    Task<int> BorradoLogicoAsync(long ID_Mensaje, CancellationToken ct = default);
    Task<IReadOnlyList<Mensaje>> ObtenerPaginaAsync(int ID_Chat, int top, DateTime? anchorFecha, long? anchorId, CancellationToken ct = default);
    Task<int> ContarNoLeidosAsync(int ID_Chat, int ID_Perfil, CancellationToken ct = default);
}
