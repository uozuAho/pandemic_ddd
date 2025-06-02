namespace pandemic.Commands;

using Values;

public record DontUseSpecialEventCommand : IPlayerCommand
{
    /// <summary>
    /// NOTE Role is redundant for this command. Keeping it for perf, rather than creating another command type.
    /// </summary>
    public Role Role => Role.Medic;
    public bool ConsumesAction => false;
    public bool IsSpecialEvent => false;
}
