using System;
using System.Globalization;

namespace HospitalProject.Helper
{
    public static class TimeSlotHelper
    {
        private static readonly string[] TimeFormats =
        {
            "hh:mmtt", // 06:00PM
            "hh.mmtt"  // 06.00PM
        };

        public static bool IsNowWithinSlot(string timeSlot)
        {
            var parts = timeSlot.Split('-');

            var start = ParseTime(parts[0]);
            var end = ParseTime(parts[1]);

            var now = DateTime.Now.TimeOfDay;

            return now >= start && now <= end;
        }


        // ***************************************
        // No show status for time finish for booking
        // ***************************************
        public static bool IsSlotOver(string timeSlot)
        {
            var parts = timeSlot.Split('-');
            var end = ParseTime(parts[1]);

            return DateTime.Now.TimeOfDay > end;
        }

        private static TimeSpan ParseTime(string time)
        {
            if (DateTime.TryParseExact(
                time.Trim(),
                TimeFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsed))
            {
                return parsed.TimeOfDay;
            }

            throw new Exception($"Invalid time slot format: {time}");
        }
    }
}

