using System.Windows;
using C_C.ViewModel;

namespace C_C.View;

public partial class ChatView : Window
{
    public ChatView(ChatViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
