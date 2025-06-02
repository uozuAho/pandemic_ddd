namespace pandemic.Values;

using System;
using System.Collections.Generic;

public abstract record PlayerCard
{
    public static readonly IEqualityComparer<PlayerCard> DefaultEqualityComparer =
        new PlayerCardEqualityComparer();
}

public class PlayerCardEqualityComparer : IEqualityComparer<PlayerCard>
{
    public bool Equals(PlayerCard? x, PlayerCard? y)
    {
        if (x == null || y == null)
        {
            return false;
        }

        if (x is PlayerCityCard xCity && y is PlayerCityCard yCity)
        {
            return xCity.City.Name == yCity.City.Name;
        }

        if (x.GetType() == y.GetType())
        {
            return true;
        }

        return false;
    }

    public int GetHashCode(PlayerCard obj)
    {
        // Don't use this type in a set/dictionary
#pragma warning disable CA1065
        throw new NotImplementedException();
#pragma warning restore CA1065
    }
}
