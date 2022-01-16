using System;
using System.Linq;
using pandemic.agents;
using pandemic.Aggregates;

namespace pandemic.console;

class WinFinder
{
    public static void FindWinWithSolver(PandemicGame game, IPandemicGameSolver solver)
    {
        var commands = solver.CommandsToWin(game).ToList();

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
