namespace pandemic.Values
{
    public record PlayerCard { }

    public record PlayerCityCard(string City) : PlayerCard;

    public record EpidemicCard : PlayerCard;
}
