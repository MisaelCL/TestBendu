using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C_C_Final.Application.Repositories;
using C_C_Final.Domain.Models;
using C_C_Final.Presentation.Commands;

namespace C_C_Final.Presentation.ViewModels
{
    public sealed class PreferenciasViewModel : BaseViewModel
    {
        private readonly IPerfilRepository _perfilRepository;
        private int _idCuenta;
        private int _idPerfil;
        private string _nikName = string.Empty;
        private string _descripcion = string.Empty;
        private byte[]? _fotoPerfilBytes;
        private ImageSource? _fotoPerfilUrl;
        private DateTime _fechaCreacion;
        private int _edadMin = 18;
        private int _edadMax = 35;
        private string? _otroPreferencia;
        private string? _generoSeleccionado;
        private bool _isBusy;

        public PreferenciasViewModel(IPerfilRepository perfilRepository)
        {
            _perfilRepository = perfilRepository;
            Generos = new ObservableCollection<string>(new[]
            {
                "Sin preferencia",
                "Solo mujeres",
                "Solo hombres",
                "Todos"
            });

            GoBackCommand = new RelayCommand(_ => CloseWindow());
            EditarFotoCommand = new RelayCommand(async _ => await EditarFotoAsync());
            EditarNombreCommand = new RelayCommand(_ => { });
            GuardarPerfilCommand = new RelayCommand(async _ => await GuardarAsync(), _ => !IsBusy);
            EditarDescripcionCommand = new RelayCommand(_ => { });
            LogoutCommand = new RelayCommand(_ => MessageBox.Show("Sesión finalizada", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Information));
            EliminarCuentaCommand = new RelayCommand(_ => MessageBox.Show("Para eliminar tu cuenta contacta al administrador.", "Cuenta", MessageBoxButton.OK, MessageBoxImage.Information));
        }

        public ObservableCollection<string> Generos { get; }

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

        public ImageSource? FotoPerfilUrl
        {
            get => _fotoPerfilUrl;
            private set => SetProperty(ref _fotoPerfilUrl, value);
        }

        public int EdadMin
        {
            get => _edadMin;
            set
            {
                if (value > _edadMax)
                {
                    _edadMax = value;
                    OnPropertyChanged(nameof(EdadMax));
                }

                SetProperty(ref _edadMin, value);
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
                    OnPropertyChanged(nameof(EdadMin));
                }

                SetProperty(ref _edadMax, value);
            }
        }

        public string? OtroPreferencia
        {
            get => _otroPreferencia;
            set => SetProperty(ref _otroPreferencia, value);
        }

        public string? GeneroSeleccionado
        {
            get => _generoSeleccionado;
            set => SetProperty(ref _generoSeleccionado, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (GuardarPerfilCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand GoBackCommand { get; }
        public ICommand EditarFotoCommand { get; }
        public ICommand EditarNombreCommand { get; }
        public ICommand GuardarPerfilCommand { get; }
        public ICommand EditarDescripcionCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand EliminarCuentaCommand { get; }

        public async Task LoadAsync(int cuentaId, CancellationToken ct = default)
        {
            _idCuenta = cuentaId;
            var perfil = await _perfilRepository.GetByCuentaIdAsync(cuentaId, ct);
            if (perfil == null)
            {
                return;
            }

            _idPerfil = perfil.IdPerfil;
            _fechaCreacion = perfil.FechaCreacion;
            NikName = perfil.Nikname;
            Descripcion = perfil.Biografia;
            _fotoPerfilBytes = perfil.FotoPerfil;
            FotoPerfilUrl = ConvertToImage(_fotoPerfilBytes);

            var preferencias = await _perfilRepository.GetPreferenciasByPerfilAsync(_idPerfil, ct);
            if (preferencias != null)
            {
                EdadMin = preferencias.EdadMinima;
                EdadMax = preferencias.EdadMaxima;
                OtroPreferencia = preferencias.PreferenciaCarrera;
                GeneroSeleccionado = MapGenero(preferencias.PreferenciaGenero);
            }
            else
            {
                GeneroSeleccionado = Generos[0];
            }
        }

        private async Task GuardarAsync()
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

                await _perfilRepository.UpdatePerfilAsync(perfil, CancellationToken.None);

                var preferencias = new Preferencias
                {
                    IdPerfil = _idPerfil,
                    PreferenciaGenero = MapGenero(GeneroSeleccionado),
                    EdadMinima = EdadMin,
                    EdadMaxima = EdadMax,
                    PreferenciaCarrera = OtroPreferencia ?? string.Empty,
                    Intereses = string.Empty
                };

                await _perfilRepository.UpsertPreferenciasAsync(preferencias, CancellationToken.None);
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

        private async Task EditarFotoAsync()
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

            _fotoPerfilBytes = await File.ReadAllBytesAsync(dialog.FileName);
            FotoPerfilUrl = new BitmapImage(new Uri(dialog.FileName));
        }

        private void CloseWindow()
        {
            Application.Current?.Windows[Application.Current.Windows.Count - 1]?.Close();
        }

        private static string MapGenero(byte preferencia)
        {
            return preferencia switch
            {
                1 => "Solo mujeres",
                2 => "Solo hombres",
                3 => "Todos",
                _ => "Sin preferencia"
            };
        }

        private static byte MapGenero(string? preferencia)
        {
            return preferencia switch
            {
                "Solo mujeres" => 1,
                "Solo hombres" => 2,
                "Todos" => 3,
                _ => 0
            };
        }

        private static ImageSource? ConvertToImage(byte[]? bytes)
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
}
