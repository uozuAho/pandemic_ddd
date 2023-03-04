using System;
using System.Collections.Generic;
using pandemic.Aggregates.Game;
using pandemic.Commands;

namespace pandemic.agents;

public class GreedyAgent
{
    public IPlayerCommand BestCommand(PandemicGame game)
    {
        return BestCommand(game, game.LegalCommands());
    }

    public IPlayerCommand BestCommand(PandemicGame game, IEnumerable<IPlayerCommand> commands)
    {
        var bestScore = int.MinValue;
        IPlayerCommand? bestCommand = null;

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
