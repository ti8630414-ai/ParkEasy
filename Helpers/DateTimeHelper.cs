using System;

namespace ParkEasy.Web.Helpers
{
    /// <summary>
    /// Central helper to normalize DateTime values to UTC before sending to PostgreSQL
    /// timestamptz columns (Npgsql only accepts Kind=Utc).
    /// </summary>
    public static class DateTimeHelper
    {
        public static DateTime ToUtc(DateTime dt)
        {
            return dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                // Unspecified (e.g. from HTML date/time inputs, query strings, model binding):
                // treat wall-clock value as UTC.
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };
        }

        public static DateTime ToUtc(DateTime? dt, DateTime? fallback = null)
        {
            if (dt.HasValue)
                return ToUtc(dt.Value);
            if (fallback.HasValue)
                return ToUtc(fallback.Value);
            return DateTime.UtcNow;
        }

        /// <summary>Combine a date + TimeSpan (both may be Unspecified/Local) into a UTC DateTime.</summary>
        public static DateTime CombineToUtc(DateTime date, TimeSpan time)
        {
            return ToUtc(date.Date.Add(time));
        }
    }
}
