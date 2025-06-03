namespace pandemic.Events;

using Values;

internal sealed record MedicTreatedDisease(string City, Colour Colour) : IEvent
{
    public override string ToString()
    {
        return $"Medic: treated disease ({Colour}) in {City}";
    }
}
