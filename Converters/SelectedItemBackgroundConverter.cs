using System.Globalization;

namespace Natillera.Converters
{
    public class SelectedItemBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selected = value is true;
            var resources = Application.Current?.Resources;

            if (selected)
            {
                if (resources?.TryGetValue("Secondary", out var secondary) == true && secondary is Color secondaryColor)
                    return secondaryColor;

                return Color.FromArgb("#E8F5E9");
            }

            if (resources?.TryGetValue("Surface", out var surface) == true && surface is Color surfaceColor)
                return surfaceColor;

            return Colors.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
