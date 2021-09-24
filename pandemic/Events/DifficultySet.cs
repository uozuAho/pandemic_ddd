namespace pandemic.Events
{
    public record DifficultySet : IEvent
    {
        public Difficulty Difficulty { get; }

        public DifficultySet(Difficulty difficulty)
        {
            Difficulty = difficulty;
        }
    }
}
