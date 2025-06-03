namespace pandemic.agents;

using System;
using Aggregates.Game;
using Commands;

public class GreedyAgent : ILiveAgent
{
    public IPlayerCommand NextCommand(PandemicGame game)
    {
        var bestScore = int.MinValue;
        IPlayerCommand? bestCommand = null;

        foreach (var command in game.LegalCommands())
        {
            var (nextState, _) = game.Do(command);

            var stateScore = GameEvaluators.HandCraftedGameEvaluator.Score(nextState);
            if (stateScore < bestScore)
            {
                continue;
            }

            bestScore = stateScore;
            bestCommand = command;
        }

        if (bestCommand == null)
        {
            throw new InvalidOperationException();
        }

        return bestCommand;
    }
}
