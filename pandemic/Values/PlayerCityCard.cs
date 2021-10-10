namespace pandemic.Values
{
    // todo: remove city
    public abstract record PlayerCard(string City) { }

    public record PlayerCityCard : PlayerCard
    {
        public PlayerCityCard(string City) : base(City)
        {
        }
    }

    // todo: remove city
    public record EpidemicCard : PlayerCard
    {
        public EpidemicCard(string City) : base(City)
        {
        }
    }
}
