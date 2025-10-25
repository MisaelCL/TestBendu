using System.Threading;

namespace C_C.Model;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task<int> InsertAsync(T entity, CancellationToken ct = default);
    Task<int> UpdateAsync(T entity, CancellationToken ct = default);
    Task<int> DeleteAsync(object id, CancellationToken ct = default);
}
