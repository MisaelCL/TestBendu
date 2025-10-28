using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class HomeView : Window
    {
        private readonly int _perfilId;

        public HomeView(int perfilId)
        {
            _perfilId = perfilId;
            InitializeComponent();
            var app = App.Current;
            if (app != null)
            {
                var viewModel = new InboxViewModel(app.MatchRepository, app.PerfilRepository, app.MatchService);
                DataContext = viewModel;
                viewModel.MiPerfilRequested += OnMiPerfilRequested;
                Loaded += (_, _) => Load(viewModel, _perfilId);
                Closed += (_, _) => viewModel.MiPerfilRequested -= OnMiPerfilRequested;
            }
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

        private static void OnMiPerfilRequested(int cuentaId)
        {
            var perfilView = new PerfilView(cuentaId);
            perfilView.Show();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
