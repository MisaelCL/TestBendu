using System.Windows;
using C_C_Final.Repositories;
using C_C_Final.Services;
using C_C_Final.View;
using C_C_Final.ViewModel;

namespace C_C_Final
{
    public partial class App : System.Windows.Application
    {
        public string ConnectionString { get; private set; }
        public CuentaRepository CuentaRepository { get; private set; }
        public PerfilRepository PerfilRepository { get; private set; }
        public MatchRepository MatchRepository { get; private set; }
        public RegisterAlumnoService RegisterAlumnoService { get; private set; }
        public MatchService MatchService { get; private set; }

        public static new App Current => (App)System.Windows.Application.Current;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConnectionString = RepositoryBase.ResolveConnectionString(null);
            CuentaRepository = new CuentaRepository(ConnectionString);
            PerfilRepository = new PerfilRepository(ConnectionString);
            MatchRepository = new MatchRepository(ConnectionString);
            RegisterAlumnoService = new RegisterAlumnoService(CuentaRepository, PerfilRepository, ConnectionString);
            MatchService = new MatchService(MatchRepository, ConnectionString);

            var login = new LoginView
            {
                DataContext = new LoginViewModel(CuentaRepository, PerfilRepository)
            };
            login.Show();
        }
    }
}
