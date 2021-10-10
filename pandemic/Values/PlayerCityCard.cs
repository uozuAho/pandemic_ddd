namespace pandemic.Values
{
    public abstract record PlayerCard { }

    public record PlayerCityCard(string City) : PlayerCard;

    public record EpidemicCard : PlayerCard;
}
