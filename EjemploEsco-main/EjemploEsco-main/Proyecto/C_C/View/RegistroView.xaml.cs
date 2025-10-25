using System.Windows;
using System.Windows.Controls;
using C_C.ViewModel;

namespace C_C.View
{
    public partial class RegistroView : Window
    {
        public RegistroView()
        {
            InitializeComponent();
            DataContext = new RegistroViewModel();
            Application.Current.MainWindow = this;
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegistroViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.Usuario.PasswordHash = passwordBox.Password;
            }
        }

        private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegistroViewModel viewModel && sender is PasswordBox passwordBox)
            {
                viewModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
