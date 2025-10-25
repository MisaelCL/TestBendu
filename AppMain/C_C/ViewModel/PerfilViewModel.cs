using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using C_C.Model;
using C_C.Services;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class PerfilViewModel : BaseViewModel
{
    private readonly IPerfilService _perfilService;
    private readonly ILogger<PerfilViewModel> _logger;
    private Perfil? _perfil;
    private bool _isBusy;

    public PerfilViewModel(IPerfilService perfilService, ILogger<PerfilViewModel> logger)
    {
        _perfilService = perfilService;
        _logger = logger;
        GuardarCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !IsBusy && Perfil is not null);
    }

    public Perfil? Perfil
    {
        get => _perfil;
        private set
        {
            if (SetProperty(ref _perfil, value))
            {
                (GuardarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (GuardarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand GuardarCommand { get; }

    public async Task CargarPerfilAsync(int idPerfil, CancellationToken ct = default)
    {
        try
        {
            IsBusy = true;
            Perfil = await _perfilService.GetAsync(idPerfil, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el perfil");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GuardarAsync()
    {
        if (Perfil is null)
        {
            return;
        }

        try
        {
            IsBusy = true;
            await _perfilService.UpdateAsync(Perfil, CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el perfil");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
