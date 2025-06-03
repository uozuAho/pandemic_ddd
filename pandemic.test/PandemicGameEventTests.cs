namespace pandemic.test;

using System;
using System.Collections.Generic;
using System.Linq;
using Aggregates.Game;
using Commands;
using Events;
using NUnit.Framework;
using Utils;

internal class PandemicGameEventTests
{
    [Test]
    [Repeat(10)]
    public void State_built_from_events_is_same_as_final_state()
    {
        var options = NewGameOptionsGenerator.RandomOptions();

        var random = new Random();
        PandemicGame game;
        List<IEvent> events;
        (game, events) = PandemicGame.CreateNewGame(options);

        for (var i = 0; i < 1000 && !game.IsOver; i++)
        {
            var legalActions = PlayerCommandGenerator.AllLegalCommands(game).ToList();
            if (legalActions.Count == 0)
            {
                Assert.Fail(
                    $"No legal actions! State: \n\n{game}\n\n Events:\n{string.Join('\n', events)}"
                );
            }
            var action = RandomChoice(legalActions, random);
            var (updatedGame, newEvents) = game.Do(action);
            game = updatedGame;
            events.AddRange(newEvents);
        }

        var builtFromEvents = PandemicGame.FromEvents(events);

        Assert.IsTrue(game.IsSameStateAs(builtFromEvents));
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
