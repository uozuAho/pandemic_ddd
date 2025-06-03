namespace pandemic.test;

using System;
using System.Collections.Generic;
using System.Linq;
using Aggregates.Game;
using NUnit.Framework;
using Utils;

internal class SpielRandomAgentTests
{
    [Test]
    [Repeat(10)]
    public void PlaysGameToCompletion()
    {
        var options = NewGameOptionsGenerator.RandomOptions();

        var random = new Random();
        var (game, events) = PandemicGame.CreateNewGame(options);
        var state = new PandemicSpielGameState(game);

        for (var i = 0; i < 1000 && !state.IsTerminal; i++)
        {
            var legalActions = state.LegalActions();
            if (!legalActions.Any())
            {
                Assert.Fail(
                    $"No legal actions! State: \n\n{state}\n\n Events:\n{string.Join('\n', events)}"
                );
            }
            var action = RandomChoice(state.LegalActions(), random);
            events.AddRange(state.ApplyAction(action));
        }

        Assert.IsTrue(state.IsTerminal, "Expected to reach terminal state in under 1000 actions");
    }

    private static T RandomChoice<T>(IEnumerable<T> items, Random random)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            throw new InvalidOperationException("no items to choose from!");
        }

        var idx = random.Next(0, itemList.Count);
        return itemList[idx];
    }
}
