namespace pandemic.agents.test;

using System.Linq;
using Aggregates.Game;
using GameData;
using NUnit.Framework;
using Values;

internal class DfsWithHeuristicsAgent_CanWin
{
    [Test]
    public void Can_win_new_game()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = [Role.Scientist, Role.Medic],
            }
        );

        Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game));
    }

    [Test]
    public void Cant_win_when_no_player_cards()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = [Role.Scientist, Role.Medic],
            }
        );
        game = game with { PlayerDrawPile = Deck.Empty<PlayerCard>() };

        Assert.IsFalse(DfsWithHeuristicsAgent.CanWin(game));
    }

    [Test]
    public void Cant_win_when_none_cured_and_19_cards_available()
    {
        var (game, events) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic },
            }
        );
        var cardsInPlayersHands = game.Players.Sum(p => p.Hand.Count);
        var cardsInPlayerDrawPile = 19 - cardsInPlayersHands;
        game = game with
        {
            PlayerDrawPile = new Deck<PlayerCard>(
                Enumerable.Range(0, cardsInPlayerDrawPile).Select(_ => new EpidemicCard())
            ),
        };

        Assert.IsFalse(DfsWithHeuristicsAgent.CanWin(game));
    }

    // 20 cards = 4 * 5: enough to cure all 4 diseases, ignoring special abilities
    [Test]
    public void Can_win_when_none_cured_and_20_cards_available()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = [Role.Scientist, Role.Medic],
                IncludeSpecialEventCards = false,
            }
        );
        var cardsInPlayersHands = game.Players.Sum(p => p.Hand.Count);
        game = game with
        {
            PlayerDrawPile = new Deck<PlayerCard>(
                PlayerCards.CityCards.Take(20 - cardsInPlayersHands)
            ),
        };

        Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game));
    }

    [Test]
    public void Can_win_when_there_are_enough_cards_of_all_colours()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic },
            }
        );
        var cardCounter = new CardCounter();

        Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game, cardCounter));
    }

    [Test]
    public void Can_win_is_true_when_card_counter_says_so()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = new[] { Role.Scientist, Role.Medic },
            }
        );
        game = game with { PlayerDrawPile = Deck.Empty<PlayerCard>() };
        var cardCounter = new CardCounter();

        Assert.IsTrue(DfsWithHeuristicsAgent.CanWin(game, cardCounter));
    }

    [Test]
    public void Cant_win_when_not_enough_blue_cards()
    {
        var (game, _) = PandemicGame.CreateNewGame(
            new NewGameOptions
            {
                Difficulty = Difficulty.Normal,
                Roles = [Role.Scientist, Role.Medic],
            }
        );
        var cardCounter = new CardCounter { CardsAvailable = { [Colour.Blue] = 4 } };

        Assert.IsFalse(DfsWithHeuristicsAgent.CanWin(game, cardCounter));
    }
}
