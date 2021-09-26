using System;
using System.Collections.Immutable;

namespace pandemic.Values
{
    public record City(string Name)
    {
        public ImmutableDictionary<Colour, int> Cubes { get; init; } =
            Enum.GetValues<Colour>().ToImmutableDictionary(c => c, _ => 0);
    }
}
