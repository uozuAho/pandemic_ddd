using pandemic.Values;

namespace pandemic.Commands;

public record DriveFerryCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"go to {City}";
    }
}
