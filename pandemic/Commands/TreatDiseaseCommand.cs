using pandemic.Values;

namespace pandemic.Commands;

public record TreatDiseaseCommand(Role Role, string City, Colour Colour) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role}: treat {Colour} in {City}";
    }
}
