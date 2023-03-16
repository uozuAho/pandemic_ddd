using pandemic.Values;

namespace pandemic.Commands;

public record DispatcherDirectFlyPawnCommand(Role PlayerToMove, string City) : IPlayerCommand
{
    public Role Role => Role.Dispatcher;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role}: direct fly {PlayerToMove} to {City}";
    }
}
