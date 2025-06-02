namespace pandemic.Events;

using Values;

public record PlayerCardPickedUp(Role Role, PlayerCard Card) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: picked up {Card}";
    }
}
