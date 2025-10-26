using System;
using System.Threading.Tasks;
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
            Loaded += async (_, _) => await LoadAsync(viewModel).ConfigureAwait(false);
        }

        private static async Task LoadAsync(ChatViewModel viewModel)
        {
            try
            {
                await viewModel.LoadAsync(0, 0).ConfigureAwait(false);
            }
            catch (Exception)
            {
                // Se ignoran los errores iniciales para permitir que la vista cargue sin datos.
            }
        }
    }
}
