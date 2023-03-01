using pandemic.Values;

namespace pandemic.Commands;

public record ShareKnowledgeTakeCommand(Role Role, string City, Role TakeFromRole) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
