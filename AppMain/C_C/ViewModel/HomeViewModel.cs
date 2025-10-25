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

public sealed class HomeViewModel : BaseViewModel
{
    private readonly IChatService _chatService;
    private readonly ILogger<HomeViewModel> _logger;
    private bool _isBusy;

    public HomeViewModel(IChatService chatService, ILogger<HomeViewModel> logger)
    {
        _chatService = chatService;
        _logger = logger;
        Chats = new ObservableCollection<Chat>();
        LoadChatsCommand = new RelayCommand(async _ => await LoadChatsAsync(), _ => !IsBusy);
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

    public ICommand LoadChatsCommand { get; }

    private async Task LoadChatsAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Chats.Clear();
            var chats = await _chatService.ListarChatsPorPerfilAsync(0, 20, CancellationToken.None).ConfigureAwait(false);
            foreach (var chat in chats)
            {
                Chats.Add(chat);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar los chats");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
