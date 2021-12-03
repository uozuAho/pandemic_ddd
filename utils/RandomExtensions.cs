namespace utils;

public static class RandomExtensions
{
    public static T Choice<T>(this Random random, IEnumerable<T> items)
    {
        var itemList = items.ToList();
        return itemList[random.Next(itemList.Count)];
    }
}
