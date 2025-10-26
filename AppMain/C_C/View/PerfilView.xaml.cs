using System;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Helpers;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class PerfilView : Window
    {
        public PerfilView()
        {
            InitializeComponent();
            var viewModel = AppBootstrapper.CreatePerfilViewModel();
            DataContext = viewModel;
            Loaded += (_, _) => Load(viewModel);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private static void Load(PerfilViewModel viewModel)
        {
            try
            {
                viewModel.Load(0);
            }
            catch (Exception)
            {
                // Ignora errores iniciales para permitir que la vista cargue.
            }
        }
    }
}
