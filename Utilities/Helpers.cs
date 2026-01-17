namespace Natillera.Utilities
{
    public static class Helpers
    {
        public static DateTime GetNextFriday(DateTime fromDate)
        {
            int days = DayOfWeek.Friday - fromDate.DayOfWeek;
            if (days < 0)
                days += 7;

            return fromDate.AddDays(days);
        }
    }

    public static class CustomFileTypes
    {
        public static FilePickerFileType Json => new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
            { DevicePlatform.Android, new[] { "application/json" } },
            { DevicePlatform.iOS, new[] { "public.json" } },
            { DevicePlatform.WinUI, new[] { ".json" } },
            { DevicePlatform.MacCatalyst, new[] { "public.json" } }
            });
    }
}
