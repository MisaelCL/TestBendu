using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class PerfilView : UserControl
    {
        public PerfilView()
        {
            InitializeComponent();
            DataContext = new PerfilViewModel();
        }
    }
}
