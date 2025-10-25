using System.Windows;
using C_C.ViewModel;

namespace C_C.View;

public partial class HomeView : Window
{
    public HomeView(HomeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
