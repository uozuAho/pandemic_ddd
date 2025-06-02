namespace pandemic.Commands;

using Values;

public record AirliftCommand(Role Role, Role PlayerToMove, string City) : IPlayerCommand
{
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => true;

    public override string ToString()
    {
        return $"{Role}: airlift {PlayerToMove} to {City}";
    }
}
