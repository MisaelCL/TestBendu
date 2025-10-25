using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace C_C.View
{
    /// <summary>
    /// Lógica de interacción para LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }


        private void IrARegistro_Click(object sender, RoutedEventArgs e)
        {
            RegisterView ventanaDeRegistro = new RegisterView();

            ventanaDeRegistro.Show();

            this.Close();
        }


        private void Login_Click(object sender, RoutedEventArgs e)
        {
            HomeView home = new HomeView();

            home.Show();

            this.Close();
        }
    }
}
