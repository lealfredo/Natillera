using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Natillera.Converters
{
    public class SelectedItemBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Color.FromArgb("#D0E8FF") : Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
