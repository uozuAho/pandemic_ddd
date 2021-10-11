using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace pandemic.Values
{
    public record City(string Name)
    {
        public ImmutableDictionary<Colour, int> Cubes { get; init; } =
            Enum.GetValues<Colour>().ToImmutableDictionary(c => c, _ => 0);

        public bool HasResearchStation { get; init; }

        public bool IsSameStateAs(City other)
        {
            return Name == other.Name
                   && Cubes.SequenceEqual(other.Cubes)
                   && HasResearchStation == other.HasResearchStation;
        }

        public static IEqualityComparer<City> DefaultEqualityComparer = new CityComparer();
    }

    internal class CityComparer : IEqualityComparer<City>
    {
        public bool Equals(City? x, City? y)
        {
            if (x == null || y == null) return false;

            return x.IsSameStateAs(y);
        }

        public int GetHashCode(City obj)
        {
            throw new NotImplementedException();
        }
    }
}
