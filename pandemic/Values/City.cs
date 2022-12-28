using System;
using System.Collections.Generic;

namespace pandemic.Values
{
    public record City(string Name)
    {
        public CubePile Cubes { get; init; } = CubePile.Empty;

        public bool HasResearchStation { get; init; }

        public bool IsSameStateAs(City other)
        {
            return Name == other.Name
                   && Cubes.HasSameCubesAs(other.Cubes)
                   && HasResearchStation == other.HasResearchStation;
        }

        public City AddCube(Colour colour)
        {
            return this with { Cubes = Cubes.AddCube(colour) };
        }

        public City RemoveCube(Colour colour)
        {
            return this with { Cubes = Cubes.RemoveCube(colour) };
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
