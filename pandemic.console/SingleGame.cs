using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Events;
using utils;

namespace pandemic.console;

internal class SingleGame
{
    public static (PandemicSpielGameState, IEnumerable<IEvent>) PlayRandomGame(
        PandemicGame game, GameStats stats)
    {
        var numActions = 0;
        var random = new Random();
        var state = new PandemicSpielGameState(game);
        var events = new List<IEvent>();

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
