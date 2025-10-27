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
                var viewModel = new PerfilViewModel(app.PerfilRepository);
                DataContext = viewModel;
                Loaded += (_, _) => Load(viewModel, _cuentaId);
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private static void Load(PerfilViewModel viewModel, int cuentaId)
        {
            try
            {
                viewModel.Load(cuentaId);
            }
            catch (Exception)
            {
                // Ignora errores iniciales para permitir que la vista cargue.
            }
        }
    }
}
