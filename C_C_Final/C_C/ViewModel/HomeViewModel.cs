using System;
using System.Windows; 
using System.Windows.Input;
using System.Linq; 
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.Services;
using C_C_Final.View; 

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// ViewModel para la vista principal (Home), donde se muestran
    /// y se interactúa con otros perfiles (like/rechazo).
    /// </summary>
    public sealed class HomeViewModel : BaseViewModel
    {
        // --- Repositorios y Servicios ---
        private readonly IPerfilRepository _perfilRepository;
        private readonly IMatchRepository _matchRepository;
        private readonly MatchService _matchService;
        private readonly int _idPerfilActual; 

        // --- Propiedades de Binding ---
        private Perfil _perfilMostrado;

        // --- Comandos ---
        public ICommand ComandoLike { get; }
        public ICommand ComandoRechazar { get; }
        public ICommand ComandoIrAPerfil { get; } 

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
            ComandoIrAPerfil = new RelayCommand(_ => NavegarAConfiguracion());
            
            // --- CORRECCIÓN ---
            // Carga el primer perfil al iniciar
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
                    // Notificar a los comandos que pueden (o no) ejecutarse
                    (ComandoLike as RelayCommand)?.NotificarCambioPuedeEjecutar();
                    (ComandoRechazar as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        // --- LÓGICA DE NAVEGACIÓN ---
        public void NavegarAConfiguracion()
        {
            try
            {
                var perfilActual = _perfilRepository.ObtenerPorId(_idPerfilActual);
                if (perfilActual == null || perfilActual.IdCuenta == 0)
                {
                    MessageBox.Show("No se pudo cargar la información de la cuenta.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int idCuenta = perfilActual.IdCuenta;
                var configuracionView = new ConfiguracionView(idCuenta);

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

        // --- LÓGICA DE "LIKE" Y "RECHAZO" ---
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
                    // Usa el MatchService (CORREGIDO) que YA NO crea un chat.
                    _matchService.CrearMatch(_idPerfilActual, idPerfilDestino, nuevoEstado);
                }
                else
                {
                    if (esLike)
                    {
                        if (MatchEstadoHelper.EsPendienteDe(matchExistente.Estado, idPerfilDestino))
                        {
                            // ¡Es un match mutuo!
                            _matchRepository.ActualizarEstado(matchExistente.IdMatch, "activo");
                            
                            // ¡AHORA SÍ, creamos el chat!
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
            
            // Cargar el siguiente perfil
            CargarSiguientePerfil();
        }

        // --- CORRECCIÓN ---
        private void CargarSiguientePerfil()
        {
            try
            {
                // Llama al nuevo método del repositorio
                PerfilMostrado = _perfilRepository.ObtenerSiguientePerfilPara(_idPerfilActual);

                if (PerfilMostrado == null)
                {
                    // Opcional: Mostrar un mensaje o un estado de "No hay más perfiles"
                    MessageBox.Show("¡Has visto todos los perfiles por ahora!", "Fin", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el siguiente perfil: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                PerfilMostrado = null;
            }
        }
    }
}
