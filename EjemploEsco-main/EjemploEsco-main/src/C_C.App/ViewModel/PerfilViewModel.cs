using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Repositories;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class PerfilViewModel : ViewModelBase
{
    private readonly IPerfilRepository _perfilRepository;
    private readonly UserSession _userSession;
    private readonly InputSanitizer _sanitizer;
    private string _bio = string.Empty;
    private string _city = string.Empty;
    private string _photoUrl = string.Empty;
    private string _intereses = string.Empty;
    private string? _mensaje;

    public PerfilViewModel(IPerfilRepository perfilRepository, UserSession userSession, InputSanitizer sanitizer)
    {
        _perfilRepository = perfilRepository;
        _userSession = userSession;
        _sanitizer = sanitizer;
        GuardarPerfilCommand = new ViewModelCommand(async _ => await GuardarPerfilAsync());
        _userSession.CurrentUserChanged += async (_, _) => await CargarPerfilAsync();
        _ = CargarPerfilAsync();
    }

    public string Bio { get => _bio; set => SetProperty(ref _bio, value); }
    public string Ciudad { get => _city; set => SetProperty(ref _city, value); }
    public string FotoUrl { get => _photoUrl; set => SetProperty(ref _photoUrl, value); }
    public string Intereses { get => _intereses; set => SetProperty(ref _intereses, value); }
    public string? Mensaje { get => _mensaje; private set => SetProperty(ref _mensaje, value); }

    public ViewModelCommand GuardarPerfilCommand { get; }

    private async Task GuardarPerfilAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            Mensaje = "Inicia sesión";
            return;
        }

        var existente = await _perfilRepository.GetByUserIdAsync(_userSession.CurrentUser.Id);
        if (existente is null)
        {
            var nuevo = new PerfilModel
            {
                UserId = _userSession.CurrentUser.Id,
                Bio = _sanitizer.Sanitize(Bio),
                Ciudad = _sanitizer.Sanitize(Ciudad),
                FotoUrl = FotoUrl.Trim(),
                Intereses = _sanitizer.Sanitize(Intereses)
            };

            await _perfilRepository.AddAsync(nuevo);
        }
        else
        {
            existente.Bio = _sanitizer.Sanitize(Bio);
            existente.Ciudad = _sanitizer.Sanitize(Ciudad);
            existente.FotoUrl = FotoUrl.Trim();
            existente.Intereses = _sanitizer.Sanitize(Intereses);
            await _perfilRepository.UpdateAsync(existente);
        }

        Mensaje = "Perfil actualizado";
    }

    private async Task CargarPerfilAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            Bio = string.Empty;
            Ciudad = string.Empty;
            FotoUrl = string.Empty;
            Intereses = string.Empty;
            Mensaje = null;
            return;
        }

        var perfil = await _perfilRepository.GetByUserIdAsync(_userSession.CurrentUser.Id);
        if (perfil is null)
        {
            Bio = string.Empty;
            Ciudad = string.Empty;
            FotoUrl = string.Empty;
            Intereses = string.Empty;
        }
        else
        {
            Bio = perfil.Bio;
            Ciudad = perfil.Ciudad;
            FotoUrl = perfil.FotoUrl;
            Intereses = perfil.Intereses;
        }
    }
}
