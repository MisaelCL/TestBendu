using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace C_C_Final.ViewModel
{
    /// <summary>
    /// Proporciona utilidades comunes para la notificación de cambios en propiedades.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Asigna un valor a la propiedad indicada y notifica si hubo un cambio real.
        /// </summary>
        protected bool EstablecerPropiedad<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            NotificarCambioPropiedad(propertyName);
            return true;
        }

        /// <summary>
        /// Dispara el evento de cambio de propiedad para el nombre indicado.
        /// </summary>
        protected void NotificarCambioPropiedad([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
