using System.Windows;
using C_C_Final.Presentation.Helpers;
using C_C_Final.View;

namespace C_C_Final
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppBootstrapper.Initialize();
            var login = new LoginView();
            login.Show();
        }
    }
}
