using System;
using Numeira.MaterialOptimizer.API.Internal;

namespace Numeira.MaterialOptimizer.Modules;

internal static class Ext
{
    public static bool ContainsAny(this string[] values, string value, StringComparison comparison = StringComparison.Ordinal)
        => ((ReadOnlySpan<string>)values.AsSpan()).ContainsAny(value, comparison);

    public static bool ContainsAny(this ReadOnlySpan<string> values, string value, StringComparison comparison = StringComparison.Ordinal)
    {
        foreach(var x in values)
        {
            if (x == null)
                continue;
            
            if (x.Contains(value, comparison))
            {
                return true;
            }
        }
        return false;
    }
}