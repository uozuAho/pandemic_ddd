using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using pandemic.Aggregates.Game;
using pandemic.Values;

namespace pandemic.agents.test
{
    internal class DfsWithHeuristicsAgent_CanWin
    {
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
            game = game with { PlayerDrawPile = Deck<PlayerCard>.Empty };

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
                PlayerDrawPile = new Deck<PlayerCard>(Enumerable
                    .Range(0, cardsInPlayerDrawPile)
                    .Select(_ => new EpidemicCard()))
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
                PlayerDrawPile = new Deck<PlayerCard>(Enumerable
                    .Range(0, cardsInPlayerDrawPile)
                    .Select(_ => new EpidemicCard()))
            };

            Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game));
        }

        [Test]
        public void Can_win_when_there_are_enough_cards_of_all_colours()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            var cardCounter = new CardCounter();

            Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game, cardCounter));
        }

        [Test]
        public void Can_win_is_true_when_card_counter_says_so()
        {
            var (game, _) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            game = game with { PlayerDrawPile = Deck<PlayerCard>.Empty };
            var cardCounter = new CardCounter();

            Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game, cardCounter));
        }

        [Test]
        public void Cant_win_when_not_enough_blue_cards()
        {
            var (game, events) = PandemicGame.CreateNewGame(new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic }
            });
            var cardCounter = new CardCounter
            {
                CardsAvailable =
                {
                    [Colour.Blue] = 4
                }
            };

            Assert.IsFalse(DfsWithHeuristicsAgent.CanWin(game, cardCounter));
        }
    }
}
