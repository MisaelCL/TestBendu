using C_C_Final.ViewModel; // Importar el namespace del ViewModel
using System.Windows;

namespace C_C_Final.View
{
    /// <summary>
    /// Lógica de interacción para HomeView.xaml (Bandeja de Entrada)
    /// </summary>
    public partial class HomeView : Window
    {
        // El constructor ahora recibe el ID del perfil del usuario logueado
        public HomeView(int idPerfilLogueado)
        {
            InitializeComponent();

            // --- CORRECCIÓN ---
            // 1. Usamos InboxViewModel (la bandeja de chats)
            var viewModel = new HomeViewModel(idPerfilLogueado);
            
            // 2. Asigna el ViewModel al DataContext
            DataContext = viewModel;
        }

        // --- MÉTODOS RESTAURADOS PARA CORREGIR ERRORES DE COMPILACIÓN ---

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            // Cierra la aplicación
            Application.Current.Shutdown();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            // Minimiza la ventana
            this.WindowState = WindowState.Minimized;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // 3. Conectamos al método que YA EXISTE en InboxViewModel
            if (DataContext is HomeViewModel viewModel)
            {
                // Este método 'AbrirConfiguracion' ya existe en InboxViewModel
                viewModel.NavegarAConfiguracion(); 
            }
        }
    }
}
