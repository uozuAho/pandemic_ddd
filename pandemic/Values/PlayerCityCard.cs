namespace pandemic.Values
{
    public abstract record PlayerCard { }

    public record PlayerCityCard(string City) : PlayerCard;

    // todo: remove city
    public record EpidemicCard(string City) : PlayerCard;
}
