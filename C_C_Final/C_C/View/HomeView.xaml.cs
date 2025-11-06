using C_C_Final.ViewModel; // Importar el namespace del ViewModel
using System.Windows;

namespace C_C_Final.View
{
    /// <summary>
    /// Lógica de interacción para HomeView.xaml
    /// </summary>
    public partial class HomeView : Window
    {
        // El constructor ahora recibe el ID del perfil del usuario logueado
        public HomeView(int idPerfilLogueado)
        {
            InitializeComponent();

            // --- LÍNEAS NUEVAS/MODIFICADAS ---
            // 1. Crea una instancia del nuevo ViewModel, pasándole el ID
            var viewModel = new HomeViewModel(idPerfilLogueado);
            
            // 2. Asigna el ViewModel al DataContext de la Ventana
            // (Esto conecta tus botones de Like/Rechazo en XAML a los Comandos del ViewModel)
            DataContext = viewModel;
            // --- FIN DE LÍNEAS NUEVAS ---
        }
    }
}
