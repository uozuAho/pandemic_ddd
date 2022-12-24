using System.Collections;

namespace utils.test;

public class EnumerableExtensionsTests
{
    [TestCaseSource(typeof(TestData), nameof(TestData.TestCases))]
    public IEnumerable<IEnumerable<int>> SplitEvenlyInto(IEnumerable<int> items, int numGroups)
    {
        return items.SplitEvenlyInto(numGroups);
    }
}

public class TestData
{
    public static IEnumerable TestCases
    {
        get
        {
            yield return new TestCaseData(new[] { 1, 2, 3 }, 3).Returns(new[]
            {
                new[] { 1 },
                new[] { 2 },
                new[] { 3 }
            });
            yield return new TestCaseData(new[] { 1, 2, 3 }, 2).Returns(new[]
            {
                new[] { 1, 2 },
                new[] { 3 },
            });
            yield return new TestCaseData(new[] { 1, 2, 3 }, 1).Returns(new[]
            {
                new[] { 1, 2, 3 },
            });
            yield return new TestCaseData(new[] { 1, 2, 3, 4 }, 2).Returns(new[]
            {
                new[] { 1, 2 },
                new[] { 3, 4 },
            });
            yield return new TestCaseData(new[] { 1, 2, 3, 4 }, 3).Returns(new[]
            {
                new[] { 1, 2 },
                new[] { 3 },
                new[] { 4 },
            });
            yield return new TestCaseData(new[] { 1, 2, 3, 4, 5 }, 2).Returns(new[]
            {
                new[] { 1, 2, 3 },
                new[] { 4, 5 },
            });
            yield return new TestCaseData(new[] { 1, 2, 3, 4, 5 }, 3).Returns(new[]
            {
                new[] { 1, 2 },
                new[] { 3, 4 },
                new[] { 5 },
            });
            yield return new TestCaseData(new[] { 1, 2, 3, 4, 5 }, 4).Returns(new[]
            {
                new[] { 1, 2 },
                new[] { 3 },
                new[] { 4 },
                new[] { 5 },
            });
        }
    }
}
