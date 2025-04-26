namespace ImageLab.Extension;

public static class IEnumerableExtension
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) => enumerable.Where(x => x is not null)!.Select(x => x!);
}