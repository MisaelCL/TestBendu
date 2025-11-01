using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Model;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Administra la información y acciones del perfil del alumno en la interfaz.
    /// </summary>
    public sealed class PerfilViewModel : BaseViewModel
    {
        private readonly IPerfilRepository _perfilRepository;
        private int _idPerfil;
        private int _idCuenta;
        private string _nikName = string.Empty;
        private string _descripcion = string.Empty;
        private ImageSource _fotoPerfil;
        private byte[] _fotoPerfilBytes;
        private bool _hasUnread;
        private int _unreadCount;
        private bool _isBusy;
        private DateTime _fechaCreacion;

        public PerfilViewModel(IPerfilRepository perfilRepository)
        {
            _perfilRepository = perfilRepository;
            Publicaciones = new ObservableCollection<PublicacionItemViewModel>();
            ComandoRegresar = new RelayCommand(_ => Regresar());
            ComandoAbrirMenu = new RelayCommand(_ => { });
            ComandoEditarPerfil = new RelayCommand(_ => { });
            ComandoSubirFoto = new RelayCommand(_ => SubirFoto());
            ComandoGuardarDescripcion = new RelayCommand(_ => GuardarCambios(), _ => !IsBusy);
        }

        public ObservableCollection<PublicacionItemViewModel> Publicaciones { get; }

        public ICommand ComandoRegresar { get; }
        public ICommand ComandoAbrirMenu { get; }
        public ICommand ComandoEditarPerfil { get; }
        public ICommand ComandoSubirFoto { get; }
        public ICommand ComandoGuardarDescripcion { get; }

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
            get => _fotoPerfil;
            private set => EstablecerPropiedad(ref _fotoPerfil, value);
        }

        public bool HasUnread
        {
            get => _hasUnread;
            private set => EstablecerPropiedad(ref _hasUnread, value);
        }

        public int UnreadCount
        {
            get => _unreadCount;
            private set => EstablecerPropiedad(ref _unreadCount, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (EstablecerPropiedad(ref _isBusy, value))
                {
                    (ComandoGuardarDescripcion as RelayCommand)?.NotificarCambioPuedeEjecutar();
                }
            }
        }

        /// <summary>
        /// Carga la información del perfil asociado a la cuenta indicada.
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
            NikName = perfil.Nikname;
            Descripcion = perfil.Biografia;
            _fotoPerfilBytes = perfil.FotoPerfil;
            FotoPerfilUrl = ConvertirAImagen(_fotoPerfilBytes);
            _fechaCreacion = perfil.FechaCreacion;

            HasUnread = false;
            UnreadCount = 0;
        }

        /// <summary>
        /// Permite al usuario seleccionar una nueva foto de perfil.
        /// </summary>
        private void SubirFoto()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp",
                Title = "Seleccionar foto de perfil"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            _fotoPerfilBytes = File.ReadAllBytes(dialog.FileName);
            FotoPerfilUrl = new BitmapImage(new Uri(dialog.FileName));
        }

        /// <summary>
        /// Guarda los cambios realizados sobre la descripción o imagen del perfil.
        /// </summary>
        private void GuardarCambios()
        {
            if (_idPerfil == 0)
            {
                MessageBox.Show("No se ha cargado un perfil", "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                var updated = _perfilRepository.ActualizarPerfil(perfil);
                if (updated)
                {
                    MessageBox.Show("Perfil actualizado", "Perfil", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No fue posible actualizar el perfil", "Perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Perfil", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Cierra la ventana actual y regresa a la vista anterior.
        /// </summary>
        private void Regresar()
        {
            Application.Current?.Windows[Application.Current.Windows.Count - 1]?.Close();
        }

        /// <summary>
        /// Convierte un arreglo de bytes en una imagen para su visualización.
        /// </summary>
        private static ImageSource ConvertirAImagen(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }

    /// <summary>
    /// Representa una publicación mostrada en el perfil del alumno.
    /// </summary>
    public sealed class PublicacionItemViewModel
    {
        public string AvatarUrl { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string FechaPublicacion { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
    }
}
