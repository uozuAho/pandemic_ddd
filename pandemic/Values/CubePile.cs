using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace pandemic.Values;

public record CubePile
{
    private CubePile()
    {
        Counts = new Dictionary<Colour, int>
        {
            { Colour.Black, 0 },
            { Colour.Blue, 0 },
            { Colour.Red, 0 },
            { Colour.Yellow, 0 },
        }.ToImmutableDictionary();
    }

    public CubePile(IImmutableDictionary<Colour, int> cubes)
    {
        Counts = cubes;
    }

    public static readonly CubePile Empty = new();

    public bool HasSameCubesAs(CubePile other)
    {
        return Counts.SequenceEqual(other.Counts);
    }

    public CubePile AddCube(Colour colour)
    {
        return new CubePile(Counts.SetItem(colour, Counts[colour] + 1));
    }

    public CubePile AddCubes(Colour colour, int numCubes)
    {
        return new CubePile(Counts.SetItem(colour, Counts[colour] + numCubes));
    }

    public CubePile RemoveCube(Colour colour)
    {
        return RemoveCubes(colour, 1);
    }

    public CubePile RemoveCubes(Colour colour, int numCubes)
    {
        return new CubePile(Counts.SetItem(colour, Counts[colour] - numCubes));
    }

    public CubePile RemoveAll(Colour colour)
    {
        return new CubePile(Counts.SetItem(colour, 0));
    }

    public bool Any()
    {
        return Counts.Values.Any(v => v > 0);
    }

    public IImmutableDictionary<Colour, int> Counts { get; }

    public int NumberOf(Colour colour)
    {
        return Counts[colour];
    }

    public override string ToString()
    {
        return string.Join(" ", Counts.Select(c => $"{c.Key}: {c.Value}"));
    }
}
