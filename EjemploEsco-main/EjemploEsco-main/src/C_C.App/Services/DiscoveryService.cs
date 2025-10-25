using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using Microsoft.EntityFrameworkCore;

namespace C_C.App.Services;

public class DiscoveryService
{
    private readonly AppDbContext _context;

    public DiscoveryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserModel>> GetCandidatesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .Include(u => u.Preferencias)
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.Preferencias is null)
        {
            return Array.Empty<UserModel>();
        }

        var excludedIds = await _context.Matches
            .Where(m => m.UserIdA == userId || m.UserIdB == userId)
            .Select(m => m.UserIdA == userId ? m.UserIdB : m.UserIdA)
            .ToListAsync(cancellationToken);

        var targetGender = user.Preferencias.GeneroBuscado;

        return await _context.Users
            .Include(u => u.Perfil)
            .Where(u => u.Id != userId)
            .Where(u => !excludedIds.Contains(u.Id))
            .Where(u => u.IsActive && !u.IsBlocked)
            .Where(u => CalculateAge(u.DateOfBirth) >= user.Preferencias.EdadMinima && CalculateAge(u.DateOfBirth) <= user.Preferencias.EdadMaxima)
            .Where(u => targetGender == "Todos" || (u.Perfil != null && string.Equals(u.Perfil.Intereses, targetGender, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(u => u.CreatedAtUtc)
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    private static int CalculateAge(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;
        if (birthDate.DayOfYear > today.DayOfYear)
        {
            age--;
        }

        return age;
    }
}
