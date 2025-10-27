using System;
using System.Windows;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class ConfiguracionView : Window
    {
        public ConfiguracionView()
        {
            InitializeComponent();
            var app = App.Current;
            if (app != null)
            {
                var viewModel = new PreferenciasViewModel(app.PerfilRepository);
                DataContext = viewModel;
                Loaded += (_, _) => Load(viewModel);
            }
        }

        private static void Load(PreferenciasViewModel viewModel)
        {
            try
            {
                viewModel.Load(0);
            }
            catch (Exception)
            {
                // Ignora errores para permitir que la vista cargue sin datos.
            }
        }
    }
}
