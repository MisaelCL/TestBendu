using System;
using System.Threading;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Repositories;

namespace C_C.App.Services;

public class ModerationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepositoryBase<ReporteModel> _reporteRepository;

    public ModerationService(IUserRepository userRepository, IRepositoryBase<ReporteModel> reporteRepository)
    {
        _userRepository = userRepository;
        _reporteRepository = reporteRepository;
    }

    public async Task<UserModel> BlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken) ?? throw new InvalidOperationException("Usuario no encontrado");
        user.IsBlocked = true;
        await _userRepository.UpdateAsync(user, cancellationToken);
        return user;
    }

    public async Task<ReporteModel> ReportUserAsync(Guid reporterId, Guid targetId, string motivo, CancellationToken cancellationToken = default)
    {
        if (reporterId == targetId)
        {
            throw new InvalidOperationException("No puedes reportarte a ti mismo");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new InvalidOperationException("Debes proporcionar un motivo");
        }

        var reporte = new ReporteModel
        {
            ReportanteId = reporterId,
            ReportadoId = targetId,
            Motivo = motivo.Trim()
        };

        await _reporteRepository.AddAsync(reporte, cancellationToken);
        return reporte;
    }
}
