using pandemic.Values;

namespace pandemic.Commands;

public record DirectFlightCommand(Role Role, string City) : PlayerCommand
{
    public override string ToString()
    {
        return $"direct fly to {City}";
    }
}
