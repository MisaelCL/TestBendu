using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.ViewModel;


namespace C_C_Final.View
{
    public partial class PerfilView : Window
    {
        private readonly int _cuentaId;

        public PerfilView() : this(0)
        {
        }

        public PerfilView(int cuentaId)
        {
            _cuentaId = cuentaId;
            InitializeComponent();
            var app = App.Current;
            if (app != null)
            {
                var viewModel = new PerfilViewModel(app.PerfilRepository, app.MatchRepository);
                DataContext = viewModel;
                Loaded += (_, _) => CargarVista(viewModel, _cuentaId);
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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private static void CargarVista(PerfilViewModel viewModel, int cuentaId)
        {
            try
            {
                viewModel.Cargar(cuentaId);
            }
            catch (Exception)
            {
                // Ignora errores iniciales para permitir que la vista cargue.
            }
        }
    }
}
