using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using C_C.Application.Repositories;
using C_C.Domain;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class HomeViewModel : BaseViewModel
{
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<HomeViewModel> _logger;
    private bool _isBusy;
    private string? _error;
    private int _perfilId;
    private int _pageSize = 20;

    public HomeViewModel(IMatchRepository matchRepository, ILogger<HomeViewModel> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
        Chats = new ObservableCollection<Chat>();
        LoadChatsCommand = new RelayCommand(async (_, ct) => await LoadChatsAsync(ct).ConfigureAwait(false), _ => !IsBusy);
    }

    public ObservableCollection<Chat> Chats { get; }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (LoadChatsCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public int PerfilId
    {
        get => _perfilId;
        set => SetProperty(ref _perfilId, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => SetProperty(ref _pageSize, value);
    }

    public ICommand LoadChatsCommand { get; }

    private async Task LoadChatsAsync(CancellationToken ct)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            Chats.Clear();
            var matches = await _matchRepository.ListByPerfilAsync(PerfilId, 0, PageSize, ct).ConfigureAwait(false);
            foreach (var match in matches)
            {
                var chat = await _matchRepository.GetChatByMatchIdAsync(match.ID_Match, ct).ConfigureAwait(false);
                if (chat is not null)
                {
                    Chats.Add(chat);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Error = "Carga cancelada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar los chats");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
