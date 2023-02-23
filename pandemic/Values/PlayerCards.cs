using System.Collections.Generic;
using System.Collections.Immutable;
using pandemic.GameData;

namespace pandemic.Values;

public record PlayerCityCard(CityData City) : PlayerCard;

public record EpidemicCard : PlayerCard;

public interface ISpecialEventCard { }

public record GovernmentGrantCard : PlayerCard, ISpecialEventCard;

public static class SpecialEventCards
{
    public static readonly ImmutableList<PlayerCard> All = new List<PlayerCard>
    {
        new GovernmentGrantCard()
    }.ToImmutableList();
}
