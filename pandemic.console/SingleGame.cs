using System;
using System.Collections.Generic;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.Events;
using pandemic.Values;
using utils;

namespace pandemic.console;

internal class SingleGame
{
    public static void PlayGameAndPrintPlaythrough()
    {
        var gameOptions = new NewGameOptions
        {
            Difficulty = Difficulty.Normal,
            Roles = new[] { Role.Medic, Role.Scientist }
        };
        var (game, events) = PandemicGame.CreateNewGame(gameOptions);

        var (endState, events2) = SingleGame.PlayRandomGame(game);
        events.AddRange(events2);

        Console.WriteLine("events:");
        PrintEvents(events);

        Console.WriteLine();
        Console.WriteLine("state:");
        PrintState(endState);
    }

    public static (PandemicGame, IEnumerable<IEvent>) PlayRandomGame(PandemicGame game)
    {
        var numActions = 0;
        var random = new Random();
        var commandGenerator = new PlayerCommandGenerator();
        var events = new List<IEvent>();

        for (; numActions < 1000 && !game.IsOver; numActions++)
        {
            if (numActions == 999) throw new InvalidOperationException("didn't expect this many turns");
            var action = random.Choice(commandGenerator.LegalCommands(game));
            var (updatedGame, newEvents) = game.Do(action);
            game = updatedGame;
            events.AddRange(newEvents);
        }

        return (game, events);
    }

    private static void PrintState(PandemicGame state)
    {
        Console.WriteLine(state);
    }

    private static void PrintEvents(IEnumerable<IEvent> events)
    {
        foreach (var @event in events)
        {
            Console.WriteLine(@event);
        }
    }
}
