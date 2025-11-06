using System;
using System.Windows; // Para MessageBox y Application
using System.Windows.Input;
using System.Linq; // Para Application.Current.Windows
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.Services;
using C_C_Final.View; // Para poder abrir ConfiguracionView

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// ViewModel para la vista principal (Home), donde se muestran
    /// y se interactúa con otros perfiles.
    /// </summary>
    public sealed class HomeViewModel : BaseViewModel
    {
        // --- Repositorios y Servicios ---
        private readonly IPerfilRepository _perfilRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly MatchService _matchService;
        private readonly int _idPerfilActual; // ID del usuario logueado

        // --- Propiedades de Binding ---
        private Perfil _perfilMostrado;

        // --- Comandos ---
        public ICommand ComandoLike { get; }
        public ICommand ComandoRechazar { get; }
        public ICommand ComandoIrAPerfil { get; } // Comando para el botón de perfil/ajustes

        // --- Constructor ---
        public HomeViewModel(int idPerfilLogueado)
        {
            _perfilRepository = new PerfilRepository();
            _matchRepository = new MatchRepository();
            _matchService = new MatchService(_matchRepository); 
            _idPerfilActual = idPerfilLogueado;

            // Inicializa Comandos
            ComandoLike = new RelayCommand(_ => RegistrarInteraccion(true), _ => PerfilMostrado != null);
            ComandoRechazar = new RelayCommand(_ => RegistrarInteraccion(false), _ => PerfilMostrado != null);
            
            // Este comando también puede navegar a la configuración
            ComandoIrAPerfil = new RelayCommand(_ => NavegarAConfiguracion());
            
            CargarSiguientePerfil();
        }

        // --- Propiedad para el Perfil en Pantalla ---
        public Perfil PerfilMostrado
        {
            get => _perfilMostrado;
            set
            {
                if (EstablecerPropiedad(ref _perfilMostrado, value))
                {
                    (ComandoLike as RelayCommand)?.NotificarCambioPuedeEjecutar();
                    (ComandoRechazar as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        // --- LÓGICA DE NAVEGACIÓN (MÉTODO NUEVO/CORREGIDO) ---

        /// <summary>
        /// Navega a la vista de configuración y cierra la HomeView actual.
        /// Este método es llamado por el code-behind de HomeView.xaml.cs
        /// </summary>
        public void NavegarAConfiguracion()
        {
            try
            {
                // 1. Obtener el Perfil actual para encontrar el IdCuenta
                var perfilActual = _perfilRepository.ObtenerPorId(_idPerfilActual);
                if (perfilActual == null || perfilActual.IdCuenta == 0)
                {
                    MessageBox.Show("No se pudo cargar la información de la cuenta.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idCuenta = perfilActual.IdCuenta;

                // 2. Abrir la ventana de configuración
                var configuracionView = new ConfiguracionView(idCuenta);

                // 3. Encontrar y cerrar la ventana Home actual
                // (Replicando la lógica de tu PerfilViewModel)
                Window ventanaActual = null;
                var app = Application.Current;
                if (app != null)
                {
                    foreach (Window window in app.Windows)
                    {
                        if (ReferenceEquals(window.DataContext, this))
                        {
                            ventanaActual = window;
                            break;
                        }
                    }
                }
                
                configuracionView.Show();
                ventanaActual?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir la configuración: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // --- LÓGICA DE "LIKE" Y "RECHAZO" (EXISTENTE) ---
        private void RegistrarInteraccion(bool esLike)
        {
            if (PerfilMostrado == null) return;

            int idPerfilDestino = PerfilMostrado.IdPerfil;

            try
            {
                var matchExistente = _matchRepository.ObtenerPorPerfiles(_idPerfilActual, idPerfilDestino);

                if (matchExistente == null)
                {
                    string nuevoEstado;
                    if (esLike)
                    {
                        nuevoEstado = MatchEstadoHelper.ConstruirPendiente(_idPerfilActual); 
                    }
                    else
                    {
                        nuevoEstado = MatchEstadoHelper.ConstruirRechazado(_idPerfilActual); 
                    }
                    _matchService.CrearMatch(_idPerfilActual, idPerfilDestino, nuevoEstado);
                }
                else
                {
                    if (esLike)
                    {
                        if (MatchEstadoHelper.EsPendienteDe(matchExistente.Estado, idPerfilDestino))
                        {
                            _matchRepository.ActualizarEstado(matchExistente.IdMatch, "activo");
                            _matchService.AsegurarChatParaMatch(matchExistente.IdMatch);
                            MessageBox.Show($"¡Hiciste match con {PerfilMostrado.Nikname}!", "¡Es un Match!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        string nuevoEstado = MatchEstadoHelper.ConstruirRechazado(_idPerfilActual);
                        _matchRepository.ActualizarEstado(matchExistente.IdMatch, nuevoEstado);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la interacción: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            CargarSiguientePerfil();
        }

        private void CargarSiguientePerfil()
        {
            // PerfilMostrado = _perfilRepository.ObtenerSiguientePerfilPara(_idPerfilActual);
            PerfilMostrado = null; 
        }
    }
}
