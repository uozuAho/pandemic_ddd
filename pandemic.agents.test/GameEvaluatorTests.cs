using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;
using Shouldly;

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
