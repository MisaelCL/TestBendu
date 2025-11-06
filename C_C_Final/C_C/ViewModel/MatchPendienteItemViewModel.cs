using System;
using System.Windows.Media;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Representa un match pendiente que debe ser aceptado o rechazado por el usuario.
    /// </summary>
    public sealed class MatchPendienteItemViewModel : BaseViewModel
    {
        private int _matchId;
        private int _perfilSolicitanteId;
        private string _nombreContacto = string.Empty;
        private string _descripcion = string.Empty;
        private ImageSource? _fotoPerfil;
        private DateTime _fechaMatch;

        public int MatchId
        {
            get => _matchId;
            set => EstablecerPropiedad(ref _matchId, value);
        }

        public int PerfilSolicitanteId
        {
            get => _perfilSolicitanteId;
            set => EstablecerPropiedad(ref _perfilSolicitanteId, value);
        }

        public string NombreContacto
        {
            get => _nombreContacto;
            set => EstablecerPropiedad(ref _nombreContacto, value);
        }

        public string Descripcion
        {
            get => _descripcion;
            set
            {
                if (EstablecerPropiedad(ref _descripcion, value))
                {
                    NotificarCambioPropiedad(nameof(TieneDescripcion));
                }
            }
        }

        public bool TieneDescripcion => !string.IsNullOrWhiteSpace(Descripcion);

        public ImageSource? FotoPerfil
        {
            get => _fotoPerfil;
            set => EstablecerPropiedad(ref _fotoPerfil, value);
        }

        public DateTime FechaMatch
        {
            get => _fechaMatch;
            set
            {
                if (EstablecerPropiedad(ref _fechaMatch, value))
                {
                    NotificarCambioPropiedad(nameof(MensajeResumen));
                }
            }
        }

        public string MensajeResumen
        {
            get
            {
                if (FechaMatch == default)
                {
                    return "Te envió un match pendiente";
                }

                var fecha = FechaMatch;
                if (fecha.Kind == DateTimeKind.Unspecified)
                {
                    fecha = DateTime.SpecifyKind(fecha, DateTimeKind.Utc);
                }

                if (fecha.Kind == DateTimeKind.Utc)
                {
                    fecha = fecha.ToLocalTime();
                }

                return $"Te envió un match el {fecha:dd/MM/yyyy HH:mm}";
            }
        }
    }
}
