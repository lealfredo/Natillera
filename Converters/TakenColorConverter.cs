using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Natillera.Converters
{
    public class TakenColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Colors.Red : Colors.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
