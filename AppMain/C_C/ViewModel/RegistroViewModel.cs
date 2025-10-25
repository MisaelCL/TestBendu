using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using C_C.Model;
using C_C.Services;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class RegistroViewModel : BaseViewModel
{
    private readonly ICuentaService _cuentaService;
    private readonly ILogger<RegistroViewModel> _logger;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isBusy;
    private string? _error;

    public RegistroViewModel(ICuentaService cuentaService, ILogger<RegistroViewModel> logger)
    {
        _cuentaService = cuentaService;
        _logger = logger;
        RegistrarCommand = new RelayCommand(async _ => await RegistrarAsync(), _ => !IsBusy);
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

    private async Task RegistrarAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            if (Password != ConfirmPassword)
            {
                Error = "Las contraseñas no coinciden";
                return;
            }

            var cuenta = new Cuenta
            {
                Email = Email,
                Estado_Cuenta = 1
            };

            await _cuentaService.RegistrarAsync(cuenta, Password, CancellationToken.None).ConfigureAwait(false);
            _logger.LogInformation("Cuenta {Email} registrada", Email);
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            MessageBox.Show("Registro exitoso", "C_C", MessageBoxButton.OK, MessageBoxImage.Information);
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
}
