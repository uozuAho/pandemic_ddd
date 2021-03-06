using pandemic.Values;

namespace pandemic.Commands;

public record DriveFerryCommand(Role Role, string City) : PlayerCommand
{
    public override string ToString()
    {
        return $"go to {City}";
    }
}
