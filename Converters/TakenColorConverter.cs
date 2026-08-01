using System.Globalization;

namespace Natillera.Converters
{
    public class TakenColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var taken = value is true;
            var resources = Application.Current?.Resources;

            if (resources != null)
            {
                if (taken && resources.TryGetValue("Danger", out var danger) && danger is Color dangerColor)
                    return dangerColor;

                if (!taken && resources.TryGetValue("Success", out var success) && success is Color successColor)
                    return successColor;
            }

            return taken ? Color.FromArgb("#C62828") : Color.FromArgb("#2E7D32");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
