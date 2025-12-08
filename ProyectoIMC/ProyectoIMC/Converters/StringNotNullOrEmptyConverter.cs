using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace ProyectoIMC.Converters
{
    /// <summary>
    /// Devuelve true si la cadena no es nula ni está vacía.
    /// </summary>
    public sealed class StringNotNullOrEmptyConverter : IValueConverter
    {
        // Revisa el texto y devuelve true sólo si hay algo escrito.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is string text && !string.IsNullOrWhiteSpace(text);
        }

        // No hay conversión inversa; las vistas sólo leen este valor.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
