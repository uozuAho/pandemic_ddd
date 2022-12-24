using Shouldly;

namespace utils.test;

public class EnumerableExtensionsTests
{
    [Test]
    public void SplitEvenlyInto()
    {
        new[] { 1, 2, 3 }.SplitEvenlyInto(3).ShouldBe(new[]
        {
            new[] { 1 },
            new[] { 2 },
            new[] { 3 }
        });
    }

    [Test]
    public void SplitEvenlyInto2()
    {
        new[] { 1, 2, 3, 4 }.SplitEvenlyInto(2).ShouldBe(new[]
        {
            new[] { 1, 2 },
            new[] { 3, 4 },
        });
    }
}
