using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var app = App.Current;
                if (app != null)
                {
                    DataContext ??= new LoginViewModel(app.CuentaRepository, app.PerfilRepository);
                }
            }

            // Arrastre de ventana desde cualquier zona vacía
            this.MouseLeftButtonDown += (_, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    DragMove();
            };
        }

        private void btnMinimizar_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        private void btnClose_Click(object sender, RoutedEventArgs e)
            => Close();

        // Sincroniza PasswordBox -> VM.Password
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            var pb = sender as PasswordBox;
            if (vm == null || pb == null) return;

            if (!string.Equals(vm.Password, pb.Password))
                vm.Password = pb.Password;
        }
    }
}
