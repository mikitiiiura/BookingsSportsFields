using System.Globalization;

namespace BookingsSportsFields.Core;

/// <summary>
/// Єдина логіка для UTC-календарних днів і парсингу дат з маршрутів/клієнтів (ISO Z, DateOnly),
/// щоб PostgreSQL timestamptz і різні клієнти не давали зсувів.
/// </summary>
public static class UtcDateTimeHelper
{
    /// <summary>Початок календарного дня в UTC (00:00 UTC для миттєвості в UTC).</summary>
    public static DateTime UtcStartOfCalendarDay(DateTime date)
    {
        var utc = date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
        return new DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>Верхня межа дня [dayStart, nextDay) для запитів.</summary>
    public static DateTime UtcExclusiveEndOfCalendarDay(DateTime date) =>
        UtcStartOfCalendarDay(date).AddDays(1);

    public static DateTime ToUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
    };

    /// <summary>
    /// Маршрутний сегмент або query: "2026-04-07", "2026-04-07T00:00:00.000Z", URL-encoded ISO.
    /// </summary>
    public static bool TryParseIsoOrDateOnly(string? value, out DateTime utcDayStart)
    {
        utcDayStart = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = System.Net.WebUtility.UrlDecode(value.Trim());

        if (value.Length == 10 && value[4] == '-' && value[7] == '-' &&
            !value.Contains('T', StringComparison.Ordinal))
        {
            if (DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
            {
                utcDayStart = new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Utc);
                return true;
            }
            return false;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
        {
            var u = dto.UtcDateTime;
            utcDayStart = new DateTime(u.Year, u.Month, u.Day, 0, 0, 0, DateTimeKind.Utc);
            return true;
        }

        return false;
    }
}
