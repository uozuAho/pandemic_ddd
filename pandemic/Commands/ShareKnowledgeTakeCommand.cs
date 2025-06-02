namespace pandemic.Commands;

using Values;

public record ShareKnowledgeTakeCommand(Role Role, string City, Role TakeFromRole) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
