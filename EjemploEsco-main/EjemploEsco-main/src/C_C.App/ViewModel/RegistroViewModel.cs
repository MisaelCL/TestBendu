using System;
using System.Threading.Tasks;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class RegistroViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly UserSession _userSession;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _displayName = string.Empty;
    private DateTime _dateOfBirth = DateTime.UtcNow.AddYears(-18);
    private string _bio = string.Empty;
    private string _city = string.Empty;
    private string _photoUrl = string.Empty;
    private string _interests = string.Empty;
    private int _minAge = 18;
    private int _maxAge = 80;
    private string _targetGender = "Todos";
    private double _maxDistanceKm = 50;
    private string? _errorMessage;
    private string? _successMessage;

    public RegistroViewModel(AuthService authService, UserSession userSession)
    {
        _authService = authService;
        _userSession = userSession;
        RegistrarCommand = new ViewModelCommand(async _ => await RegistrarAsync());
    }

    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
    public DateTime DateOfBirth { get => _dateOfBirth; set => SetProperty(ref _dateOfBirth, value); }
    public string Bio { get => _bio; set => SetProperty(ref _bio, value); }
    public string City { get => _city; set => SetProperty(ref _city, value); }
    public string PhotoUrl { get => _photoUrl; set => SetProperty(ref _photoUrl, value); }
    public string Interests { get => _interests; set => SetProperty(ref _interests, value); }
    public int MinAge { get => _minAge; set => SetProperty(ref _minAge, value); }
    public int MaxAge { get => _maxAge; set => SetProperty(ref _maxAge, value); }
    public string TargetGender { get => _targetGender; set => SetProperty(ref _targetGender, value); }
    public double MaxDistanceKm { get => _maxDistanceKm; set => SetProperty(ref _maxDistanceKm, value); }
    public string? ErrorMessage { get => _errorMessage; private set => SetProperty(ref _errorMessage, value); }
    public string? SuccessMessage { get => _successMessage; private set => SetProperty(ref _successMessage, value); }

    public ViewModelCommand RegistrarCommand { get; }

    private async Task RegistrarAsync()
    {
        try
        {
            ErrorMessage = null;
            SuccessMessage = null;
            var request = new UserRegistrationRequest(Email, Password, DisplayName, DateOnly.FromDateTime(DateOfBirth), Bio, City, PhotoUrl, Interests, MinAge, MaxAge, TargetGender, MaxDistanceKm);
            var user = await _authService.RegisterAsync(request);
            _userSession.SetCurrentUser(user);
            SuccessMessage = "Cuenta creada correctamente";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
