using pandemic.Values;

namespace pandemic.Commands;

public record ShareKnowledgeTakeFromResearcherCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
