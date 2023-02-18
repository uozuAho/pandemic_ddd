using System;
using System.Collections.Generic;
using pandemic.GameData;

namespace pandemic.Values
{
    public abstract record PlayerCard
    {
        public static IEqualityComparer<PlayerCard> DefaultEqualityComparer = new PlayerCardEqualityComparer();
    }

    public record PlayerCityCard(CityData City) : PlayerCard;

    public record EpidemicCard : PlayerCard;

    public record GovernmentGrantCard : PlayerCard;

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

            return false;
        }

        public int GetHashCode(PlayerCard obj)
        {
            throw new NotImplementedException();
        }
    }
}
