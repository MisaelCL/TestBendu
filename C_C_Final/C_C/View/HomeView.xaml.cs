using C_C_Final.ViewModel;
using System.Windows;

namespace C_C_Final.View
{
    /// <summary>
    /// Lógica de interacción para HomeView.xaml
    /// </summary>
    public partial class HomeView : Window
    {
        public HomeView(int idPerfilLogueado)
        {
            InitializeComponent();

            // 1. Crea una instancia del nuevo ViewModel
            var viewModel = new HomeViewModel(idPerfilLogueado);
            
            // 2. Asigna el ViewModel al DataContext
            DataContext = viewModel;
        }

        // --- MÉTODOS RESTAURADOS PARA CORREGIR ERRORES ---

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // Cierra la aplicación (o usa this.Close() si prefieres)
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            // Minimiza la ventana
            this.WindowState = WindowState.Minimized;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // 3. Conecta el evento Click del botón a un método en el ViewModel
            // Esto actúa como un "puente" entre el code-behind y el MVVM.
            if (DataContext is HomeViewModel viewModel)
            {
                viewModel.NavegarAConfiguracion();
            }
        }
    }
}
