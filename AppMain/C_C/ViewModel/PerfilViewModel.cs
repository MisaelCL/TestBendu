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
            GoBackCommand = new RelayCommand(_ => OnGoBack());
            OpenMenuCommand = new RelayCommand(_ => { });
            EditarPerfilCommand = new RelayCommand(_ => { });
            SubirFotoCommand = new RelayCommand(_ => SubirFoto());
            GuardarDescripcionCommand = new RelayCommand(_ => Guardar(), _ => !IsBusy);
        }

        public ObservableCollection<PublicacionItemViewModel> Publicaciones { get; }

        public ICommand GoBackCommand { get; }
        public ICommand OpenMenuCommand { get; }
        public ICommand EditarPerfilCommand { get; }
        public ICommand SubirFotoCommand { get; }
        public ICommand GuardarDescripcionCommand { get; }

        public string NikName
        {
            get => _nikName;
            set => SetProperty(ref _nikName, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set => SetProperty(ref _descripcion, value);
        }

        public ImageSource FotoPerfilUrl
        {
            get => _fotoPerfil;
            private set => SetProperty(ref _fotoPerfil, value);
        }

        public bool HasUnread
        {
            get => _hasUnread;
            private set => SetProperty(ref _hasUnread, value);
        }

        public int UnreadCount
        {
            get => _unreadCount;
            private set => SetProperty(ref _unreadCount, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (GuardarDescripcionCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public void Load(int cuentaId)
        {
            _idCuenta = cuentaId;
            var perfil = _perfilRepository.GetByCuentaId(cuentaId);
            if (perfil == null)
            {
                return;
            }

            _idPerfil = perfil.IdPerfil;
            NikName = perfil.Nikname;
            Descripcion = perfil.Biografia;
            _fotoPerfilBytes = perfil.FotoPerfil;
            FotoPerfilUrl = ConvertToImage(_fotoPerfilBytes);
            _fechaCreacion = perfil.FechaCreacion;

            HasUnread = false;
            UnreadCount = 0;
        }

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

        private void Guardar()
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

                var updated = _perfilRepository.UpdatePerfil(perfil);
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

        private void OnGoBack()
        {
            Application.Current?.Windows[Application.Current.Windows.Count - 1]?.Close();
        }

        private static ImageSource ConvertToImage(byte[] bytes)
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

    public sealed class PublicacionItemViewModel
    {
        public string AvatarUrl { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string FechaPublicacion { get; set; } = string.Empty;
        public string ImagenUrl { get; set; } = string.Empty;
    }
}
