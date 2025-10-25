using System;
using System.Threading.Tasks;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly UserSession _userSession;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string? _errorMessage;

    public LoginViewModel(AuthService authService, UserSession userSession)
    {
        _authService = authService;
        _userSession = userSession;

        LoginCommand = new ViewModelCommand(async _ => await LoginAsync());
        RegistrarCommand = new ViewModelCommand(async _ => await RegisterAsync());
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

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ViewModelCommand LoginCommand { get; }

    public ViewModelCommand RegistrarCommand { get; }

    private async Task LoginAsync()
    {
        try
        {
            ErrorMessage = null;
            var user = await _authService.LoginAsync(Email, Password);
            _userSession.SetCurrentUser(user);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private Task RegisterAsync()
    {
        // La navegación se realiza desde la ventana principal; aquí solo limpiamos mensajes.
        ErrorMessage = null;
        return Task.CompletedTask;
    }
}
