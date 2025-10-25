using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using C_C.Application.Repositories;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class PerfilViewModel : BaseViewModel
{
    private readonly IPerfilRepository _perfilRepository;
    private readonly ILogger<PerfilViewModel> _logger;

    private int? _perfilId;
    private int? _cuentaId;
    private string _nikname = string.Empty;
    private string? _biografia;
    private string? _fotoPerfilBase64;
    private DateTime _fechaCreacion;
    private bool _isBusy;
    private string? _error;

    public PerfilViewModel(IPerfilRepository perfilRepository, ILogger<PerfilViewModel> logger)
    {
        _perfilRepository = perfilRepository;
        _logger = logger;
        LoadCommand = new RelayCommand(async (_, ct) => await LoadAsync(ct).ConfigureAwait(false), _ => !IsBusy);
        SaveCommand = new RelayCommand(async (_, ct) => await SaveAsync(ct).ConfigureAwait(false), _ => !IsBusy);
    }

    public int? PerfilId
    {
        get => _perfilId;
        set => SetProperty(ref _perfilId, value);
    }

    public int? CuentaId
    {
        get => _cuentaId;
        set => SetProperty(ref _cuentaId, value);
    }

    public string Nikname
    {
        get => _nikname;
        set => SetProperty(ref _nikname, value);
    }

    public string? Biografia
    {
        get => _biografia;
        set => SetProperty(ref _biografia, value);
    }

    public string? FotoPerfilBase64
    {
        get => _fotoPerfilBase64;
        set => SetProperty(ref _fotoPerfilBase64, value);
    }

    public DateTime FechaCreacion
    {
        get => _fechaCreacion;
        private set => SetProperty(ref _fechaCreacion, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (LoadCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand SaveCommand { get; }

    private async Task LoadAsync(CancellationToken ct)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            var perfil = await ObtenerPerfilAsync(ct).ConfigureAwait(false);
            if (perfil is null)
            {
                Error = "No se encontró el perfil solicitado";
                return;
            }

            PerfilId = perfil.ID_Perfil;
            CuentaId = perfil.ID_Cuenta;
            Nikname = perfil.Nikname;
            Biografia = perfil.Biografia;
            FotoPerfilBase64 = perfil.Foto_Perfil is null ? null : Convert.ToBase64String(perfil.Foto_Perfil);
            FechaCreacion = perfil.Fecha_Creacion;
        }
        catch (OperationCanceledException)
        {
            Error = "Carga cancelada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar el perfil");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        if (IsBusy)
        {
            return;
        }

        if (PerfilId is null)
        {
            Error = "Debe cargar un perfil antes de guardar";
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            var foto = string.IsNullOrWhiteSpace(FotoPerfilBase64) ? null : Convert.FromBase64String(FotoPerfilBase64);
            var perfil = new Domain.Perfil
            {
                ID_Perfil = PerfilId.Value,
                ID_Cuenta = CuentaId ?? 0,
                Nikname = Nikname,
                Biografia = Biografia,
                Foto_Perfil = foto,
                Fecha_Creacion = FechaCreacion
            };

            var updated = await _perfilRepository.UpdatePerfilAsync(perfil, ct).ConfigureAwait(false);
            if (!updated)
            {
                Error = "No fue posible actualizar el perfil";
                return;
            }

            _logger.LogInformation("Perfil {PerfilId} actualizado", PerfilId);
        }
        catch (FormatException)
        {
            Error = "La foto de perfil debe estar en formato Base64";
        }
        catch (OperationCanceledException)
        {
            Error = "Actualización cancelada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar el perfil");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<Domain.Perfil?> ObtenerPerfilAsync(CancellationToken ct)
    {
        if (PerfilId.HasValue)
        {
            return await _perfilRepository.GetByIdAsync(PerfilId.Value, ct).ConfigureAwait(false);
        }

        if (CuentaId.HasValue)
        {
            return await _perfilRepository.GetByCuentaIdAsync(CuentaId.Value, ct).ConfigureAwait(false);
        }

        return null;
    }
}
