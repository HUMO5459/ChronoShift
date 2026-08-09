using System;

namespace ChronoShift;

/// <summary>
/// Turns TimeManager's day counter into the labels the UI design asks for:
/// "1900-MART" for the date plate and "14:32" for the clock beside it.
/// </summary>
public static class GameCalendar
{
    private static readonly string[] MonthNames =
    {
        "YANVAR", "FEVRAL", "MART", "APREL", "MAY", "IYUN",
        "IYUL", "AVGUST", "SENTABR", "OKTABR", "NOYABR", "DEKABR",
    };

    /// <summary>Day counts of a common year; TimeManager's 365-day year has no leap day.</summary>
    private static readonly int[] MonthLengths = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

    /// <summary>Length of the calendar's year, so callers converting years to days agree with it.</summary>
    public const double DaysPerYear = 365.0;

    /// <summary>Zero-based month index for a 1-based day of the year.</summary>
    public static int MonthOfYear(int dayOfYear)
    {
        int remaining = Math.Clamp(dayOfYear, 1, 365);
        for (int month = 0; month < MonthLengths.Length; month++)
        {
            if (remaining <= MonthLengths[month])
            {
                return month;
            }

            remaining -= MonthLengths[month];
        }

        return MonthLengths.Length - 1;
    }

    /// <summary>Calendar year for a day counter that started at <paramref name="startYear"/>.</summary>
    public static int YearOf(double elapsedDays, int startYear) => startYear + (int)(elapsedDays / DaysPerYear);

    /// <summary>1-based day of the year for a day counter.</summary>
    public static int DayOfYear(double elapsedDays) => (int)(elapsedDays % DaysPerYear) + 1;

    /// <summary>Date plate, e.g. "1900-MART".</summary>
    public static string FormatDate(int year, int dayOfYear) => $"{year}-{MonthNames[MonthOfYear(dayOfYear)]}";

    /// <summary>Date plate straight from a day counter, e.g. "1900-MART".</summary>
    public static string FormatDate(double elapsedDays, int startYear) =>
        FormatDate(YearOf(elapsedDays, startYear), DayOfYear(elapsedDays));

    /// <summary>Clock, e.g. "14:32", read from the fractional part of the day.</summary>
    public static string FormatClock(double elapsedDays)
    {
        double fraction = elapsedDays - Math.Floor(elapsedDays);
        int totalMinutes = (int)(fraction * 24.0 * 60.0);
        return $"{totalMinutes / 60:00}:{totalMinutes % 60:00}";
    }
}
