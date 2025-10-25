using C_C.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace C_C
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loginview = new LoginView();
            loginview.Show();
            loginview.IsVisibleChanged += (s, ev) =>
            {
                if (loginview.IsVisible == false && loginview.IsLoaded)
                {
                    var mainView = new MainWindow();
                    mainView.Show();
                    loginview.Close();
                }
            };
        }   
    }
}
