using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class PreferenciasView : UserControl
    {
        public PreferenciasView()
        {
            InitializeComponent();
            DataContext = new PreferenciasViewModel();
        }
    }
}
