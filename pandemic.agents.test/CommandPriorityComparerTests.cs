using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates;
using pandemic.Aggregates.Game;
using pandemic.Commands;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.agents.test
{
    internal class CommandPriorityComparerTests
    {
        private static readonly StandardGameBoard Board = new();

        [Test]
        public void Gets_basics_right()
        {
            var game = ANewGame();
            var comparer = new CommandPriorityComparer(game);

            var commands = new List<PlayerCommand>
            {
                new DiscardPlayerCardCommand(new EpidemicCard()),
                new DiscoverCureCommand(new[] { new PlayerCityCard(Board.City("Atlanta")) }),
                new DriveFerryCommand(Role.Scientist, "Atlanta"),
                new BuildResearchStationCommand("Miami"),
            };

            var sortedCommands = commands.OrderByDescending(c => c, comparer).ToList();

            Assert.AreEqual(typeof(DiscoverCureCommand), sortedCommands[0].GetType());
            Assert.AreEqual(typeof(BuildResearchStationCommand), sortedCommands[1].GetType());
            Assert.AreEqual(typeof(DriveFerryCommand), sortedCommands[2].GetType());
            Assert.AreEqual(typeof(DiscardPlayerCardCommand), sortedCommands[3].GetType());
        }

        [Test]
        public void Avoids_building_research_stations_of_same_colour()
        {
            var game = ANewGame();
            var comparer = new CommandPriorityComparer(game);

            // atlanta already has a research station, we don't need another
            // station on a blue city
            Assert.That(
                new DriveFerryCommand(Role.Scientist, "Paris"),
                Is.GreaterThan(new BuildResearchStationCommand("Chicago"))
                    .Using(comparer));
        }

        [Test]
        public void Prefers_drive_over_direct_flight_if_next_to_that_city()
        {
            var game = ANewGame();
            game = game with
            {
                Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
                {
                    Hand = PlayerHand.Empty.Add(PlayerCards.CityCard("Chicago"))
                })
            };

            var comparer = new CommandPriorityComparer(game);

            // atlanta already has a research station, we don't need another
            // station on a blue city
            Assert.That(
                new DriveFerryCommand(Role.Scientist, "Chicago"),
                Is.GreaterThan(new DirectFlightCommand(Role.Scientist, "Chicago"))
                    .Using(comparer));
        }

        // todo: prefers cure over direct flight

        private static PandemicGame ANewGame()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            return game;
        }
    }
}
