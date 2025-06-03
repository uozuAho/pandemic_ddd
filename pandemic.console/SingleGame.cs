namespace pandemic.console;

using System;
using System.Collections.Generic;
using agents;
using agents.GameEvaluators;
using Aggregates.Game;
using Commands;
using Events;
using Values;

internal class SingleGame
{
    public static void PlayGameAndPrintPlaythrough(ILiveAgent agent)
    {
        var gameOptions = new NewGameOptions
        {
            Difficulty = Difficulty.Introductory,
            Roles = [Role.Medic, Role.Dispatcher],
            CommandGenerator = new SensibleCommandGenerator(),
        };
        var (game, events) = PandemicGame.CreateNewGame(gameOptions);

        var (endState, events2) = PlayGame(game, agent);
        events.AddRange(events2);

        Console.WriteLine("events:");
        PrintEvents(events);

        Console.WriteLine();
        Console.WriteLine("state:");
        PrintState(endState);
    }

    private static (PandemicGame, IEnumerable<IEvent>) PlayGame(PandemicGame game, ILiveAgent agent)
    {
        var numActions = 0;
        var events = new List<IEvent>();

        for (; numActions < 1000 && !game.IsOver; numActions++)
        {
            if (numActions == 999)
            {
                throw new InvalidOperationException("didn't expect this many turns");
            }
            // var scores = CommandScores(game);
            var command = agent.NextCommand(game);
            game = game.Do(command, events);
        }

        return (game, events);
    }

    private static IEnumerable<(int, IPlayerCommand)> CommandScores(PandemicGame game)
    {
        foreach (var command in game.LegalCommands())
        {
            var (nextState, _) = game.Do(command);
            yield return (HandCraftedGameEvaluator.Score(nextState), command);
        }
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
