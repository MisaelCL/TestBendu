using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class AjustesView : UserControl
    {
        public AjustesView()
        {
            InitializeComponent();
            DataContext = new AjustesViewModel();
        }
    }
}
