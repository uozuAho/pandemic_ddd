using System.Diagnostics;
using NUnit.Framework;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.GameData;
using pandemic.test.Utils;
using pandemic.Values;

namespace pandemic.agents.test
{
    internal class GreedyBfsTests
    {
        private static readonly StandardGameBoard Board = StandardGameBoard.Instance();

        [Test]
        public void Does_step()
        {
            var game = ANewGame();

            var bfs = new GreedyBestFirstSearch(game);

            bfs.Step();
        }

        [Test]
        public void Chooses_cure_over_any_action()
        {
            var game = ANewGame();
            game = game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Hand = PlayerHand.Of("Atlanta", "Chicago", "New York", "Montreal", "Paris")
                })
            };

            var bfs = new GreedyBestFirstSearch(game);

            bfs.Step(); // first node is root
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
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Location = "Miami",
                    Hand = new PlayerHand(new[]
                    {
                        // 4 yellows
                        new PlayerCityCard(Board.City("Miami")),
                        new PlayerCityCard(Board.City("Mexico City")),
                        new PlayerCityCard(Board.City("Los Angeles")),
                        new PlayerCityCard(Board.City("Lagos")),

                        // 2 others
                        new PlayerCityCard(Board.City("Jakarta")),
                        new PlayerCityCard(Board.City("Cairo")),

                        // 3 blues
                        new PlayerCityCard(Board.City("Paris")),
                        new PlayerCityCard(Board.City("Atlanta")),
                        new PlayerCityCard(Board.City("Chicago")),
                    }),
                    ActionsRemaining = 0
                })
            };
            var bfs = new GreedyBestFirstSearch(game);

            bfs.Step(); // first node is root
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
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist },
                IncludeSpecialEventCards = false
            });

            game = game.WithNoEpidemics();

            return game with { SelfConsistencyCheckingEnabled = false };
        }
    }
}
