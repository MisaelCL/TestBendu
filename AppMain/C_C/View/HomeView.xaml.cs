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
    /// Lógica de interacción para HomeView.xaml
    /// </summary>
    public partial class HomeView : Window
    {
        public HomeView()
        {
            InitializeComponent();
        }



        private void BotonIrAChats_Click(object sender, RoutedEventArgs e)
        {

            // ChatWindow ventanaDeChats = new ChatWindow();

            //ventanaDeChats.Show();

            //this.Close();
        }


        private void MoverVentana(object sender, MouseButtonEventArgs e)
        {

            if (e.ButtonState == MouseButtonState.Pressed)
            {

                this.DragMove();
            }
        }
    }
}
