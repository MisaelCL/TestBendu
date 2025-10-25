using ProyectoEjemplo.View;
using System.Windows;

namespace ProyectoEjemplo
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnLastWindowClose;

            var loginView = new LoginView();
            MainWindow = loginView;
            loginView.Show();
        }
    }
}