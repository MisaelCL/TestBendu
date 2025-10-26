using System;
using System.Threading.Tasks;
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
            Loaded += async (_, _) => await LoadAsync(viewModel).ConfigureAwait(false);
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

        private static async Task LoadAsync(InboxViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync(0).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Ignora errores de carga inicial para permitir que la vista se muestre.
            }
        }
    }
}
