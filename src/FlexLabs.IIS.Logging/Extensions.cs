using System;
using System.Text.RegularExpressions;

namespace FlexLabs.IIS.Logging
{
    public static class Extensions
    {
        private const string GuidRegex = @"^\{?[0-9a-f]{8}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{4}-?[0-9a-f]{12}\}?$";
        private static Regex _guidRegex = new Regex(GuidRegex, RegexOptions.IgnoreCase);
        public static Guid ParseToGuid(this string value)
        {
            if (value == null) return Guid.Empty;
            if (!_guidRegex.IsMatch(value)) return Guid.Empty;
            return new Guid(value);
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (value == null) return null;
            if (value.Length > maxLength) return value.Substring(0, maxLength);
            return value;
        }

        public static string NullIfEmpty(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            return value;
        }
    }
}
