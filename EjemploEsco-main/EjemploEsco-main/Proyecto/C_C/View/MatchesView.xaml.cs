using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class MatchesView : UserControl
    {
        public MatchesView()
        {
            InitializeComponent();
            DataContext = new MatchesViewModel();
        }
    }
}
