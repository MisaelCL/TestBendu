using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using C_C.Application.Repositories;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class InboxViewModel : BaseViewModel
{
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<InboxViewModel> _logger;
    private readonly ObservableCollection<InboxItemViewModel> _items = new();

    private int _perfilId;
    private int _pageSize = 20;
    private int _currentPage;
    private bool _hasMore = true;
    private bool _isBusy;
    private string? _error;

    public InboxViewModel(IMatchRepository matchRepository, ILogger<InboxViewModel> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
        Inbox = new ReadOnlyObservableCollection<InboxItemViewModel>(_items);
        RefreshCommand = new RelayCommand(async (_, ct) => await ReloadAsync(ct).ConfigureAwait(false), _ => !IsBusy);
        LoadMoreCommand = new RelayCommand(async (_, ct) => await LoadNextPageAsync(ct).ConfigureAwait(false), _ => !IsBusy && HasMore);
    }

    public ReadOnlyObservableCollection<InboxItemViewModel> Inbox { get; }

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

    public bool HasMore
    {
        get => _hasMore;
        private set
        {
            if (SetProperty(ref _hasMore, value))
            {
                (LoadMoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (RefreshCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (LoadMoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand LoadMoreCommand { get; }

    private async Task ReloadAsync(CancellationToken ct)
    {
        _items.Clear();
        _currentPage = 0;
        HasMore = true;
        await LoadPageAsync(_currentPage, ct).ConfigureAwait(false);
    }

    private async Task LoadNextPageAsync(CancellationToken ct)
    {
        if (!HasMore)
        {
            return;
        }

        _currentPage++;
        await LoadPageAsync(_currentPage, ct).ConfigureAwait(false);
    }

    private async Task LoadPageAsync(int page, CancellationToken ct)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            var matches = await _matchRepository.ListByPerfilAsync(PerfilId, page, PageSize, ct).ConfigureAwait(false);
            if (matches.Count < PageSize)
            {
                HasMore = false;
            }

            foreach (var match in matches)
            {
                var chat = await _matchRepository.GetChatByMatchIdAsync(match.ID_Match, ct).ConfigureAwait(false);
                if (chat is null)
                {
                    continue;
                }

                var mensajes = await _matchRepository.ListMensajesAsync(chat.ID_Chat, 0, 1, ct).ConfigureAwait(false);
                var ultimo = mensajes.Count > 0 ? mensajes[0] : null;
                var item = new InboxItemViewModel
                {
                    MatchId = match.ID_Match,
                    ChatId = chat.ID_Chat,
                    OtroPerfilId = match.Perfil_Emisor == PerfilId ? match.Perfil_Receptor : match.Perfil_Emisor,
                    UltimoMensaje = ultimo?.Contenido ?? "Sin mensajes",
                    FechaUltimoMensaje = ultimo?.Fecha_Envio ?? chat.LastMessageAtUtc
                };
                _items.Add(item);
            }
        }
        catch (OperationCanceledException)
        {
            Error = "Carga cancelada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la bandeja de entrada");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class InboxItemViewModel : BaseViewModel
{
    private int _matchId;
    private int _chatId;
    private int _otroPerfilId;
    private string _ultimoMensaje = string.Empty;
    private DateTime? _fechaUltimoMensaje;

    public int MatchId
    {
        get => _matchId;
        set => SetProperty(ref _matchId, value);
    }

    public int ChatId
    {
        get => _chatId;
        set => SetProperty(ref _chatId, value);
    }

    public int OtroPerfilId
    {
        get => _otroPerfilId;
        set => SetProperty(ref _otroPerfilId, value);
    }

    public string UltimoMensaje
    {
        get => _ultimoMensaje;
        set => SetProperty(ref _ultimoMensaje, value);
    }

    public DateTime? FechaUltimoMensaje
    {
        get => _fechaUltimoMensaje;
        set => SetProperty(ref _fechaUltimoMensaje, value);
    }
}
