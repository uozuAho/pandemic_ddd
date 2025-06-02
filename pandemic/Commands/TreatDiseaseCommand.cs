namespace pandemic.Commands;

using Values;

public record TreatDiseaseCommand(Role Role, string City, Colour Colour) : IPlayerCommand
{
    public bool ConsumesAction => true;
    public bool IsSpecialEvent => false;

    public override string ToString()
    {
        return $"{Role}: treat {Colour} in {City}";
    }
}
