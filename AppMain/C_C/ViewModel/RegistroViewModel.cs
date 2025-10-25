using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using C_C.Application.Services;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class RegistroViewModel : BaseViewModel
{
    private readonly IRegisterAlumnoService _registerService;
    private readonly ILogger<RegistroViewModel> _logger;

    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private int _matricula;
    private string _nombre = string.Empty;
    private string _apaterno = string.Empty;
    private string _amaterno = string.Empty;
    private DateTime _fechaNacimiento = DateTime.Today.AddYears(-18);
    private string _genero = "M";
    private string _correoInstitucional = string.Empty;
    private string _carrera = string.Empty;
    private string? _nikname;
    private string? _biografia;
    private byte _preferenciaGenero;
    private int _edadMinima = 18;
    private int _edadMaxima = 35;
    private string _preferenciaCarrera = string.Empty;
    private string? _intereses;
    private bool _isBusy;
    private string? _error;

    public RegistroViewModel(IRegisterAlumnoService registerService, ILogger<RegistroViewModel> logger)
    {
        _registerService = registerService;
        _logger = logger;
        RegistrarCommand = new RelayCommand(async (_, ct) => await RegistrarAsync(ct).ConfigureAwait(false), _ => !IsBusy);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public int Matricula
    {
        get => _matricula;
        set => SetProperty(ref _matricula, value);
    }

    public string Nombre
    {
        get => _nombre;
        set => SetProperty(ref _nombre, value);
    }

    public string Apaterno
    {
        get => _apaterno;
        set => SetProperty(ref _apaterno, value);
    }

    public string Amaterno
    {
        get => _amaterno;
        set => SetProperty(ref _amaterno, value);
    }

    public DateTime FechaNacimiento
    {
        get => _fechaNacimiento;
        set => SetProperty(ref _fechaNacimiento, value);
    }

    public string Genero
    {
        get => _genero;
        set => SetProperty(ref _genero, value);
    }

    public string CorreoInstitucional
    {
        get => _correoInstitucional;
        set => SetProperty(ref _correoInstitucional, value);
    }

    public string Carrera
    {
        get => _carrera;
        set => SetProperty(ref _carrera, value);
    }

    public string? Nikname
    {
        get => _nikname;
        set => SetProperty(ref _nikname, value);
    }

    public string? Biografia
    {
        get => _biografia;
        set => SetProperty(ref _biografia, value);
    }

    public byte PreferenciaGenero
    {
        get => _preferenciaGenero;
        set => SetProperty(ref _preferenciaGenero, value);
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
                (RegistrarCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public ICommand RegistrarCommand { get; }

    private async Task RegistrarAsync(CancellationToken ct)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;

            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                Error = "Las contraseñas no coinciden";
                return;
            }

            var edad = CalcularEdad(FechaNacimiento);
            if (edad < 18)
            {
                Error = "Debes tener al menos 18 años";
                return;
            }

            if (EdadMinima < 18 || EdadMaxima > 99 || EdadMinima > EdadMaxima)
            {
                Error = "Rango de edad inválido";
                return;
            }

            var generoChar = string.IsNullOrWhiteSpace(Genero) ? 'M' : char.ToUpperInvariant(Genero[0]);
            if (generoChar != 'M' && generoChar != 'F')
            {
                Error = "Selecciona un género válido";
                return;
            }

            var request = new RegisterAlumnoRequest(
                Email,
                Password,
                Matricula,
                Nombre,
                Apaterno,
                Amaterno,
                FechaNacimiento,
                generoChar,
                CorreoInstitucional,
                Carrera,
                Nikname,
                Biografia,
                FotoPerfil: null,
                PreferenciaGenero,
                EdadMinima,
                EdadMaxima,
                string.IsNullOrWhiteSpace(PreferenciaCarrera) ? Carrera : PreferenciaCarrera,
                Intereses);

            await _registerService.RegisterAsync(request, ct).ConfigureAwait(false);

            _logger.LogInformation("Cuenta {Email} registrada correctamente", Email);
            MessageBox.Show("Registro exitoso", "C_C", MessageBoxButton.OK, MessageBoxImage.Information);
            LimpiarCampos();
        }
        catch (OperationCanceledException)
        {
            Error = "Operación cancelada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static int CalcularEdad(DateTime nacimiento)
    {
        var today = DateTime.Today;
        var edad = today.Year - nacimiento.Year;
        if (nacimiento.Date > today.AddYears(-edad))
        {
            edad--;
        }

        return edad;
    }

    private void LimpiarCampos()
    {
        Password = string.Empty;
        ConfirmPassword = string.Empty;
        Nikname = null;
        Biografia = null;
        Intereses = null;
    }
}
