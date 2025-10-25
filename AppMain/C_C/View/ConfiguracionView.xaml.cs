using System.Windows;
using C_C.ViewModel;

namespace C_C.View;

public partial class ConfiguracionView : Window
{
    public ConfiguracionView(ConfiguracionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
