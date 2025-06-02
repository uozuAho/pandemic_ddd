namespace pandemic.Commands;

using Values;

public record ShareKnowledgeTakeFromResearcherCommand(Role Role, string City) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
