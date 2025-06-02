namespace pandemic.agents;

using Aggregates.Game;
using Commands;

/// <summary>
/// A pandemic game-playing agent that can return a command to play
/// without needing to search/learn an entire game.
/// </summary>
public interface ILiveAgent
{
    IPlayerCommand NextCommand(PandemicGame game);
}
