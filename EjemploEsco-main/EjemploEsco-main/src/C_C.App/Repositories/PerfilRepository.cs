using System;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using Microsoft.EntityFrameworkCore;

namespace C_C.App.Repositories;

public class PerfilRepository : RepositoryBase<PerfilModel>, IPerfilRepository
{
    public PerfilRepository(AppDbContext context)
        : base(context)
    {
    }

    public Task<PerfilModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Context.Perfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public Task<PreferenciasModel?> GetPreferenciasByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return Context.Preferencias.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task SavePreferenciasAsync(PreferenciasModel preferencias, CancellationToken cancellationToken = default)
    {
        var existing = await Context.Preferencias.FirstOrDefaultAsync(p => p.UserId == preferencias.UserId, cancellationToken);
        if (existing is null)
        {
            await Context.Preferencias.AddAsync(preferencias, cancellationToken);
        }
        else
        {
            existing.EdadMinima = preferencias.EdadMinima;
            existing.EdadMaxima = preferencias.EdadMaxima;
            existing.GeneroBuscado = preferencias.GeneroBuscado;
            existing.DistanciaMaximaKm = preferencias.DistanciaMaximaKm;
            Context.Preferencias.Update(existing);
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}
