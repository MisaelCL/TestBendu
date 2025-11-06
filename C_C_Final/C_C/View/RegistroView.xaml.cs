using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class RegistroView : Window
    {
        public RegistroView()
        {
            InitializeComponent();

            // Suscríbete al evento DataContextChanged (no existe override)
            DataContextChanged += RegistroView_DataContextChanged;
            var app = App.Current;
            if (app != null)
            {
                DataContext = new RegistroViewModel(app.RegisterAlumnoService);
            }
        }

        // Manejador del cambio de DataContext: attach/detach de PropertyChanged
        private void RegistroView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldVm = e.OldValue as RegistroViewModel;
            if (oldVm != null)
            {
                PropertyChangedEventManager.RemoveHandler(oldVm, RegistroViewModelOnPropertyChanged, string.Empty);
            }

            var newVm = e.NewValue as RegistroViewModel;
            if (newVm != null)
            {
                PropertyChangedEventManager.AddHandler(newVm, RegistroViewModelOnPropertyChanged, string.Empty);
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            // Limpieza final
            var vm = DataContext as RegistroViewModel;
            if (vm != null)
            {
                PropertyChangedEventManager.RemoveHandler(vm, RegistroViewModelOnPropertyChanged, string.Empty);
            }
            base.OnClosed(e);
        }

        private void RegistroViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as RegistroViewModel;
            if (vm == null) return;

            if (e.PropertyName == nameof(RegistroViewModel.Password))
            {
                if (string.IsNullOrEmpty(vm.Password) && (passwordBox?.Password?.Length > 0))
                {
                    passwordBox.Password = string.Empty;
                }
            }
            else if (e.PropertyName == nameof(RegistroViewModel.ConfirmPassword))
            {
                if (string.IsNullOrEmpty(vm.ConfirmPassword) && (confirmPasswordBox?.Password?.Length > 0))
                {
                    confirmPasswordBox.Password = string.Empty;
                }
            }
        }

        private void btnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as RegistroViewModel;
            var pb = sender as PasswordBox;
            if (vm != null && pb != null && vm.Password != pb.Password)
            {
                vm.Password = pb.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as RegistroViewModel;
            var pb = sender as PasswordBox;
            if (vm != null && pb != null && vm.ConfirmPassword != pb.Password)
            {
                vm.ConfirmPassword = pb.Password;
            }
        }
    }
}
