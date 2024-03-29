using System;
using System.Collections.Generic;
using pandemic.Aggregates.Game;
using pandemic.Commands;

namespace pandemic.agents
{
    public interface IPandemicGameSolver
    {
        /// <summary>
        /// Given a game, return the commands to win the game, or empty if no win is possible.
        /// </summary>
        IEnumerable<IPlayerCommand> CommandsToWin(PandemicGame state, TimeSpan timeout);
    }
}
