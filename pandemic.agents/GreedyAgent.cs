using System;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.Commands;

namespace pandemic.agents;

public class GreedyAgent
{
    public IPlayerCommand BestCommand(PandemicGame game)
    {
        var bestScore = int.MinValue;
        IPlayerCommand? bestCommand = null;

        var commands = game.LegalCommands().ToList();
        foreach (var command in commands)
        {
            var (nextState, _) = game.Do(command);

            var stateScore = GameEvaluator.Evaluate(nextState);
            if (stateScore < bestScore) continue;

            bestScore = stateScore;
            bestCommand = command;
        }

        if (bestCommand == null)
            throw new InvalidOperationException();

        return bestCommand;
    }
}
