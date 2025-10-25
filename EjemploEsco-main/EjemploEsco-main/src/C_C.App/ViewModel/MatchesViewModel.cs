using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class MatchesViewModel : ViewModelBase
{
    private readonly MatchService _matchService;
    private readonly UserSession _userSession;
    private MatchModel? _selectedMatch;
    private string? _errorMessage;

    public MatchesViewModel(MatchService matchService, UserSession userSession)
    {
        _matchService = matchService;
        _userSession = userSession;
        Matches = new ObservableCollection<MatchModel>();
        AbrirChatCommand = new ViewModelCommand(async _ => await AbrirChatAsync(), _ => SelectedMatch is not null);
        _userSession.CurrentUserChanged += async (_, _) => await LoadMatchesAsync();
        _ = LoadMatchesAsync();
    }

    public ObservableCollection<MatchModel> Matches { get; }

    public MatchModel? SelectedMatch
    {
        get => _selectedMatch;
        set
        {
            if (SetProperty(ref _selectedMatch, value))
            {
                AbrirChatCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ViewModelCommand AbrirChatCommand { get; }

    public event EventHandler<Guid>? AbrirChatSolicitado;

    private async Task AbrirChatAsync()
    {
        if (SelectedMatch?.ChatId is null)
        {
            ErrorMessage = "Aún no existe chat para este match";
            return;
        }

        AbrirChatSolicitado?.Invoke(this, SelectedMatch.ChatId.Value);
        await Task.CompletedTask;
    }

    private async Task LoadMatchesAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            Matches.Clear();
            SelectedMatch = null;
            return;
        }

        var matches = await _matchService.GetMatchesAsync(_userSession.CurrentUser.Id);
        Matches.Clear();
        foreach (var match in matches)
        {
            Matches.Add(match);
        }

        ErrorMessage = Matches.Count == 0 ? "Aún no tienes matches" : null;
    }
}
