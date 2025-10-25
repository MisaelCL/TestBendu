using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class ChatView : UserControl
    {
        public ChatView()
        {
            InitializeComponent();
            DataContext = new ChatViewModel();
        }
    }
}
