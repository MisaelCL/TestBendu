using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ProyectoIMC.Converters
{
    /// <summary>
    /// Convierte un valor numérico double en true cuando es mayor que cero.
    /// </summary>
    public sealed class DoubleGreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double number)
            {
                return number > 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
