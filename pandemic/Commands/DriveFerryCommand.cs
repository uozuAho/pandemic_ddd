namespace pandemic.Commands;

using Values;

public record DriveFerryCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role} drive/ferry to {City}";
    }
}
