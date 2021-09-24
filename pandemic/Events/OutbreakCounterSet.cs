namespace pandemic.Events
{
    public record OutbreakCounterSet : IEvent
    {
        public int Value { get; set; }

        public OutbreakCounterSet(int value)
        {
            Value = value;
        }
    }
}
