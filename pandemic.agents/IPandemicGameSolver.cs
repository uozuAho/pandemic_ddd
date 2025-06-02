namespace pandemic.agents;

using System;
using System.Collections.Generic;
using Aggregates.Game;
using Commands;

public interface IPandemicGameSolver
{
    /// <summary>
    /// Given a game, return the commands to win the game, or empty if no win is possible.
    /// </summary>
    IEnumerable<IPlayerCommand> CommandsToWin(PandemicGame state, TimeSpan timeout);
}
