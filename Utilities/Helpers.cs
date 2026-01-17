using System;
using System.Collections.Generic;
using System.Text;

namespace Natillera.Utilities
{
    public class Helpers
    {
        public static DateTime GetNextFriday(DateTime fromDate)
        {
            int days = DayOfWeek.Friday - fromDate.DayOfWeek;
            if (days < 0)
                days += 7;

            return fromDate.AddDays(days);
        }
    }
}
