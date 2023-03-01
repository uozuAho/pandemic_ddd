using pandemic.Values;

namespace pandemic.Commands;

public interface IPlayerCommand : ICommand
{
    /// <summary>
    /// The role 'issuing' the command
    /// </summary>
    Role Role { get; }

    bool ConsumesAction { get; }
}
