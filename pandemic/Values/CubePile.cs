using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace pandemic.Values;

public record CubePile
{
    private readonly IImmutableDictionary<Colour, int> _cubes;

    private CubePile()
    {
        _cubes = new Dictionary<Colour, int>
        {
            { Colour.Black, 0 },
            { Colour.Blue, 0 },
            { Colour.Red, 0 },
            { Colour.Yellow, 0 },
        }.ToImmutableDictionary();
    }

    public CubePile(IImmutableDictionary<Colour, int> cubes)
    {
        _cubes = cubes;
    }

    public static readonly CubePile Empty = new();

    public bool HasSameCubesAs(CubePile other)
    {
        return _cubes.SequenceEqual(other._cubes);
    }

    public CubePile AddCube(Colour colour)
    {
        return new CubePile(_cubes.SetItem(colour, _cubes[colour] + 1));
    }

    public CubePile RemoveCube(Colour colour)
    {
        return new CubePile(_cubes.SetItem(colour, _cubes[colour] - 1));
    }

    public bool Any()
    {
        return _cubes.Values.Any(v => v > 0);
    }

    public IImmutableDictionary<Colour, int> Counts()
    {
        return _cubes;
    }

    public int NumberOf(Colour colour)
    {
        return _cubes[colour];
    }
}
