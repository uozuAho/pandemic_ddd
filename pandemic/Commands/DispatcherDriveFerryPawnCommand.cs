namespace pandemic.Commands;

using Values;

public record DispatcherDriveFerryPawnCommand(Role PlayerToMove, string City) : IPlayerCommand
{
    public Role Role => Role.Dispatcher;

    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role}: drive {PlayerToMove} to {City}";
    }
}
