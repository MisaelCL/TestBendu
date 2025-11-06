using System.Windows;
using C_C_Final.Repositories;
using C_C_Final.Services;
using C_C_Final.View;
using C_C_Final.ViewModel;

namespace C_C_Final
{
    public partial class App : System.Windows.Application
    {
        public string ConnectionString { get; private set; } = string.Empty;
        public CuentaRepository CuentaRepository { get; private set; } = null!;
        public PerfilRepository PerfilRepository { get; private set; } = null!;
        public PreferenciasRepository PreferenciasRepository { get; private set; } = null!;
        public MatchRepository MatchRepository { get; private set; } = null!;
        public RegisterAlumnoService RegisterAlumnoService { get; private set; } = null!;
        public MatchService MatchService { get; private set; } = null!;
        public CuentaDeletionService CuentaDeletionService { get; private set; } = null!;
        public static new App Current => (App)System.Windows.Application.Current;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ConnectionString = RepositoryBase.ResolverCadenaConexion(null);
            CuentaRepository = new CuentaRepository(ConnectionString);
            PerfilRepository = new PerfilRepository(ConnectionString);
            PreferenciasRepository = new PreferenciasRepository(ConnectionString);
            MatchRepository = new MatchRepository(ConnectionString);
            RegisterAlumnoService = new RegisterAlumnoService(CuentaRepository, PerfilRepository, PreferenciasRepository, ConnectionString);
            MatchService = new MatchService(MatchRepository, ConnectionString);
            CuentaDeletionService = new CuentaDeletionService(CuentaRepository, PerfilRepository, MatchRepository);

            var login = new LoginView
            {
                DataContext = new LoginViewModel(CuentaRepository, PerfilRepository)
            };

            MainWindow = login;
            login.Show();
        }
    }
}
