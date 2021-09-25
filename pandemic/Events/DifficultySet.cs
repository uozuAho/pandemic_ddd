using pandemic.Values;

namespace pandemic.Events
{
    public record DifficultySet(Difficulty Difficulty) : IEvent;
}
