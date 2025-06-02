namespace pandemic.Events;

using Values;

public record DifficultySet(Difficulty Difficulty) : IEvent;
