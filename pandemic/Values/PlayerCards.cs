﻿using System.Collections.Generic;
using System.Collections.Immutable;
using pandemic.GameData;

namespace pandemic.Values;

public record PlayerCityCard(CityData City) : PlayerCard;

public record EpidemicCard : PlayerCard;

public interface ISpecialEventCard { }

public record GovernmentGrantCard : PlayerCard, ISpecialEventCard;
public record EventForecastCard : PlayerCard, ISpecialEventCard;
public record AirliftCard : PlayerCard, ISpecialEventCard;
public record ResilientPopulationCard : PlayerCard, ISpecialEventCard;

public static class SpecialEventCards
{
    public static readonly ImmutableList<PlayerCard> All = new List<PlayerCard>
    {
        new GovernmentGrantCard(),
        new EventForecastCard(),
        new AirliftCard(),
        new ResilientPopulationCard()
    }.ToImmutableList();
}