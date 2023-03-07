using pandemic.Aggregates.Game;
using pandemic.Commands;

namespace pandemic.agents;

/// <summary>
/// A pandemic game-playing agent that can return a command to play
/// without needing to search/learn an entire game.
/// </summary>
public interface ILiveAgent
{
    IPlayerCommand NextCommand(PandemicGame game);
}
