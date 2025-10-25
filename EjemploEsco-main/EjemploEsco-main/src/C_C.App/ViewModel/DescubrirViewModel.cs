using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class DescubrirViewModel : ViewModelBase
{
    private readonly DiscoveryService _discoveryService;
    private readonly MatchService _matchService;
    private readonly UserSession _userSession;
    private UserModel? _currentCandidate;
    private string? _errorMessage;

    public DescubrirViewModel(DiscoveryService discoveryService, MatchService matchService, UserSession userSession)
    {
        _discoveryService = discoveryService;
        _matchService = matchService;
        _userSession = userSession;
        Candidates = new ObservableCollection<UserModel>();

        LikeCommand = new ViewModelCommand(async _ => await LikeAsync());
        SkipCommand = new ViewModelCommand(async _ => await SkipAsync());
        _userSession.CurrentUserChanged += async (_, _) => await LoadCandidatesAsync();
        _ = LoadCandidatesAsync();
    }

    public ObservableCollection<UserModel> Candidates { get; }

    public UserModel? CurrentCandidate
    {
        get => _currentCandidate;
        private set => SetProperty(ref _currentCandidate, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ViewModelCommand LikeCommand { get; }

    public ViewModelCommand SkipCommand { get; }

    private async Task LikeAsync()
    {
        if (_userSession.CurrentUser is null || CurrentCandidate is null)
        {
            ErrorMessage = "Inicia sesión para continuar";
            return;
        }

        try
        {
            await _matchService.RegisterLikeAsync(_userSession.CurrentUser.Id, CurrentCandidate.Id, liked: true);
            await LoadCandidatesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task SkipAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            ErrorMessage = "Inicia sesión para continuar";
            return;
        }

        try
        {
            if (CurrentCandidate is not null)
            {
                await _matchService.RegisterLikeAsync(_userSession.CurrentUser.Id, CurrentCandidate.Id, liked: false);
            }

            await LoadCandidatesAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task LoadCandidatesAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            Candidates.Clear();
            CurrentCandidate = null;
            return;
        }

        var candidates = await _discoveryService.GetCandidatesAsync(_userSession.CurrentUser.Id);
        Candidates.Clear();
        foreach (var candidate in candidates)
        {
            Candidates.Add(candidate);
        }

        CurrentCandidate = Candidates.FirstOrDefault();
        ErrorMessage = Candidates.Count == 0 ? "No encontramos coincidencias por ahora" : null;
    }
}
