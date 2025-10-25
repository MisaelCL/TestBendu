using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using C_C.App.Model;
using C_C.App.Services;

namespace C_C.App.ViewModel;

public class ChatViewModel : ViewModelBase
{
    private readonly ChatService _chatService;
    private readonly UserSession _userSession;
    private Guid? _chatId;
    private string _messageText = string.Empty;
    private string? _errorMessage;

    public ChatViewModel(ChatService chatService, UserSession userSession)
    {
        _chatService = chatService;
        _userSession = userSession;
        Mensajes = new ObservableCollection<MensajeModel>();
        EnviarMensajeCommand = new ViewModelCommand(async _ => await EnviarMensajeAsync(), _ => _chatId.HasValue);
    }

    public ObservableCollection<MensajeModel> Mensajes { get; }

    public string MessageText
    {
        get => _messageText;
        set => SetProperty(ref _messageText, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public ViewModelCommand EnviarMensajeCommand { get; }

    public async Task LoadChatAsync(Guid chatId)
    {
        _chatId = chatId;
        EnviarMensajeCommand.RaiseCanExecuteChanged();
        var messages = await _chatService.GetConversationAsync(chatId);
        Mensajes.Clear();
        foreach (var message in messages)
        {
            Mensajes.Add(message);
        }

        if (_userSession.CurrentUser is not null)
        {
            await _chatService.MarkAsReadAsync(chatId, _userSession.CurrentUser.Id);
        }
    }

    private async Task EnviarMensajeAsync()
    {
        if (_chatId is null || _userSession.CurrentUser is null)
        {
            ErrorMessage = "Selecciona un chat";
            return;
        }

        try
        {
            ErrorMessage = null;
            var sent = await _chatService.SendMessageAsync(_chatId.Value, _userSession.CurrentUser.Id, MessageText);
            Mensajes.Add(sent);
            MessageText = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
