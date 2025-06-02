namespace pandemic.Events;

using Values;

internal record TreatedDisease(Role Role, string City, Colour Colour) : IEvent
{
    public override string ToString()
    {
        return $"{Role}: treated disease ({Colour}) in {City}";
    }
}
