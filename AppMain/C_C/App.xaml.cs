using System.Windows;
using C_C_Final.Helpers;
using C_C_Final.View;

namespace C_C_Final
{
    public partial class App : System.Windows.Application
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
