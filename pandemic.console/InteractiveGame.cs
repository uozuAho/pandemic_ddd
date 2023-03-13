using System;
using System.Linq;
using pandemic.agents;
using pandemic.Aggregates.Game;

namespace pandemic.console;

public class InteractiveGame
{
    public static void Play(PandemicGame game)
    {
        Console.WriteLine("Running the game!");
        Help(game);

        while (!game.IsOver)
        {
            var command = StrToCommand(Console.ReadLine()!);
            if (command is QuitCommand) break;

            game = Execute(game, command);
        }
    }

    private static ICommand StrToCommand(string input)
    {
        return input switch
        {
            "h" => new HelpCommand(),
            "q" => new QuitCommand(),
            "s" => new StatusCommand(),
            "c" => new AvailableCommandsCommand(),
            _ => TryGetGameCommand(input)
        };
    }

    private static ICommand TryGetGameCommand(string input)
    {
        if (int.TryParse(input, out var number))
        {
            return new GameCommand(number);
        }

        return new BadInputCommand();
    }

    private static PandemicGame Execute(PandemicGame game, ICommand command)
    {
        return command switch
        {
            HelpCommand => Help(game),
            StatusCommand => Status(game),
            BadInputCommand => BadInput(game),
            AvailableCommandsCommand => AvailableCommands(game),
            GameCommand c => DoCommand(game, c.CommandNumber),
            _ => throw new NotImplementedException()
        };
    }

    private static PandemicGame DoCommand(PandemicGame game, int commandNumber)
    {
        var commands = game.LegalCommands().ToList();

        (game, var events) = game.Do(commands[commandNumber]);

        foreach (var @event in events)
        {
            Console.WriteLine(@event);
        }

        return game;
    }

    private static PandemicGame AvailableCommands(PandemicGame game)
    {
        var agent = new GreedyAgent();
        var commands = game.LegalCommands().ToList();

        var greedyCommand = agent.NextCommand(game);
        var greedyIdx = commands.IndexOf(greedyCommand);

        Console.WriteLine($"Greedy choice: {greedyIdx}: {greedyCommand}");

        for (var i = 0; i < commands.Count; i++)
        {
            Console.WriteLine($"{i}: {commands[i]}");
        }

        return game;
    }

    private static PandemicGame BadInput(PandemicGame game)
    {
        Console.WriteLine("What?");
        return game;
    }

    private static PandemicGame Status(PandemicGame game)
    {
        Console.WriteLine(game);
        return game;
    }

    private static PandemicGame Help(PandemicGame game)
    {
        Console.WriteLine("h - help");
        Console.WriteLine("q - quit");
        Console.WriteLine("s - status");
        Console.WriteLine("c - available commands");
        Console.WriteLine("<number> - do command <number>");

        return game;
    }
}



interface ICommand { }
record QuitCommand() : ICommand;
record HelpCommand() : ICommand;
record StatusCommand() : ICommand;
record BadInputCommand() : ICommand;
record AvailableCommandsCommand : ICommand;
record GameCommand(int CommandNumber) : ICommand;
