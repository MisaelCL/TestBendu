using System;
using System.Windows;

namespace C_C.Services
{
    public class NavigationService
    {
        public void Navigate(Window current, Window destination)
        {
            destination.Show();
            current.Close();
        }
    }
}
