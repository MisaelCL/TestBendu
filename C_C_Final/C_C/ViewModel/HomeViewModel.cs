using System;
using System.Windows; // Para MessageBox
using System.Windows.Input;
using C_C_Final.Model;
using C_C_Final.Repositories;
using C_C_Final.Services; // Necesario para MatchService

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
        // (Aquí iría tu lógica para cargar perfiles, ej: una lista o cola)

        // --- Comandos ---
        public ICommand ComandoLike { get; }
        public ICommand ComandoRechazar { get; }
        public ICommand ComandoIrAPerfil { get; }
        // (Otros comandos que puedas necesitar)

        // --- Constructor ---
        // Asume que el ID del perfil logueado se pasa al crear la vista
        public HomeViewModel(int idPerfilLogueado)
        {
            // Inicializa repositorios y servicios
            // (Siguiendo el patrón de tus otros ViewModels)
            _perfilRepository = new PerfilRepository();
            _matchRepository = new MatchRepository();
            
            // IMPORTANTE: Inyectamos el MatchRepository al MatchService
            _matchService = new MatchService(_matchRepository); 

            _idPerfilActual = idPerfilLogueado;

            // Inicializa Comandos
            ComandoLike = new RelayCommand(_ => RegistrarInteraccion(true), _ => PerfilMostrado != null);
            ComandoRechazar = new RelayCommand(_ => RegistrarInteraccion(false), _ => PerfilMostrado != null);
            ComandoIrAPerfil = new RelayCommand(_ => AbrirMiPerfil());

            // Carga el primer perfil para mostrar
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
                    // Notificar a los comandos que el estado 'CanExecute' puede haber cambiado
                    (ComandoLike as RelayCommand)?.NotificarCambioPuedeEjecutar();
                    (ComandoRechazar as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        // --- LÓGICA DE "LIKE" Y "RECHAZO" ---
        private void RegistrarInteraccion(bool esLike)
        {
            if (PerfilMostrado == null) return;

            int idPerfilDestino = PerfilMostrado.IdPerfil;

            try
            {
                // 1. Buscar si ya existe una interacción previa
                var matchExistente = _matchRepository.ObtenerPorPerfiles(_idPerfilActual, idPerfilDestino);

                if (matchExistente == null)
                {
                    // 2. No existe interacción previa.
                    string nuevoEstado;
                    if (esLike)
                    {
                        // Es el primer 'like', queda pendiente del lado actual
                        nuevoEstado = MatchEstadoHelper.ConstruirPendiente(_idPerfilActual); // "pendiente:123"
                    }
                    else
                    {
                        // Es el primer 'rechazo'
                        nuevoEstado = MatchEstadoHelper.ConstruirRechazado(_idPerfilActual); // "rechazado:123"
                    }
                    
                    // Usamos el MatchService (CORREGIDO) que YA NO crea un chat.
                    _matchService.CrearMatch(_idPerfilActual, idPerfilDestino, nuevoEstado);
                }
                else
                {
                    // 3. Ya existe una interacción.
                    if (esLike)
                    {
                        // El usuario actual da 'Like'
                        // ¿Estaba pendiente por parte del OTRO usuario?
                        if (MatchEstadoHelper.EsPendienteDe(matchExistente.Estado, idPerfilDestino))
                        {
                            // ¡Es un match mutuo!
                            _matchRepository.ActualizarEstado(matchExistente.IdMatch, "activo");
                            
                            // ¡AHORA SÍ, creamos el chat!
                            _matchService.AsegurarChatParaMatch(matchExistente.IdMatch);
                            
                            MessageBox.Show($"¡Hiciste match con {PerfilMostrado.Nikname}!", "¡Es un Match!", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        // Si ya estaba activo, rechazado, o pendiente por el usuario actual, no hacemos nada.
                    }
                    else
                    {
                        // El usuario actual 'Rechaza'
                        // Actualizamos el estado a rechazado por el usuario actual.
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

        private void CargarSiguientePerfil()
        {
            // --- LÓGICA DE EJEMPLO ---
            // Aquí debes implementar tu lógica para obtener el siguiente
            // perfil de la base de datos que no sea el usuario actual
            // y que no haya sido ya interactuado (o filtrar por preferencias).
            
            // Esta es una implementación placeholder (debes crear este método):
            // PerfilMostrado = _perfilRepository.ObtenerSiguientePerfilPara(_idPerfilActual);
            
            // Por ahora, lo dejaremos en null.
            PerfilMostrado = null; 
            
            if (PerfilMostrado == null)
            {
                // (Opcional) Mostrar un mensaje de que no hay más perfiles
            }
        }

        private void AbrirMiPerfil()
        {
            // (Esta lógica es un placeholder)
            MessageBox.Show("Navegación al perfil personal aún no implementada en HomeViewModel.", "Aviso");
        }
    }
}
