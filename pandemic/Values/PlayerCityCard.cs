namespace pandemic.Values
{
    // todo: remove city
    public record PlayerCard(string City) { }

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
