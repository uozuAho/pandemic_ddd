using pandemic.Values;

namespace pandemic.Commands;

public interface IPlayerCommand : ICommand
{
    Role Role { get; }
}
