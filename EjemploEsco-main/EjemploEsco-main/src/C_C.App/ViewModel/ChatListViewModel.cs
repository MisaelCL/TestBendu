using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Repositories;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class ChatListViewModel : ViewModelBase
{
    private readonly IChatRepository _chatRepository;
    private readonly UserSession _userSession;
    private ChatModel? _selectedChat;
    private string? _errorMessage;

    public ChatListViewModel(IChatRepository chatRepository, UserSession userSession)
    {
        _chatRepository = chatRepository;
        _userSession = userSession;
        Chats = new ObservableCollection<ChatModel>();
        AbrirChatCommand = new ViewModelCommand(async _ => await AbrirChatAsync(), _ => SelectedChat is not null);
        _userSession.CurrentUserChanged += async (_, _) => await LoadChatsAsync();
        _ = LoadChatsAsync();
    }

    public ObservableCollection<ChatModel> Chats { get; }

    public ChatModel? SelectedChat
    {
        get => _selectedChat;
        set
        {
            if (SetProperty(ref _selectedChat, value))
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
        if (SelectedChat is null)
        {
            return;
        }

        AbrirChatSolicitado?.Invoke(this, SelectedChat.Id);
        await Task.CompletedTask;
    }

    private async Task LoadChatsAsync()
    {
        if (_userSession.CurrentUser is null)
        {
            Chats.Clear();
            SelectedChat = null;
            return;
        }

        var chats = await _chatRepository.GetChatsForUserAsync(_userSession.CurrentUser.Id);
        Chats.Clear();
        foreach (var chat in chats)
        {
            Chats.Add(chat);
        }

        ErrorMessage = Chats.Count == 0 ? "Sin chats activos" : null;
    }
}
