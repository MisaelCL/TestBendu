using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class DescubrirView : UserControl
    {
        public DescubrirView()
        {
            InitializeComponent();
            DataContext = new DescubrirViewModel();
        }
    }
}
