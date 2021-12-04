using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents.test
{
    internal class DfsWithHeuristicsAgentTests
    {
        private static readonly StandardGameBoard Board = new();

        [Test]
        public void Command_priorities()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            var commands = new List<PlayerCommand>
            {
                new DiscardPlayerCardCommand(new EpidemicCard()),
                new DiscoverCureCommand(new[] { new PlayerCityCard(Board.City("Atlanta")) }),
                new DriveFerryCommand(Role.Scientist, "asdf"),
                new BuildResearchStationCommand("asdf"),
            };

            var sortedCommands = commands.OrderBy(c => DfsWithHeuristicsAgent.CommandPriority(c, game)).ToList();

            Assert.AreEqual(typeof(DiscoverCureCommand), sortedCommands[0].GetType());
            Assert.AreEqual(typeof(BuildResearchStationCommand), sortedCommands[1].GetType());
            Assert.AreEqual(typeof(DriveFerryCommand), sortedCommands[2].GetType());
            Assert.AreEqual(typeof(DiscardPlayerCardCommand), sortedCommands[3].GetType());
        }

        [Test]
        public void Can_win_new_game()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });

            Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game));
        }

        [Test]
        public void Cant_win_when_no_player_cards()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            game = game with { PlayerDrawPile = ImmutableList<PlayerCard>.Empty };

            Assert.IsFalse(DfsWithHeuristicsAgent.CanWin(game));
        }

        [Test]
        public void Cant_win_when_none_cured_and_19_cards_available()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            var cardsInPlayersHands = game.Players.Sum(p => p.Hand.Count);
            var cardsInPlayerDrawPile = 19 - cardsInPlayersHands;
            game = game with
            {
                PlayerDrawPile = Enumerable
                    .Range(0, cardsInPlayerDrawPile)
                    .Select(_ => new EpidemicCard())
                    .Cast<PlayerCard>()
                    .ToImmutableList()
            };

            Assert.IsFalse(DfsWithHeuristicsAgent.CanWin(game));
        }

        [Test]
        public void Can_win_when_none_cured_and_20_cards_available()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            var cardsInPlayersHands = game.Players.Sum(p => p.Hand.Count);
            var cardsInPlayerDrawPile = 20 - cardsInPlayersHands;
            game = game with
            {
                PlayerDrawPile = Enumerable
                    .Range(0, cardsInPlayerDrawPile)
                    .Select(_ => new EpidemicCard())
                    .Cast<PlayerCard>()
                    .ToImmutableList()
            };

            Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game));
        }
    }
}
