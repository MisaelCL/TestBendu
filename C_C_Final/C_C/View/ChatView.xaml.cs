using System;
using System.Windows;
using C_C_Final.ViewModel;

namespace C_C_Final.View
{
    public partial class ChatView : Window
    {
        private readonly int _matchId;
        private readonly int _perfilActualId;

        public ChatView() : this(0, 0)
        {
        }

        public ChatView(int matchId, int perfilActualId)
        {
            _matchId = matchId;
            _perfilActualId = perfilActualId;
            InitializeComponent();
            var app = App.Current;
            if (app != null)
            {
                var viewModel = new ChatViewModel(app.MatchRepository, app.PerfilRepository, app.MatchService);
                DataContext = viewModel;
                Loaded += (_, _) => CargarVista(viewModel, _matchId, _perfilActualId);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static void CargarVista(ChatViewModel viewModel, int matchId, int perfilActualId)
        {
            try
            {
                viewModel.Cargar(matchId, perfilActualId);
            }
            catch (Exception)
            {
                // Se ignoran los errores iniciales para permitir que la vista cargue sin datos.
            }
        }
    }
}
