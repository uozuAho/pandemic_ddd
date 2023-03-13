using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;
using Shouldly;
using utils;

namespace pandemic.agents.test
{
    internal class GameEvaluatorTests_HandScore
    {
        [Test]
        public void Cards_of_same_colour_are_positive()
        {
            var board = StandardGameBoard.Instance();
            var hand = new PlayerHand(Enumerable
                .Repeat(new PlayerCityCard(board.City("Atlanta")), 3));

            GameEvaluator.PlayerHandScore(PandemicGame.CreateUninitialisedGame(), hand).ShouldBePositive();
        }

        [Test]
        public void Cards_of_cured_colour_are_worth_negative()
        {
            var board = StandardGameBoard.Instance();
            var game = PandemicGame.CreateUninitialisedGame().Cure(Colour.Blue);
            var hand = new PlayerHand(Enumerable
                .Repeat(new PlayerCityCard(board.City("Atlanta")), 5));

            GameEvaluator.PlayerHandScore(game, hand).ShouldBeNegative();
        }
    }

    internal class GameEvaluatorTests
    {
        [Test]
        public void Closer_to_high_cube_cities_is_better()
        {
            var game = PandemicGame.CreateUninitialisedGame();
            var atlanta = game.CityByName("Atlanta");
            game = game with
            {
                Cities = game.Cities.Replace(atlanta,
                    atlanta with { Cubes = atlanta.Cubes.AddCubes(Colour.Blue, 3) })
            };

            var gameClose = game with { Players = game.Players.Add(new Player { Location = "Chicago" }) };
            var gameFar = game with { Players = game.Players.Add(new Player { Location = "Paris" }) };

            var closeScore = GameEvaluator.Score(gameClose);
            var farScore = GameEvaluator.Score(gameFar);

            closeScore.ShouldBeGreaterThan(farScore);
        }

        [Test]
        public void Spread_players_to_cubes()
        {
            var game = PandemicGame.CreateUninitialisedGame();
            var atlanta = game.CityByName("Chicago");
            var stPeter = game.CityByName("St. Petersburg");
            game = game with
            {
                Cities = game.Cities
                    .Replace(atlanta, atlanta.AddCube(Colour.Blue))
                    .Replace(stPeter, stPeter.AddCube(Colour.Blue))
            };
            StandardGameBoard.DriveFerryDistance("Chicago", "St. Petersburg").ShouldBe(5);

            // expected rank of player positions, best to worst:
            //    X                       X  <-- cities with cubes
            // A: o                       o  <-- o = player position
            // B: o                  o
            // C: o           o
            // D: o     o
            // E: oo
            var gameA = game with
            {
                Players = game.Players
                    .Add(new Player { Location = "Chicago" })
                    .Add(new Player { Location = "St. Petersburg" })
            };
            var gameB = game with
            {
                Players = game.Players
                    .Add(new Player { Location = "Chicago" })
                    .Add(new Player { Location = "Essen" })
            };
            var gameC = game with
            {
                Players = game.Players
                    .Add(new Player { Location = "Chicago" })
                    .Add(new Player { Location = "London" })
            };
            var gameD = game with
            {
                Players = game.Players
                    .Add(new Player { Location = "Chicago" })
                    .Add(new Player { Location = "New York" })
            };
            var gameE = game with
            {
                Players = game.Players
                    .Add(new Player { Location = "Chicago" })
                    .Add(new Player { Location = "Chicago" })
            };

            var orderedGames = new[] { ("A", gameA), ("B", gameB), ("C", gameC), ("D", gameD), ("E", gameE) };
            var shuffledGames = orderedGames.Shuffle();

            shuffledGames
                .OrderByDescending(g => GameEvaluator.Score(g.Item2))
                .Select(g => g.Item1)
                .ShouldBe(new[] { "A", "B", "C", "D", "E" });
        }

        [Test]
        public void When_player_has_enough_cards_to_cure__closer_to_station_is_better()
        {
            var game = PandemicGame.CreateUninitialisedGame();
            var atlanta = game.CityByName("Atlanta");
            var paris = game.CityByName("Paris");
            game = game with
            {
                Cities = game.Cities
                    .Replace(atlanta, atlanta with { Cubes = atlanta.Cubes.AddCubes(Colour.Blue, 3) })
                    .Replace(paris, paris with { HasResearchStation = true })
            };
            var player = new Player
            {
                Hand = PlayerHand.Of("Atlanta", "Chicago", "New York", "Montreal", "Paris")
            };

            var atChicago = game with { Players = game.Players.Add(player with { Location = "Chicago" }) };
            var atParis = game with { Players = game.Players.Add(player with { Location = "Paris" }) };

            var scoreAtChicago = GameEvaluator.Score(atChicago);
            var scoreAtParis = GameEvaluator.Score(atParis);

            scoreAtParis.ShouldBeGreaterThan(scoreAtChicago);
        }
    }
}
