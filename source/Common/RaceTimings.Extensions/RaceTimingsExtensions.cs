using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace RaceTimings.Extensions;

public static class RaceTimingsExtensions
{
    /// <summary>
    /// Check if the number is between a numerical high and low,
    /// inclusive of the provide min/max values. 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    
    [Pure]
    public static bool IsBetween<T1, T2, T3>(this T1 value, T2 min, T3 max)
        where T1 : struct, IComparable
        where T2 : struct, IComparable
        where T3 : struct, IComparable
    {
        double valueAsDouble = Convert.ToDouble(value);
        double minAsDouble = Convert.ToDouble(min);
        double maxAsDouble = Convert.ToDouble(max);

        return valueAsDouble >= minAsDouble && valueAsDouble <= maxAsDouble;
    }
    
    [Pure]
    public static bool IsNotBetween<T1, T2, T3>(this T1 value, T2 min, T3 max)
        where T1 : struct, IComparable
        where T2 : struct, IComparable
        where T3 : struct, IComparable
    {
        double valueAsDouble = Convert.ToDouble(value);
        double minAsDouble = Convert.ToDouble(min);
        double maxAsDouble = Convert.ToDouble(max);

        return !(valueAsDouble >= minAsDouble && valueAsDouble <= maxAsDouble);
    }
   
    [Pure]
    public static bool IsIn<T>(this T item, params T[] values)
    {
        return values.Contains(item);
    }
    [Pure]
    public static bool IsNotIn<T>(this T item, params T[] values)
    {
        return !values.Contains(item);
    }
    
    public static void Match(
        this bool result,
        Action onTrue,
        Action onFalse)
    {
        if (result)
            onTrue();
        else
            onFalse();
    }
    
    /// <summary>
    /// Check if the string matches a regex pattern.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    [Pure]
    public static bool MatchesRegexPattern(this string value, [StringSyntax(StringSyntaxAttribute.Regex)] string pattern) => new Regex(pattern).IsMatch(value);
}