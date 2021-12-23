using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pandemic.agents.GreedyBfs;
using pandemic.Aggregates;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents.test
{
    internal class GreedyBfsTests
    {
        private static StandardGameBoard Board = new();

        [Test]
        public void Does_step()
        {
            var game = ANewGame();
            var problem = new PandemicSearchProblem(game, new PlayerCommandGeneratorFast());

            var bfs = new GreedyBestFirstSearch<PandemicGame, PlayerCommand>(problem, state => -GameEvaluator.Evaluate(state));

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
                    Hand = new PlayerHand(Enumerable.Repeat(new PlayerCityCard(Board.City("Atlanta")), 5))
                })
            };
            var problem = new PandemicSearchProblem(game, new PlayerCommandGeneratorFast());

            var bfs = new GreedyBestFirstSearch<PandemicGame, PlayerCommand>(problem, state => -GameEvaluator.Evaluate(state));

            bfs.Step(); // first node is root
            var nextNode = bfs.Step(); 
            Assert.That(nextNode, Is.Not.Null);
            Assert.That(nextNode.Action, Is.TypeOf<DiscoverCureCommand>());
        }

        // choose build research station over move
        // keep cards of same colour

        private PandemicGame ANewGame()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Heroic,
                Roles = new[] { Role.Medic, Role.QuarantineSpecialist },
            });

            return game;
        }
    }
}
