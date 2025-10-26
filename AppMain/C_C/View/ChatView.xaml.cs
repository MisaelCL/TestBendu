using System;
using System.Windows;
using C_C_Final.Helpers;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class ChatView : Window
    {
        public ChatView()
        {
            InitializeComponent();
            var viewModel = AppBootstrapper.CreateChatViewModel();
            DataContext = viewModel;
            Loaded += (_, _) => Load(viewModel);
        }

        private static void Load(ChatViewModel viewModel)
        {
            try
            {
                viewModel.Load(0, 0);
            }
            catch (Exception)
            {
                // Se ignoran los errores iniciales para permitir que la vista cargue sin datos.
            }
        }
    }
}
