using pandemic.GameData;

namespace pandemic.Values
{
    public record PlayerCard { }

    public record PlayerCityCard(CityData City) : PlayerCard;

    public record EpidemicCard : PlayerCard;
}
