using System;
using System.Collections.Generic;
using System.Linq;

namespace pandemic.Values
{
    public record City(string Name)
    {
        public CubePile Cubes { get; init; } = CubePile.Empty;

        public bool HasResearchStation { get; init; }

        /// <summary>
        /// Returns the highest number of cubes of any one colour in this city
        /// </summary>
        public int MaxNumCubes()
        {
            var max = 0;
            if (Cubes.Black > max) max = Cubes.Black;
            if (max == 3) return max;
            if (Cubes.Blue > max) max = Cubes.Blue;
            if (max == 3) return max;
            if (Cubes.Red > max) max = Cubes.Red;
            if (max == 3) return max;
            if (Cubes.Yellow > max) max = Cubes.Yellow;
            return max;
        }

        public bool IsSameStateAs(City other)
        {
            return Name == other.Name
                   && Cubes.HasSameCubesAs(other.Cubes)
                   && HasResearchStation == other.HasResearchStation;
        }

        public City AddCube(Colour colour)
        {
            return AddCubes(colour, 1);
        }

        public City AddCubes(Colour colour, int numCubes)
        {
            return this with { Cubes = Cubes.AddCubes(colour, numCubes) };
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
