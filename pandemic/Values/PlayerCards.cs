using pandemic.GameData;

namespace pandemic.Values;

public record PlayerCityCard(CityData City) : PlayerCard;

public record EpidemicCard : PlayerCard;

public interface ISpecialEventCard { }

public record GovernmentGrantCard : PlayerCard, ISpecialEventCard;
