namespace Zlepper.RimWorld.ExtremeTraits;

internal static class Extensions
{
    internal static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }
}