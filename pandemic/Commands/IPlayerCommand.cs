namespace pandemic.Commands;

using Values;

public interface IPlayerCommand
{
    /// <summary>
    /// The role 'issuing' the command
    /// </summary>
    Role Role { get; }

    bool ConsumesAction { get; }

    bool IsSpecialEvent { get; }
}
