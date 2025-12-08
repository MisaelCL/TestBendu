using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ProyectoIMC.Converters
{
    /// <summary>
    /// Convierte un valor numérico double en true cuando es mayor que cero.
    /// </summary>
    public sealed class DoubleGreaterThanZeroConverter : DoubleGreaterThanZeroConverterBase, IValueConverter
    {
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
