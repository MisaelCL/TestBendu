using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class ChatListView : UserControl
    {
        public ChatListView()
        {
            InitializeComponent();
            DataContext = new ChatListViewModel();
        }
    }
}
