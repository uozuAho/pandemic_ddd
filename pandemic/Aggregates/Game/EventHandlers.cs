using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Events;
using pandemic.Values;

namespace pandemic.Aggregates.Game;

public partial record PandemicGame
{
    private (PandemicGame, ICollection<IEvent>) ApplyEvents(IEnumerable<IEvent> events)
    {
        var eventList = events.ToList();
        var state = eventList.Aggregate(this, ApplyEvent);
        return (state, eventList);
    }

    private (PandemicGame, ICollection<IEvent>) ApplyEvents(params IEvent[] events)
    {
        return ApplyEvents(events.AsEnumerable());
    }

    private PandemicGame ApplyEvent(IEvent @event, ICollection<IEvent> events)
    {
        events.Add(@event);
        return ApplyEvent(this, @event);
    }

    private static PandemicGame ApplyEvent(PandemicGame game, IEvent @event)
    {
        return @event switch
        {
            DifficultySet d => game with {Difficulty = d.Difficulty},
            EpidemicCardDiscarded e => ApplyEpidemicCardDiscarded(game, e),
            InfectionCardDrawn i => ApplyInfectionCardDrawn(game, i),
            InfectionDeckSetUp s => game with {InfectionDrawPile = s.Deck.ToImmutableList()},
            InfectionRateSet i => game with {InfectionRate = i.Rate},
            OutbreakCounterSet o => game with {OutbreakCounter = o.Value},
            PlayerAdded p => ApplyPlayerAdded(game, p),
            PlayerMoved p => ApplyPlayerMoved(game, p),
            ResearchStationBuilt r => ApplyResearchStationBuilt(game, r),
            PlayerCardPickedUp p => ApplyPlayerCardPickedUp(game),
            PlayerCardsDealt d => ApplyPlayerCardsDealt(game, d),
            PlayerDrawPileSetupWithEpidemicCards p => game with {PlayerDrawPile = p.DrawPile},
            PlayerDrawPileShuffledForDealing p => ApplyPlayerDrawPileSetUp(game, p),
            PlayerCardDiscarded p => ApplyPlayerCardDiscarded(game, p),
            CubeAddedToCity c => ApplyCubesAddedToCity(game, c),
            CureDiscovered c => ApplyCureDiscovered(game, c),
            GameLost g => game with {LossReason = g.Reason},
            TurnEnded t => ApplyTurnEnded(game),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };
    }

    private static PandemicGame ApplyEpidemicCardDiscarded(PandemicGame game, EpidemicCardDiscarded e)
    {
        var player = game.PlayerByRole(e.Player.Role);
        var discardedCard = game.PlayerByRole(e.Player.Role).Hand.First(c => c is EpidemicCard);

        return game with
        {
            Players = game.Players.Replace(player, player with
            {
                Hand = e.Player.Hand.Remove(discardedCard)
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.Add(discardedCard)
        };
    }

    private static PandemicGame ApplyCureDiscovered(PandemicGame game, CureDiscovered c)
    {
        return game with
        {
            CureDiscovered = game.CureDiscovered.SetItem(c.Colour, true),
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
            {
                ActionsRemaining = game.CurrentPlayer.ActionsRemaining - 1
            })
        };
    }

    private static PandemicGame ApplyResearchStationBuilt(PandemicGame game, ResearchStationBuilt @event)
    {
        var city = game.Cities.Single(c => c.Name == @event.City);

        return game with
        {
            Cities = game.Cities.Replace(city, city with
            {
                HasResearchStation = true
            }),
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
            {
                ActionsRemaining = game.CurrentPlayer.ActionsRemaining - 1
            })
        };
    }

    private static PandemicGame ApplyPlayerDrawPileSetUp(PandemicGame game, PlayerDrawPileShuffledForDealing @event)
    {
        return game with
        {
            PlayerDrawPile = @event.Pile
        };
    }

    private static PandemicGame ApplyPlayerCardsDealt(PandemicGame game, PlayerCardsDealt dealt)
    {
        var cards = game.PlayerDrawPile.TakeLast(dealt.Cards.Length).ToList();
        var player = game.PlayerByRole(dealt.Role);

        return game with
        {
            PlayerDrawPile = game.PlayerDrawPile.RemoveRange(cards),
            Players = game.Players.Replace(player, player with
            {
                Hand = new PlayerHand(cards)
            })
        };
    }

    private static PandemicGame ApplyInfectionCardDrawn(PandemicGame game, InfectionCardDrawn drawn)
    {
        return game with
        {
            InfectionDrawPile = game.InfectionDrawPile.RemoveAt(game.InfectionDrawPile.Count - 1),
            InfectionDiscardPile = game.InfectionDiscardPile.Add(drawn.Card),
        };
    }

    private static PandemicGame ApplyCubesAddedToCity(PandemicGame game, CubeAddedToCity cubeAddedToCity)
    {
        var city = game.CityByName(cubeAddedToCity.City.Name);
        var colour = cubeAddedToCity.City.Colour;
        var newCity = city with { Cubes = city.Cubes.SetItem(colour, city.Cubes[colour] + 1) };

        return game with
        {
            Cities = game.Cities.Replace(city, newCity),
            Cubes = game.Cubes.SetItem(colour, game.Cubes[colour] - 1)
        };
    }

    private static PandemicGame ApplyPlayerCardPickedUp(PandemicGame game)
    {
        var pickedCard = game.PlayerDrawPile.Last();
        return game with
        {
            PlayerDrawPile = game.PlayerDrawPile.Remove(pickedCard),
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(pickedCard)
            })
        };
    }

    private static PandemicGame ApplyPlayerCardDiscarded(PandemicGame game, PlayerCardDiscarded discarded)
    {
        return game with
        {
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Remove(discarded.Card)
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.Add(discarded.Card)
        };
    }

    private static PandemicGame ApplyPlayerAdded(PandemicGame pandemicGame, PlayerAdded playerAdded)
    {
        var newPlayers = pandemicGame.Players.Select(p => p with { }).ToList();
        newPlayers.Add(new Player {Role = playerAdded.Role, Location = "Atlanta"});

        return pandemicGame with { Players = newPlayers.ToImmutableList() };
    }

    private static PandemicGame ApplyPlayerMoved(PandemicGame pandemicGame, PlayerMoved playerMoved)
    {
        var newPlayers = pandemicGame.Players.Select(p => p).ToList();
        var movedPlayerIdx = newPlayers.FindIndex(p => p.Role == playerMoved.Role);
        var movedPlayer = newPlayers[movedPlayerIdx];

        newPlayers[movedPlayerIdx] = movedPlayer with
        {
            Location = playerMoved.Location,
            ActionsRemaining = movedPlayer.ActionsRemaining - 1
        };

        return pandemicGame with {Players = newPlayers.ToImmutableList()};
    }

    private static PandemicGame ApplyTurnEnded(PandemicGame game)
    {
        return game with
        {
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with {ActionsRemaining = 4}),
            CurrentPlayerIdx = (game.CurrentPlayerIdx + 1) % game.Players.Count
        };
    }
}
