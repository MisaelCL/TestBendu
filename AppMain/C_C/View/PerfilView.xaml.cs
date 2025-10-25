using System.Windows;
using C_C.ViewModel;

namespace C_C.View;

public partial class PerfilView : Window
{
    public PerfilView(PerfilViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
