namespace pandemic.agents.test;

using System.Diagnostics;
using Aggregates.Game;
using Commands;
using GameData;
using GreedyBfs;
using NUnit.Framework;
using pandemic.test.Utils;
using Values;

internal class GreedyBfsTests
{
    [Test]
    public void Does_step()
    {
        var game = ANewGame();

        var bfs = new GreedyBestFirstSearch(game);

        _ = bfs.Step();
    }

    [Test]
    public void Chooses_cure_over_any_action()
    {
        var game = ANewGame();
        game = game with
        {
            Players = game.Players.Replace(
                game.CurrentPlayer,
                game.CurrentPlayer with
                {
                    Hand = PlayerHand.Of("Atlanta", "Chicago", "New York", "Montreal", "Paris"),
                }
            ),
        };

        var bfs = new GreedyBestFirstSearch(game);

        _ = bfs.Step(); // first node is root
        var nextNode = bfs.Step();
        Debug.Assert(nextNode != null);
        Assert.That(nextNode.Action, Is.TypeOf<DiscoverCureCommand>());
    }

    [Test]
    public void Keeps_cards_of_same_colour()
    {
        var game = ANewGame();
        game = game with
        {
            PhaseOfTurn = TurnPhase.DrawCards,
            Players = game.Players.Replace(
                game.CurrentPlayer,
                game.CurrentPlayer with
                {
                    Location = "Miami",
                    Hand = new PlayerHand(
                        new[]
                        {
                            // 4 yellows
                            new PlayerCityCard(StandardGameBoard.City("Miami")),
                            new PlayerCityCard(StandardGameBoard.City("Mexico City")),
                            new PlayerCityCard(StandardGameBoard.City("Los Angeles")),
                            new PlayerCityCard(StandardGameBoard.City("Lagos")),
                            // 2 others
                            new PlayerCityCard(StandardGameBoard.City("Jakarta")),
                            new PlayerCityCard(StandardGameBoard.City("Cairo")),
                            // 3 blues
                            new PlayerCityCard(StandardGameBoard.City("Paris")),
                            new PlayerCityCard(StandardGameBoard.City("Atlanta")),
                            new PlayerCityCard(StandardGameBoard.City("Chicago")),
                        }
                    ),
                    ActionsRemaining = 0,
                }
            ),
        };
        var bfs = new GreedyBestFirstSearch(game);

        _ = bfs.Step(); // first node is root
        var nextNode = bfs.Step();
        Assert.That(nextNode?.Action, Is.TypeOf<DiscardPlayerCardCommand>());
        var discardedCard = (nextNode?.Action as DiscardPlayerCardCommand)?.Card as PlayerCityCard;
        Assert.That(discardedCard?.City.Name, Is.AnyOf("Jakarta", "Cairo"));

        nextNode = bfs.Step();
        Assert.That(nextNode?.Action, Is.TypeOf<DiscardPlayerCardCommand>());
        discardedCard = (nextNode?.Action as DiscardPlayerCardCommand)?.Card as PlayerCityCard;
        Assert.That(discardedCard?.City.Name, Is.AnyOf("Jakarta", "Cairo"), $"{nextNode?.State}");
    }

    private static PandemicGame ANewGame()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist },
                IncludeSpecialEventCards = false,
            }
        );

        game = game.WithNoEpidemics();

        return game with
        {
            SelfConsistencyCheckingEnabled = false,
        };
    }
}
