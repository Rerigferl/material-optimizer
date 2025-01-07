using System.Runtime.CompilerServices;

namespace Numeira.MaterialOptimizer;

internal static class Utils
{
    public static int GetVersion<T>(this List<T> list)
    {
        return Unsafe.As<Tuple<T[], int, int>>(list).Item3;
    }
}