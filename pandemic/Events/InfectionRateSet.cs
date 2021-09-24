namespace pandemic.Events
{
    public record InfectionRateSet : IEvent
    {
        public int Rate { get; }

        public InfectionRateSet(int rate)
        {
            Rate = rate;
        }
    }
}
