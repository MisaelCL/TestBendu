using System.Windows;
using System.Windows.Controls;

namespace C_C.App.Views;

public partial class RegistroView : UserControl
{
    public RegistroView()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModel.RegistroViewModel vm && sender is PasswordBox passwordBox)
        {
            vm.Password = passwordBox.Password;
        }
    }
}
