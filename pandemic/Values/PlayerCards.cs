namespace pandemic.Values;

using System.Collections.Immutable;
using GameData;

public record PlayerCityCard(CityData City) : PlayerCard
{
    public override string ToString()
    {
        return $"{City.Name} ({City.Colour})";
    }
}

public record EpidemicCard : PlayerCard
{
    public override string ToString()
    {
        return "Epidemic";
    }
}

public interface ISpecialEventCard { }

public record GovernmentGrantCard : PlayerCard, ISpecialEventCard
{
    public override string ToString()
    {
        return "Government grant";
    }
}

public record EventForecastCard : PlayerCard, ISpecialEventCard
{
    public override string ToString()
    {
        return "Event forecast";
    }
}

public record AirliftCard : PlayerCard, ISpecialEventCard
{
    public override string ToString()
    {
        return "Airlift";
    }
}

public record ResilientPopulationCard : PlayerCard, ISpecialEventCard
{
    public override string ToString()
    {
        return "Resilient population";
    }
}

public record OneQuietNightCard : PlayerCard, ISpecialEventCard
{
    public override string ToString()
    {
        return "One quiet night";
    }
}

public static class SpecialEventCards
{
    public static readonly ImmutableList<PlayerCard> All =
    [
        new GovernmentGrantCard(),
        new EventForecastCard(),
        new AirliftCard(),
        new ResilientPopulationCard(),
        new OneQuietNightCard(),
    ];
}
