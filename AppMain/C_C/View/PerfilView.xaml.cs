using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using C_C_Final.Presentation.Helpers;
using C_C_Final.Presentation.ViewModels;

namespace C_C_Final.View
{
    public partial class PerfilView : Window
    {
        public PerfilView()
        {
            InitializeComponent();
            var viewModel = AppBootstrapper.CreatePerfilViewModel();
            DataContext = viewModel;
            Loaded += async (_, _) => await LoadAsync(viewModel).ConfigureAwait(false);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private static async Task LoadAsync(PerfilViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync(0).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Ignora errores iniciales para permitir que la vista cargue.
            }
        }
    }
}
