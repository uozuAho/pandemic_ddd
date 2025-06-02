namespace pandemic.Events;

using Values;

internal record MedicTreatedDisease(string City, Colour Colour) : IEvent
{
    public override string ToString()
    {
        return $"Medic: treated disease ({Colour}) in {City}";
    }
}
