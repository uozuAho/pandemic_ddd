namespace pandemic.Events
{
    // todo: change city to player card
    public record PlayerCardDiscarded(string Card) : IEvent;
}
