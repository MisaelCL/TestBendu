using C_C_Final.Model;
using C_C_Final.Services;
using C_C_Final.Repositories;
using C_C_Final.ViewModel;

namespace C_C_Final.Helpers
{
    public static class AppBootstrapper
    {
        private static bool _initialized;
        private static SqlConnectionFactory _connectionFactory = null!;
        private static CuentaRepository _cuentaRepository = null!;
        private static PerfilRepository _perfilRepository = null!;
        private static MatchRepository _matchRepository = null!;
        private static RegisterAlumnoService _registerAlumnoService = null!;
        private static MatchService _matchService = null!;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _connectionFactory = new SqlConnectionFactory();
            _cuentaRepository = new CuentaRepository(_connectionFactory);
            _perfilRepository = new PerfilRepository(_connectionFactory);
            _matchRepository = new MatchRepository(_connectionFactory);
            _registerAlumnoService = new RegisterAlumnoService(_cuentaRepository, _perfilRepository, _connectionFactory);
            _matchService = new MatchService(_matchRepository, _connectionFactory);
            _initialized = true;
        }

        public static RegistroViewModel CreateRegistroViewModel()
        {
            Initialize();
            return new RegistroViewModel(_registerAlumnoService);
        }

        public static PerfilViewModel CreatePerfilViewModel()
        {
            Initialize();
            return new PerfilViewModel(_perfilRepository);
        }

        public static PreferenciasViewModel CreatePreferenciasViewModel()
        {
            Initialize();
            return new PreferenciasViewModel(_perfilRepository);
        }

        public static InboxViewModel CreateInboxViewModel()
        {
            Initialize();
            return new InboxViewModel(_matchRepository, _perfilRepository, _matchService);
        }

        public static ChatViewModel CreateChatViewModel()
        {
            Initialize();
            return new ChatViewModel(_matchRepository, _perfilRepository, _matchService);
        }
    }
}
