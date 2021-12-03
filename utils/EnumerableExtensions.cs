namespace utils;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
    {
        var rng = new Random();
        return items.OrderBy(_ => rng.Next());
    }

    public static IEnumerable<T> Reversed<T>(this IEnumerable<T> items)
    {
        var temp = new List<T>();
        temp.AddRange(items);
        temp.Reverse();
        return temp;
    }
}
