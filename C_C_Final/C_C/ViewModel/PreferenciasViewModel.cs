using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Model;
using C_C_Final.View;
using C_C_Final.Services;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Permite consultar y actualizar las preferencias del perfil del alumno.
    /// </summary>
    public sealed class PreferenciasViewModel : BaseViewModel
    {
        private const int EdadMinimaPermitida = 18;
        private const int EdadMaximaPermitida = 99;
        private const int EdadMaximaPredeterminada = 35;

        private readonly IPerfilRepository _perfilRepository;
        private readonly IPreferenciasRepository _preferenciasRepository;
        private readonly CuentaDeletionService _cuentaDeletionService;
        private int _idCuenta;
        private int _idPerfil;
        private int _idPreferencias;
        private string _nikName = string.Empty;
        private string _descripcion = string.Empty;
        private byte[] _fotoPerfilBytes;
        private ImageSource _fotoPerfilUrl;
        private DateTime _fechaCreacion;
        private int _edadMin = EdadMinimaPermitida;
        private int _edadMax = EdadMaximaPredeterminada;
        private string _otroPreferencia;
        private string _generoSeleccionado;
        private bool _isBusy;

        public PreferenciasViewModel(
            IPerfilRepository perfilRepository,
            IPreferenciasRepository preferenciasRepository,
            CuentaDeletionService cuentaDeletionService)
        {
            _perfilRepository = perfilRepository ?? throw new ArgumentNullException(nameof(perfilRepository));
            _preferenciasRepository = preferenciasRepository ?? throw new ArgumentNullException(nameof(preferenciasRepository));
            _cuentaDeletionService = cuentaDeletionService ?? throw new ArgumentNullException(nameof(cuentaDeletionService));
            Generos = new ObservableCollection<string>(new[]
            {
                "Sin preferencia",
                "Solo mujeres",
                "Solo hombres",
                "Todos"
            });
            EditarFotoCommand = new RelayCommand(_ => EditarFoto());
            GuardarPerfilCommand = new RelayCommand(_ => GuardarCambios(), _ => !IsBusy);
            CerrarSesionCommand = new RelayCommand(_ => CerrarSesion());
            EliminarCuentaCommand = new RelayCommand(_ => EliminarCuenta(), _ => !IsBusy);
        }

        public ObservableCollection<string> Generos { get; }

        public string NikName
        {
            get => _nikName;
            set => EstablecerPropiedad(ref _nikName, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set => EstablecerPropiedad(ref _descripcion, value);
        }

        public ImageSource FotoPerfilUrl
        {
            get => _fotoPerfilUrl;
            private set => EstablecerPropiedad(ref _fotoPerfilUrl, value);
        }

        public int EdadMin
        {
            get => _edadMin;
            set
            {
                var nuevoValor = NormalizarEdadMinima(value);

                if (nuevoValor > _edadMax)
                {
                    _edadMax = nuevoValor;
                    NotificarCambioPropiedad(nameof(EdadMax));
                }

                EstablecerPropiedad(ref _edadMin, nuevoValor);
            }
        }

        public int EdadMax
        {
            get => _edadMax;
            set
            {
                if (value < _edadMin)
                {
                    _edadMin = value;
                    NotificarCambioPropiedad(nameof(EdadMin));
                }

                EstablecerPropiedad(ref _edadMax, value);
            }
        }

        public string OtroPreferencia
        {
            get => _otroPreferencia;
            set => EstablecerPropiedad(ref _otroPreferencia, value);
        }

        public string GeneroSeleccionado
        {
            get => _generoSeleccionado;
            set => EstablecerPropiedad(ref _generoSeleccionado, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (EstablecerPropiedad(ref _isBusy, value))
                {
                    (GuardarPerfilCommand as RelayCommand)?.NotificarCambioPuedeEjecutar();
                    (EliminarCuentaCommand as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        public ICommand EditarFotoCommand { get; }
        public ICommand EditarNombreCommand { get; }
        public ICommand GuardarPerfilCommand { get; }
        public ICommand EditarDescripcionCommand { get; }
        public ICommand CerrarSesionCommand { get; }
        public ICommand EliminarCuentaCommand { get; }

        /// <summary>
        /// Carga las preferencias almacenadas para la cuenta indicada.
        /// </summary>
        public void Cargar(int cuentaId)
        {
            _idCuenta = cuentaId;
            var perfil = _perfilRepository.ObtenerPorCuentaId(cuentaId);
            if (perfil == null)
            {
                return;
            }

            _idPerfil = perfil.IdPerfil;
            _fechaCreacion = perfil.FechaCreacion;
            NikName = perfil.Nikname;
            Descripcion = perfil.Biografia;
            _fotoPerfilBytes = perfil.FotoPerfil ?? Array.Empty<byte>();
            FotoPerfilUrl = ConvertirAImagen(_fotoPerfilBytes);

            var preferencias = _preferenciasRepository.ObtenerPorPerfilId(_idPerfil);
            if (preferencias != null)
            {
                _idPreferencias = preferencias.IdPreferencias;
                var edadMinima = NormalizarEdadMinima(preferencias.EdadMinima);
                var edadMaxima = NormalizarEdadMaxima(preferencias.EdadMaxima, edadMinima);

                if (preferencias.EdadMinima == 0 && preferencias.EdadMaxima == 0)
                {
                    edadMinima = EdadMinimaPermitida;
                    edadMaxima = EdadMaximaPredeterminada;
                }
                else if (edadMaxima < edadMinima)
                {
                    edadMaxima = edadMinima;
                }

                EdadMin = edadMinima;
                EdadMax = edadMaxima;
                OtroPreferencia = preferencias.PreferenciaCarrera;
                GeneroSeleccionado = MapearGeneroDesdeCodigo(preferencias.PreferenciaGenero);
            }
            else
            {
                _idPreferencias = 0;
                GeneroSeleccionado = Generos[0];
            }

            if (string.IsNullOrWhiteSpace(GeneroSeleccionado))
            {
                GeneroSeleccionado = Generos[0];
            }
        }

        /// <summary>
        /// Persiste los cambios realizados sobre el perfil y sus preferencias.
        /// </summary>
        private void GuardarCambios()
        {
            if (_idPerfil == 0)
            {
                MessageBox.Show("No se ha cargado un perfil", "Preferencias", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsBusy = true;
                var perfil = new Perfil
                {
                    IdPerfil = _idPerfil,
                    IdCuenta = _idCuenta,
                    Nikname = NikName,
                    Biografia = Descripcion,
                    FotoPerfil = _fotoPerfilBytes,
                    FechaCreacion = _fechaCreacion
                };

                if (!_perfilRepository.ActualizarPerfil(perfil))
                {
                    throw new InvalidOperationException("No se pudieron guardar los cambios del perfil.");
                }

                var preferencias = new Preferencias
                {
                    IdPreferencias = _idPreferencias,
                    IdPerfil = _idPerfil,
                    PreferenciaGenero = MapearGeneroDesdeTexto(GeneroSeleccionado),
                    EdadMinima = EdadMin,
                    EdadMaxima = EdadMax,
                    PreferenciaCarrera = OtroPreferencia ?? string.Empty,
                    Intereses = string.Empty
                };

                if (_idPreferencias > 0)
                {
                    if (!_preferenciasRepository.ActualizarPreferencias(preferencias))
                    {
                        throw new InvalidOperationException("No se pudieron actualizar las preferencias.");
                    }
                }
                else
                {
                    var newId = _preferenciasRepository.CrearPreferencias(preferencias);
                    if (newId <= 0)
                    {
                        throw new InvalidOperationException("No se pudieron guardar las preferencias.");
                    }

                    _idPreferencias = newId;
                }

                MessageBox.Show("Preferencias actualizadas", "Preferencias", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Preferencias", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void EliminarCuenta()
        {
            if (_idCuenta == 0)
            {
                MessageBox.Show("No se ha cargado una cuenta válida", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "Esta acción eliminará tu cuenta y todos tus datos de manera permanente. ¿Deseas continuar?",
                "Eliminar cuenta",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                IsBusy = true;
                _cuentaDeletionService.EliminarCuenta(_idCuenta);
                MessageBox.Show("Tu cuenta se eliminó correctamente.", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Information);
                CerrarSesion();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Cuenta", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al eliminar la cuenta: {ex.Message}", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static void CerrarSesion()
        {
            var loginView = new LoginView();
            loginView.Show();

            var app = Application.Current;
            if (app == null)
            {
                return;
            }

            var ventanas = app.Windows.Cast<Window>().ToList();
            foreach (var window in ventanas)
            {
                if (!ReferenceEquals(window, loginView))
                {
                    window.Close();
                }
            }
        }

        private static int NormalizarEdad(int edad, int minimo, int maximo)
        {
            return Math.Clamp(edad, minimo, maximo);
        }

        private static int NormalizarEdadMinima(int edad)
        {
            return NormalizarEdad(edad, EdadMinimaPermitida, EdadMaximaPermitida);
        }

        private static int NormalizarEdadMaxima(int edad, int limiteInferior)
        {
            return NormalizarEdad(edad, Math.Max(limiteInferior, EdadMinimaPermitida), EdadMaximaPermitida);
        }

        private void EditarFoto()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Seleccionar foto"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _fotoPerfilBytes = File.ReadAllBytes(dialog.FileName);
            FotoPerfilUrl = new BitmapImage(new Uri(dialog.FileName));
        }

        private static string MapearGeneroDesdeCodigo(byte preferencia)
        {
            switch (preferencia)
            {
                case 1:
                    return "Solo mujeres";
                case 2:
                    return "Solo hombres";
                case 3:
                    return "Todos";
                default:
                    return "Sin preferencia";
            }
        }

        private static byte MapearGeneroDesdeTexto(string preferencia)
        {
            switch (preferencia)
            {
                case "Solo mujeres":
                    return 1;
                case "Solo hombres":
                    return 2;
                case "Todos":
                    return 3;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Convierte un arreglo de bytes a una imagen compatible con WPF.
        /// </summary>
        private static ImageSource ConvertirAImagen(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
