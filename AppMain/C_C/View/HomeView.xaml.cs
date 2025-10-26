using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Helpers;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class HomeView : Window
    {
        private readonly int _perfilId;

        public HomeView() : this(0)
        {
        }

        public HomeView(int perfilId)
        {
            _perfilId = perfilId;
            InitializeComponent();
            var viewModel = AppBootstrapper.CreateInboxViewModel();
            DataContext = viewModel;
            Loaded += (_, _) => Load(viewModel, _perfilId);
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

        private static void Load(InboxViewModel viewModel, int perfilId)
        {
            try
            {
                viewModel.Load(perfilId);
            }
            catch (Exception)
            {
                // Ignora errores de carga inicial para permitir que la vista se muestre.
            }
        }
    }
}
