using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Helpers;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class HomeView : Window
    {
        public HomeView()
        {
            InitializeComponent();
            var viewModel = AppBootstrapper.CreateInboxViewModel();
            DataContext = viewModel;
            Loaded += (_, _) => Load(viewModel);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
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

        private static void Load(InboxViewModel viewModel)
        {
            try
            {
                viewModel.Load(0);
            }
            catch (Exception)
            {
                // Ignora errores de carga inicial para permitir que la vista se muestre.
            }
        }
    }
}
