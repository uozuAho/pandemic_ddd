using pandemic.Values;

namespace pandemic.Events
{
    public record CureDiscovered(Colour Colour) : IEvent;
}
