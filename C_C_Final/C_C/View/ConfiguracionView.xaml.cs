using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class ConfiguracionView : Window
    {
        private readonly int _cuentaId;

        public ConfiguracionView() : this(0)
        {
        }

        public ConfiguracionView(int cuentaId)
        {
            _cuentaId = cuentaId;
            InitializeComponent();
            var app = App.Current;
            if (app != null)
            {
                var viewModel = new PreferenciasViewModel(app.PerfilRepository, app.PreferenciasRepository, app.CuentaDeletionService);
                DataContext = viewModel;
                Loaded += (_, _) => CargarVista(viewModel, _cuentaId);
            }
        }

        private static void CargarVista(PreferenciasViewModel viewModel, int cuentaId)
        {
            try
            {
                viewModel.Cargar(cuentaId);
            }
            catch (Exception)
            {
                // Ignora errores para permitir que la vista cargue sin datos.
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cuentaId != 0)
            {
                var perfilView = new PerfilView(_cuentaId);
                perfilView.Show();
            }

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
    }
}
