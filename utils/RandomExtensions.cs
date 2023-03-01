namespace utils;

public static class RandomExtensions
{
    public static T Choice<T>(this Random random, IEnumerable<T> items)
    {
        var itemList = items.ToList();
        return itemList[random.Next(itemList.Count)];
    }

    /// <summary>
    /// Choose a random set of size N from the given options
    /// </summary>
    public static IEnumerable<T> Choice<T>(this Random random, int number, IEnumerable<T> options)
    {
        return options.OrderBy(_ => random.Next()).Take(number);
    }
}
