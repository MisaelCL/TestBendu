using System.Globalization;

namespace ProyectoIMC.Converters
{
    /// <summary>
    /// Clase base para convertidores que verifican si un valor double es mayor que cero.
    /// </summary>
    public class DoubleGreaterThanZeroConverterBase
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue > 0;
            }
            return false;
        }
    }
}