namespace pandemic.Commands;

using Values;

/// <summary>
/// Move from one research station to another
/// </summary>
public record ShuttleFlightCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role} shuttle fly to {City}";
    }
}
