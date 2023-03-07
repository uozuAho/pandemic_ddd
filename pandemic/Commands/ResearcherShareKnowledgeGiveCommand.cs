using pandemic.Values;

namespace pandemic.Commands;

public record ResearcherShareKnowledgeGiveCommand(Role PlayerToGiveTo, string City) : IPlayerCommand
{
    public Role Role => Role.Researcher;
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;
}
