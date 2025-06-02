namespace pandemic.agents.test;

using System.Collections.Generic;
using System.Linq;
using Aggregates.Game;
using Commands;
using GameData;
using NUnit.Framework;
using pandemic.test.Utils;
using Values;

internal class CommandPriorityComparerTests
{
    [Test]
    public void Gets_basics_right()
    {
        var game = ANewGame();
        var comparer = new CommandPriorityComparer(game);

        var commands = new List<IPlayerCommand>
        {
            new DiscardPlayerCardCommand(Role.Scientist, new EpidemicCard()),
            new DiscoverCureCommand(
                Role.Scientist,
                [new PlayerCityCard(StandardGameBoard.City("Atlanta"))]
            ),
            new DirectFlightCommand(Role.Scientist, "Chicago"),
            new DriveFerryCommand(Role.Scientist, "Chicago"),
            new BuildResearchStationCommand(Role.Scientist, "Miami"),
        };

        var sortedCommands = commands.OrderByDescending(c => c, comparer).ToList();

        Assert.That(
            sortedCommands.Select(c => c.GetType()).ToList(),
            Is.EqualTo(
                new[]
                {
                    typeof(DiscoverCureCommand),
                    typeof(BuildResearchStationCommand), // note that the comparer doesn't check for valid commands
                    typeof(DriveFerryCommand),
                    typeof(DiscardPlayerCardCommand), // this only ends up before direct flight due to order before sorting
                    typeof(DirectFlightCommand),
                }
            )
        );
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
            Is.GreaterThan(new BuildResearchStationCommand(Role.Scientist, "Chicago"))
                .Using(comparer)
        );
    }

    [Test]
    public void Prefers_drive_over_direct_flight_if_next_to_that_city()
    {
        var game = ANewGame();
        game = game with
        {
            Players = game.Players.Replace(
                game.CurrentPlayer,
                game.CurrentPlayer with
                {
                    Hand = PlayerHand.Empty.Add(PlayerCards.CityCard("Chicago")),
                }
            ),
        };

        var comparer = new CommandPriorityComparer(game);

        // atlanta already has a research station, we don't need another
        // station on a blue city
        Assert.That(
            new DriveFerryCommand(Role.Scientist, "Chicago"),
            Is.GreaterThan(new DirectFlightCommand(Role.Scientist, "Chicago")).Using(comparer)
        );
    }

    [Test]
    public void Prefers_drive_over_charter_flight_if_next_to_that_city()
    {
        var game = ANewGame();
        game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

        var comparer = new CommandPriorityComparer(game);

        Assert.That(
            new DriveFerryCommand(Role.Scientist, "Chicago"),
            Is.GreaterThan(
                    new CharterFlightCommand(
                        Role.Scientist,
                        PlayerCards.CityCard("Atlanta"),
                        "Chicago"
                    )
                )
                .Using(comparer)
        );
    }

    [Test]
    public void Prefers_shuttle_flight_over_charter()
    {
        var game = ANewGame();
        game = game.SetCurrentPlayerAs(game.CurrentPlayer with { Hand = PlayerHand.Of("Atlanta") });

        var bogota = game.CityByName("Bogota");

        game = game with
        {
            Cities = game.Cities.Replace(bogota, bogota with { HasResearchStation = true }),
        };

        var comparer = new CommandPriorityComparer(game);

        Assert.That(
            new ShuttleFlightCommand(Role.Scientist, "Bogota"),
            Is.GreaterThan(
                    new CharterFlightCommand(
                        Role.Scientist,
                        PlayerCards.CityCard("Atlanta"),
                        "Bogota"
                    )
                )
                .Using(comparer)
        );
    }

    private static PandemicGame ANewGame()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic },
            }
        );
        return game;
    }
}
