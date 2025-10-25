using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using C_C.Application.Repositories;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class ChatViewModel : BaseViewModel
{
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<ChatViewModel> _logger;
    private readonly ObservableCollection<MensajeItemViewModel> _mensajes = new();

    private int? _matchId;
    private int? _chatId;
    private int _perfilActual;
    private int _pageSize = 20;
    private int _currentPage;
    private bool _hasMore = true;
    private string _nuevoMensaje = string.Empty;
    private bool _isBusy;
    private string? _error;

    public ChatViewModel(IMatchRepository matchRepository, ILogger<ChatViewModel> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
        Mensajes = new ReadOnlyObservableCollection<MensajeItemViewModel>(_mensajes);
        LoadCommand = new RelayCommand(async (_, ct) => await ReloadAsync(ct).ConfigureAwait(false), _ => !IsBusy);
        LoadMoreCommand = new RelayCommand(async (_, ct) => await LoadMoreAsync(ct).ConfigureAwait(false), _ => !IsBusy && HasMore);
        SendCommand = new RelayCommand(async (_, ct) => await SendAsync(ct).ConfigureAwait(false), _ => !IsBusy && !string.IsNullOrWhiteSpace(NuevoMensaje));
    }

    public ReadOnlyObservableCollection<MensajeItemViewModel> Mensajes { get; }

    public int? MatchId
    {
        get => _matchId;
        set => SetProperty(ref _matchId, value);
    }

    public int? ChatId
    {
        get => _chatId;
        set => SetProperty(ref _chatId, value);
    }

    public int PerfilActual
    {
        get => _perfilActual;
        set => SetProperty(ref _perfilActual, value);
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

    public string NuevoMensaje
    {
        get => _nuevoMensaje;
        set
        {
            if (SetProperty(ref _nuevoMensaje, value))
            {
                (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                (LoadCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (LoadMoreCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (SendCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand SendCommand { get; }

    private async Task ReloadAsync(CancellationToken ct)
    {
        _mensajes.Clear();
        _currentPage = 0;
        HasMore = true;
        await EnsureChatAsync(ct).ConfigureAwait(false);
        await LoadPageAsync(_currentPage, ct).ConfigureAwait(false);
    }

    private async Task LoadMoreAsync(CancellationToken ct)
    {
        if (!HasMore)
        {
            return;
        }

        _currentPage++;
        await LoadPageAsync(_currentPage, ct).ConfigureAwait(false);
    }

    private async Task EnsureChatAsync(CancellationToken ct)
    {
        if (ChatId.HasValue)
        {
            return;
        }

        if (!MatchId.HasValue)
        {
            throw new InvalidOperationException("Se requiere un ID de match o de chat para cargar la conversación.");
        }

        var chat = await _matchRepository.GetChatByMatchIdAsync(MatchId.Value, ct).ConfigureAwait(false);
        if (chat is null)
        {
            var chatId = await _matchRepository.EnsureChatForMatchAsync(MatchId.Value, ct).ConfigureAwait(false);
            ChatId = chatId;
        }
        else
        {
            ChatId = chat.ID_Chat;
        }
    }

    private async Task LoadPageAsync(int page, CancellationToken ct)
    {
        if (IsBusy)
        {
            return;
        }

        if (!ChatId.HasValue)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            var mensajes = await _matchRepository.ListMensajesAsync(ChatId.Value, page, PageSize, ct).ConfigureAwait(false);
            if (mensajes.Count < PageSize)
            {
                HasMore = false;
            }

            foreach (var mensaje in mensajes)
            {
                _mensajes.Add(new MensajeItemViewModel
                {
                    Id = mensaje.ID_Mensaje,
                    ChatId = mensaje.ID_Chat,
                    Remitente = mensaje.Remitente,
                    Contenido = mensaje.Contenido,
                    FechaEnvio = mensaje.Fecha_Envio,
                    EstaLeido = mensaje.Confirmacion_Lectura,
                    EstaEditado = mensaje.IsEdited,
                    FechaEdicion = mensaje.EditedAtUtc,
                    EstaEliminado = mensaje.IsDeleted
                });
            }
        }
        catch (OperationCanceledException)
        {
            Error = "Carga cancelada";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar los mensajes");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SendAsync(CancellationToken ct)
    {
        if (IsBusy || string.IsNullOrWhiteSpace(NuevoMensaje))
        {
            return;
        }

        await EnsureChatAsync(ct).ConfigureAwait(false);
        if (!ChatId.HasValue)
        {
            Error = "No fue posible resolver la conversación";
            return;
        }

        try
        {
            IsBusy = true;
            Error = null;
            var messageId = await _matchRepository.AddMensajeAsync(ChatId.Value, PerfilActual, NuevoMensaje, false, ct).ConfigureAwait(false);
            _mensajes.Insert(0, new MensajeItemViewModel
            {
                Id = messageId,
                ChatId = ChatId.Value,
                Remitente = PerfilActual,
                Contenido = NuevoMensaje,
                FechaEnvio = DateTime.UtcNow,
                EstaLeido = false,
                EstaEditado = false,
                FechaEdicion = null,
                EstaEliminado = false
            });
            NuevoMensaje = string.Empty;
        }
        catch (OperationCanceledException)
        {
            Error = "Envío cancelado";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar el mensaje");
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class MensajeItemViewModel : BaseViewModel
{
    private long _id;
    private int _chatId;
    private int _remitente;
    private string _contenido = string.Empty;
    private DateTime _fechaEnvio;
    private bool _estaLeido;
    private bool _estaEditado;
    private DateTime? _fechaEdicion;
    private bool _estaEliminado;

    public long Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int ChatId
    {
        get => _chatId;
        set => SetProperty(ref _chatId, value);
    }

    public int Remitente
    {
        get => _remitente;
        set => SetProperty(ref _remitente, value);
    }

    public string Contenido
    {
        get => _contenido;
        set => SetProperty(ref _contenido, value);
    }

    public DateTime FechaEnvio
    {
        get => _fechaEnvio;
        set => SetProperty(ref _fechaEnvio, value);
    }

    public bool EstaLeido
    {
        get => _estaLeido;
        set => SetProperty(ref _estaLeido, value);
    }

    public bool EstaEditado
    {
        get => _estaEditado;
        set => SetProperty(ref _estaEditado, value);
    }

    public DateTime? FechaEdicion
    {
        get => _fechaEdicion;
        set => SetProperty(ref _fechaEdicion, value);
    }

    public bool EstaEliminado
    {
        get => _estaEliminado;
        set => SetProperty(ref _estaEliminado, value);
    }
}
