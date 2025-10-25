using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using C_C.Model;
using C_C.Services;
using C_C.ViewModel.Commands;
using Microsoft.Extensions.Logging;

namespace C_C.ViewModel;

public sealed class ChatViewModel : BaseViewModel
{
    private readonly IMensajeService _mensajeService;
    private readonly ILogger<ChatViewModel> _logger;
    private int _chatId;
    private int _perfilActual;
    private string _mensaje = string.Empty;
    private bool _isBusy;

    public ChatViewModel(IMensajeService mensajeService, ILogger<ChatViewModel> logger)
    {
        _mensajeService = mensajeService;
        _logger = logger;
        Mensajes = new ObservableCollection<Mensaje>();
        CargarMensajesCommand = new RelayCommand(async _ => await CargarMensajesAsync(), _ => !IsBusy);
        EnviarMensajeCommand = new RelayCommand(async _ => await EnviarMensajeAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(Mensaje));
    }

    public ObservableCollection<Mensaje> Mensajes { get; }

    public int ChatId
    {
        get => _chatId;
        set => SetProperty(ref _chatId, value);
    }

    public int PerfilActual
    {
        get => _perfilActual;
        set => SetProperty(ref _perfilActual, value);
    }

    public string Mensaje
    {
        get => _mensaje;
        set
        {
            if (SetProperty(ref _mensaje, value))
            {
                (EnviarMensajeCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                (CargarMensajesCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (EnviarMensajeCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand CargarMensajesCommand { get; }
    public ICommand EnviarMensajeCommand { get; }

    private async Task CargarMensajesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Mensajes.Clear();
            var mensajes = await _mensajeService.ObtenerUltimosAsync(ChatId, 50, CancellationToken.None).ConfigureAwait(false);
            foreach (var mensaje in mensajes)
            {
                Mensajes.Add(mensaje);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar los mensajes");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EnviarMensajeAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Mensaje))
        {
            return;
        }

        try
        {
            IsBusy = true;
            var nuevo = new Mensaje
            {
                ID_Chat = ChatId,
                Remitente = PerfilActual,
                Contenido = Mensaje
            };

            await _mensajeService.EnviarAsync(nuevo, CancellationToken.None).ConfigureAwait(false);
            Mensaje = string.Empty;
            await CargarMensajesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar mensaje");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
