namespace pandemic.Events
{
    public record InfectionCardDrawn(string City) : IEvent;
}
