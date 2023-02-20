using System;
using System.Collections.Generic;

namespace pandemic.Values
{
    public abstract record PlayerCard
    {
        public static IEqualityComparer<PlayerCard> DefaultEqualityComparer = new PlayerCardEqualityComparer();
    }

    public class PlayerCardEqualityComparer : IEqualityComparer<PlayerCard>
    {
        public bool Equals(PlayerCard? x, PlayerCard? y)
        {
            if (x == null || y == null) return false;

            if (x is EpidemicCard && y is EpidemicCard) return true;

            if (x is PlayerCityCard xCity && y is PlayerCityCard yCity)
            {
                return xCity.City.Name == yCity.City.Name;
            }

            // todo: special event cards not covered. Make failing test first

            return false;
        }

        public int GetHashCode(PlayerCard obj)
        {
            throw new NotImplementedException();
        }
    }
}
