using System;
using System.Threading.Tasks;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class AjustesViewModel : ViewModelBase
{
    private readonly ModerationService _moderationService;
    private readonly UserSession _userSession;
    private string _usuarioObjetivo = string.Empty;
    private string _motivo = string.Empty;
    private string? _mensaje;

    public AjustesViewModel(ModerationService moderationService, UserSession userSession)
    {
        _moderationService = moderationService;
        _userSession = userSession;
        BloquearCommand = new ViewModelCommand(async _ => await BloquearAsync());
        ReportarCommand = new ViewModelCommand(async _ => await ReportarAsync());
    }

    public string UsuarioObjetivo
    {
        get => _usuarioObjetivo;
        set => SetProperty(ref _usuarioObjetivo, value);
    }

    public string Motivo
    {
        get => _motivo;
        set => SetProperty(ref _motivo, value);
    }

    public string? Mensaje
    {
        get => _mensaje;
        private set => SetProperty(ref _mensaje, value);
    }

    public ViewModelCommand BloquearCommand { get; }

    public ViewModelCommand ReportarCommand { get; }

    private async Task BloquearAsync()
    {
        if (!Guid.TryParse(UsuarioObjetivo, out var userId))
        {
            Mensaje = "GUID inválido";
            return;
        }

        await _moderationService.BlockUserAsync(userId);
        Mensaje = "Usuario bloqueado";
    }

    private async Task ReportarAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            Mensaje = "Inicia sesión";
            return;
        }

        if (!Guid.TryParse(UsuarioObjetivo, out var userId))
        {
            Mensaje = "GUID inválido";
            return;
        }

        try
        {
            await _moderationService.ReportUserAsync(_userSession.CurrentUser.Id, userId, Motivo);
            Mensaje = "Reporte enviado";
        }
        catch (Exception ex)
        {
            Mensaje = ex.Message;
        }
    }
}
