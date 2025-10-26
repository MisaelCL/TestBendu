using System;
using System.Threading.Tasks;
using System.Windows;
using C_C_Final.Presentation.Helpers;
using C_C_Final.Presentation.ViewModels;

namespace C_C_Final.View
{
    public partial class ConfiguracionView : Window
    {
        public ConfiguracionView()
        {
            InitializeComponent();
            var viewModel = AppBootstrapper.CreatePreferenciasViewModel();
            DataContext = viewModel;
            Loaded += async (_, _) => await LoadAsync(viewModel).ConfigureAwait(false);
        }

        private static async Task LoadAsync(PreferenciasViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync(0).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Ignora errores para permitir que la vista cargue sin datos.
            }
        }
    }
}
