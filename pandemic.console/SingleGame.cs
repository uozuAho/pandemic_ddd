using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.console;

class SingleGame
{
    public static void PlaySingleRandomGameVerbose()
    {
        var options = new NewGameOptions
        {
            Difficulty = Difficulty.Introductory,
            Roles = new[] {Role.Medic, Role.Scientist}
        };

        var (state, events) = PlayRandomGame(options, new GameStats());

        PrintEventsAndState(events, state);
    }

    private static void PrintEventsAndState(IEnumerable<IEvent> events, PandemicSpielGameState state)
    {
        Console.WriteLine("Game over! Events:");
        Console.WriteLine();
        foreach (var @event in events)
        {
            Console.WriteLine(@event);
        }

        Console.WriteLine();
        Console.WriteLine("Final state:");
        Console.WriteLine();
        Console.WriteLine(state);
    }

    private static (PandemicSpielGameState, IEnumerable<IEvent>) PlayRandomGame(
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

            var action = RandomChoice(state.LegalActions(), random);
            events.AddRange(state.ApplyAction(action));
        }

        stats.AddNumActionsInGame(numActions);

        return (state, events);
    }

    private static T RandomChoice<T>(IEnumerable<T> items, Random random)
    {
        var itemList = items.ToList();
        if (!itemList.Any()) throw new InvalidOperationException("no items to choose from!");

        var idx = random.Next(0, itemList.Count);
        return itemList[idx];
    }
}
