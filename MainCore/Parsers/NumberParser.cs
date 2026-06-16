using System.Net;

namespace MainCore.Parsers
{
    public static class NumberParsers
    {
        /// <summary>
        /// Parses a time duration string into a TimeSpan.
        /// Handles both standard Travian format (HH:MM:SS) and TTWars format (HH:MM:SS (+MMM ms)).
        /// TTWars servers use millisecond precision for timers.
        /// </summary>
        public static TimeSpan ToDuration(this string value)
        {
            if (string.IsNullOrEmpty(value)) return TimeSpan.Zero;

            // TTWars format: 00:00:02 (+332 ms), milliseconds matter
            int ms = 0;
            if (value.Contains("(+"))
            {
                var parts = value.Split('(');
                if (parts.Length > 1)
                {
                    // Extract milliseconds from (+XXX ms) format
                    var msPart = parts[1].Replace("ms)", "").Replace(")", "").Trim();
                    ms = msPart.ParseInt();
                    if (ms == -1) ms = 0;
                }
                value = parts[0];
            }

            // Clean up the value string
            value = value.Trim();

            // h:m:s format
            var arr = value.Split(':');
            if (arr.Length < 3) return TimeSpan.Zero;

            var h = arr[0].ParseInt();
            var m = arr[1].ParseInt();
            var s = arr[2].ParseInt();

            // Handle invalid values
            if (h == -1) h = 0;
            if (m == -1) m = 0;
            if (s == -1) s = 0;

            return new TimeSpan(0, h, m, s, ms);
        }

        private static string Normalized(this string value)
        {
            var valueStrDecoded = WebUtility.HtmlDecode(value);
            if (string.IsNullOrEmpty(valueStrDecoded)) return "";

            var valueStr = new string(valueStrDecoded.Where(c => char.IsDigit(c) || c == '-' || c == '−').ToArray());
            valueStr = valueStr.Replace('−', '-');

            if (string.IsNullOrEmpty(valueStr)) return "";
            return valueStr;
        }

        public static int ParseInt(this string value)
        {
            var normValue = value.Normalized();
            if (string.IsNullOrEmpty(normValue)) return -1;
            // Guard against standalone minus sign
            if (normValue == "-" || normValue == "+") return -1;
            if (int.TryParse(normValue, out var result)) return result;
            return -1;
        }

        public static long ParseLong(this string value)
        {
            var normValue = value.Normalized();
            if (string.IsNullOrEmpty(normValue)) return -1;
            // Guard against standalone minus sign
            if (normValue == "-" || normValue == "+") return -1;
            if (long.TryParse(normValue, out var result)) return result;
            return -1;
        }
    }
}