namespace pandemic.console;

using System;
using System.Linq;
using agents;
using Aggregates.Game;

internal class WinFinder
{
    public static void FindWinWithSolver(
        PandemicGame game,
        IPandemicGameSolver solver,
        TimeSpan timeout
    )
    {
        var commands = solver.CommandsToWin(game, timeout).ToList();

        if (commands.Any())
        {
            Console.WriteLine("found win!");
            foreach (var command in commands)
            {
                Console.WriteLine(command);
            }
        }
        else
        {
            Console.WriteLine("no win found");
        }
    }
}
