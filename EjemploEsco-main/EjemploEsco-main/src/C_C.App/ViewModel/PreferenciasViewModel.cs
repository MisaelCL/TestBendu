using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Repositories;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class PreferenciasViewModel : ViewModelBase
{
    private readonly IPerfilRepository _perfilRepository;
    private readonly UserSession _userSession;
    private int _edadMinima = 18;
    private int _edadMaxima = 80;
    private string _genero = "Todos";
    private double _distancia = 50;
    private string? _mensaje;

    public PreferenciasViewModel(IPerfilRepository perfilRepository, UserSession userSession)
    {
        _perfilRepository = perfilRepository;
        _userSession = userSession;
        GuardarPreferenciasCommand = new ViewModelCommand(async _ => await GuardarAsync());
        _userSession.CurrentUserChanged += async (_, _) => await CargarAsync();
        _ = CargarAsync();
    }

    public int EdadMinima { get => _edadMinima; set => SetProperty(ref _edadMinima, value); }
    public int EdadMaxima { get => _edadMaxima; set => SetProperty(ref _edadMaxima, value); }
    public string Genero { get => _genero; set => SetProperty(ref _genero, value); }
    public double Distancia { get => _distancia; set => SetProperty(ref _distancia, value); }
    public string? Mensaje { get => _mensaje; private set => SetProperty(ref _mensaje, value); }

    public ViewModelCommand GuardarPreferenciasCommand { get; }

    private async Task GuardarAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            Mensaje = "Inicia sesión";
            return;
        }

        if (EdadMinima < 18 || EdadMaxima < EdadMinima)
        {
            Mensaje = "Rango de edad inválido";
            return;
        }

        var preferencias = new PreferenciasModel
        {
            UserId = _userSession.CurrentUser.Id,
            EdadMinima = EdadMinima,
            EdadMaxima = EdadMaxima,
            GeneroBuscado = Genero,
            DistanciaMaximaKm = Distancia
        };

        await _perfilRepository.SavePreferenciasAsync(preferencias);
        Mensaje = "Preferencias guardadas";
    }

    private async Task CargarAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            EdadMinima = 18;
            EdadMaxima = 80;
            Genero = "Todos";
            Distancia = 50;
            Mensaje = null;
            return;
        }

        var preferencias = await _perfilRepository.GetPreferenciasByUserIdAsync(_userSession.CurrentUser.Id);
        if (preferencias is null)
        {
            EdadMinima = 18;
            EdadMaxima = 80;
            Genero = "Todos";
            Distancia = 50;
        }
        else
        {
            EdadMinima = preferencias.EdadMinima;
            EdadMaxima = preferencias.EdadMaxima;
            Genero = preferencias.GeneroBuscado;
            Distancia = preferencias.DistanciaMaximaKm;
        }
    }
}
