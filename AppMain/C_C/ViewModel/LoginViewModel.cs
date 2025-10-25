using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using C_C.Resources.utils;
using C_C.Services;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class LoginViewModel : BaseViewModel
{
    private readonly ICuentaService _cuentaService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LoginViewModel> _logger;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isBusy;
    private string? _error;

    public LoginViewModel(ICuentaService cuentaService, IPasswordHasher passwordHasher, ILogger<LoginViewModel> logger)
    {
        _cuentaService = cuentaService;
        _passwordHasher = passwordHasher;
        _logger = logger;
        LoginCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsBusy);
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

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (LoginCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public ICommand LoginCommand { get; }

    private async Task LoginAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            var cuenta = await _cuentaService.ObtenerPorEmailAsync(Email, CancellationToken.None).ConfigureAwait(false);
            if (cuenta is null || !_passwordHasher.Verify(Password, cuenta.PasswordHash))
            {
                Error = "Credenciales inválidas";
                return;
            }

            _logger.LogInformation("Usuario {Email} autenticado", Email);
            MessageBox.Show("Inicio de sesión exitoso", "C_C", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el inicio de sesión");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
