using System.Windows;
using C_C_Final.Repositories;
using C_C_Final.Services;
using C_C_Final.View;
using C_C_Final.ViewModel;

namespace C_C_Final
{
    /// <summary>
    ///     Punto central de arranque de la aplicación. Esta clase actúa como el "composition root"
    ///     resolviendo la cadena de conexión y materializando las dependencias concretas que se van a
    ///     reutilizar en el resto de la aplicación (repositorios y servicios).
    /// </summary>
    public partial class App : System.Windows.Application
    {
        /// <summary>
        ///     Cadena de conexión que se comparte por todas las capas de acceso a datos. Se resuelve una
        ///     sola vez al iniciar la aplicación y se reutiliza para cada repositorio/servicio.
        /// </summary>
        public string ConnectionString { get; private set; } = string.Empty;

        /// <summary>
        ///     Repositorio orientado a las entidades Cuenta/Alumno. Expone operaciones de autenticación y
        ///     registro, por lo que resulta imprescindible tenerlo listo desde el arranque.
        /// </summary>
        public CuentaRepository CuentaRepository { get; private set; } = null!;

        /// <summary>
        ///     Repositorio que encapsula la lectura de perfiles (datos públicos), imprescindible para la
        ///     experiencia de sugerencias y edición de la información visible por otros usuarios.
        /// </summary>
        public PerfilRepository PerfilRepository { get; private set; } = null!;

        /// <summary>
        ///     Repositorio para las preferencias de búsqueda. Se inyecta en servicios que actualizan filtros
        ///     y se reutiliza en la generación de sugerencias.
        /// </summary>
        public PreferenciasRepository PreferenciasRepository { get; private set; } = null!;

        /// <summary>
        ///     Repositorio que maneja la persistencia de matches, chats y mensajes. Es compartido entre
        ///     distintos ViewModels y servicios de negocio.
        /// </summary>
        public MatchRepository MatchRepository { get; private set; } = null!;

        /// <summary>
        ///     Servicio que coordina el proceso de registro de un alumno asegurando operaciones atómicas.
        /// </summary>
        public RegisterAlumnoService RegisterAlumnoService { get; private set; } = null!;

        /// <summary>
        ///     Servicio que envuelve la lógica transaccional relacionada con matches y chats.
        /// </summary>
        public MatchService MatchService { get; private set; } = null!;

        /// <summary>
        ///     Servicio auxiliar para eliminar una cuenta y sus entidades asociadas (perfiles, matches, etc.).
        /// </summary>
        public CuentaDeletionService CuentaDeletionService { get; private set; } = null!;

        /// <summary>
        ///     Acceso tipado a la instancia actual de la aplicación para simplificar el acceso al contenedor
        ///     manual de dependencias desde otras capas.
        /// </summary>
        public static new App Current => (App)System.Windows.Application.Current;

        /// <summary>
        ///     Durante el evento de inicio se construyen los repositorios y servicios y se muestra la ventana
        ///     inicial de login con su ViewModel configurado. Este método concentra toda la orquestación
        ///     inicial de la aplicación.
        /// </summary>
        /// <param name="e">Argumentos de inicio proporcionados por WPF.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Resolver la cadena de conexión desde App.config a través del repositorio base.
            ConnectionString = RepositoryBase.ResolverCadenaConexion();

            // 2. Instanciar los repositorios concretos compartidos por toda la aplicación.
            CuentaRepository = new CuentaRepository(ConnectionString);
            PerfilRepository = new PerfilRepository(ConnectionString);
            PreferenciasRepository = new PreferenciasRepository(ConnectionString);
            MatchRepository = new MatchRepository(ConnectionString);

            // 3. Instanciar los servicios que dependen de los repositorios anteriores.
            RegisterAlumnoService = new RegisterAlumnoService(CuentaRepository, PerfilRepository, PreferenciasRepository, ConnectionString);
            MatchService = new MatchService(MatchRepository, ConnectionString);
            CuentaDeletionService = new CuentaDeletionService(CuentaRepository, PerfilRepository, MatchRepository);

            // 4. Crear la ventana inicial de la aplicación (login) suministrándole su ViewModel con
            //    las dependencias necesarias.
            var login = new LoginView
            {
                DataContext = new LoginViewModel(CuentaRepository, PerfilRepository)
            };

            // 5. Establecer la ventana principal y mostrarla al usuario.
            MainWindow = login;
            login.Show();
        }
    }
}
