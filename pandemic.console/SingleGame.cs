using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;
using utils;

namespace pandemic.console;

internal class SingleGame
{
    public static (PandemicSpielGameState, IEnumerable<IEvent>) PlayRandomGame(
        NewGameOptions options, GameStats stats)
    {
        var random = new Random();
        var (game, events) = PandemicGame.CreateNewGame(options);
        var state = new PandemicSpielGameState(game);
        var numActions = 0;

        for (; numActions < 1000 && !state.IsTerminal; numActions++)
        {
            if (numActions == 999) throw new InvalidOperationException("didn't expect this many turns");

            var legalActions = state.LegalActions().ToList();
            if (!legalActions.Any())
            {
                throw new InvalidOperationException("oh no!");
            }

            stats.AddLegalActionCount(legalActions.Count);

            var action = random.Choice(state.LegalActions());
            events.AddRange(state.ApplyAction(action));
        }

        stats.AddNumActionsInGame(numActions);

        return (state, events);
    }
}
