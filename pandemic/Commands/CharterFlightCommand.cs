using pandemic.Values;

namespace pandemic.Commands;

public record CharterFlightCommand(Role Role, string City) : PlayerCommand
{
    public override string ToString()
    {
        return $"charter flight to {City}";
    }
}
