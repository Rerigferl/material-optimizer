using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace Numeira.MaterialOptimizer.API.Internal;

internal static class UnsafeUtilities
{
    public static void Deconstruct<T>(this List<T> list, out T[] buffer, out int count)
    {
        var tuple = Unsafe.As<Tuple<T[], int>>(list);
        (buffer, count) = (tuple.Item1, tuple.Item2);
    }

    public static Span<T> AsSpan<T>(this List<T> list)
    {
        var (buffer, count) = list;
        return buffer.AsSpan(0, count);
    }

    public static int GetVersion<T>(this List<T> list)
    {
        return Unsafe.As<Tuple<T[], int, int>>(list).Item3;
    }
}