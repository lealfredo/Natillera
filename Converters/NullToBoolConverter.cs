using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Natillera.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        // Si no es null → true
        // Si es null → false
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        // No lo usamos, pero debe existir
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
