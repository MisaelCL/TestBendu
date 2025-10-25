using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using C_C.Application.Repositories;
using C_C.Domain;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class PreferenciasViewModel : BaseViewModel
{
    private readonly IPerfilRepository _perfilRepository;
    private readonly ILogger<PreferenciasViewModel> _logger;

    private int _perfilId;
    private byte _genero;
    private int _edadMinima = 18;
    private int _edadMaxima = 35;
    private string _preferenciaCarrera = string.Empty;
    private string? _intereses;
    private bool _isBusy;
    private string? _error;

    public PreferenciasViewModel(IPerfilRepository perfilRepository, ILogger<PreferenciasViewModel> logger)
    {
        _perfilRepository = perfilRepository;
        _logger = logger;
        LoadCommand = new RelayCommand(async (_, ct) => await LoadAsync(ct).ConfigureAwait(false), _ => !IsBusy);
        SaveCommand = new RelayCommand(async (_, ct) => await SaveAsync(ct).ConfigureAwait(false), _ => !IsBusy);
    }

    public int PerfilId
    {
        get => _perfilId;
        set => SetProperty(ref _perfilId, value);
    }

    public byte PreferenciaGenero
    {
        get => _genero;
        set => SetProperty(ref _genero, value);
    }

    public int EdadMinima
    {
        get => _edadMinima;
        set => SetProperty(ref _edadMinima, value);
    }

    public int EdadMaxima
    {
        get => _edadMaxima;
        set => SetProperty(ref _edadMaxima, value);
    }

    public string PreferenciaCarrera
    {
        get => _preferenciaCarrera;
        set => SetProperty(ref _preferenciaCarrera, value);
    }

    public string? Intereses
    {
        get => _intereses;
        set => SetProperty(ref _intereses, value);
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
            var preferencias = await _perfilRepository.GetPreferenciasByPerfilAsync(PerfilId, ct).ConfigureAwait(false);
            if (preferencias is null)
            {
                PreferenciaGenero = 0;
                EdadMinima = 18;
                EdadMaxima = 35;
                PreferenciaCarrera = string.Empty;
                Intereses = null;
                return;
            }

            PreferenciaGenero = preferencias.Preferencia_Genero;
            EdadMinima = preferencias.Edad_Minima;
            EdadMaxima = preferencias.Edad_Maxima;
            PreferenciaCarrera = preferencias.Preferencia_Carrera;
            Intereses = preferencias.Intereses;
        }
        catch (OperationCanceledException)
        {
            Error = "Carga cancelada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar las preferencias");
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

        if (EdadMinima < 18 || EdadMaxima > 99 || EdadMinima > EdadMaxima)
        {
            Error = "Rango de edades inválido";
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            var preferencias = new Preferencias
            {
                ID_Perfil = PerfilId,
                Preferencia_Genero = PreferenciaGenero,
                Edad_Minima = EdadMinima,
                Edad_Maxima = EdadMaxima,
                Preferencia_Carrera = PreferenciaCarrera,
                Intereses = Intereses
            };

            await _perfilRepository.UpsertPreferenciasAsync(preferencias, ct).ConfigureAwait(false);
            _logger.LogInformation("Preferencias del perfil {PerfilId} guardadas", PerfilId);
        }
        catch (OperationCanceledException)
        {
            Error = "Guardado cancelado";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar las preferencias");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
