using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using pandemic.Events;
using pandemic.GameData;
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
            InfectionDeckSetUp s => game with {InfectionDrawPile = new Deck<InfectionCard>(s.Deck)},
            InfectionRateSet i => game with {InfectionRate = i.Rate},
            OutbreakCounterSet o => game with {OutbreakCounter = o.Value},
            PlayerAdded p => ApplyPlayerAdded(game, p),
            PlayerMoved p => ApplyPlayerMoved(game, p),
            ResearchStationBuilt r => ApplyResearchStationBuilt(game, r),
            PlayerCardPickedUp p => ApplyPlayerCardPickedUp(game),
            PlayerCardsDealt d => ApplyPlayerCardsDealt(game, d),
            PlayerDrawPileSetupWithEpidemicCards p => game with {PlayerDrawPile = new Deck<PlayerCard>(p.DrawPile)},
            PlayerDrawPileShuffledForDealing p => ApplyPlayerDrawPileSetUp(game, p),
            PlayerCardDiscarded p => ApplyPlayerCardDiscarded(game, p),
            CubeAddedToCity c => ApplyCubesAddedToCity(game, c),
            CureDiscovered c => ApplyCureDiscovered(game, c),
            GameLost g => game with {LossReason = g.Reason},
            TurnEnded t => ApplyTurnEnded(game),
            PlayerDirectFlewTo p => ApplyPlayerDirectFlewTo(game, p),
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
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(discardedCard)
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
            PlayerDrawPile = new Deck<PlayerCard>(@event.Pile)
        };
    }

    private static PandemicGame ApplyPlayerCardsDealt(PandemicGame game, PlayerCardsDealt dealt)
    {
        var (newDrawPile, cards) = game.PlayerDrawPile.Draw(dealt.Cards.Length);
        var player = game.PlayerByRole(dealt.Role);

        return game with
        {
            PlayerDrawPile = newDrawPile,
            Players = game.Players.Replace(player, player with
            {
                Hand = new PlayerHand(cards)
            })
        };
    }

    private static PandemicGame ApplyInfectionCardDrawn(PandemicGame game, InfectionCardDrawn drawn)
    {
        var (newDrawPile, drawnCard) = game.InfectionDrawPile.Draw();

        if (drawnCard != drawn.Card)
            throw new InvalidOperationException(
                "Card at top of draw pile should be the same as the drawn card in the event");

        return game with
        {
            InfectionDrawPile = newDrawPile,
            InfectionDiscardPile = game.InfectionDiscardPile.Add(drawnCard),
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
        var (newDrawPile, drawnCard) = game.PlayerDrawPile.Draw();

        return game with
        {
            PlayerDrawPile = newDrawPile,
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with
            {
                Hand = game.CurrentPlayer.Hand.Add(drawnCard)
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
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(discarded.Card)
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

    private static PandemicGame ApplyPlayerDirectFlewTo(PandemicGame game, PlayerDirectFlewTo evt)
    {
        var newPlayers = game.Players.Select(p => p).ToList();
        var movedPlayerIdx = newPlayers.FindIndex(p => p.Role == evt.Role);
        var movedPlayer = newPlayers[movedPlayerIdx];

        newPlayers[movedPlayerIdx] = movedPlayer with
        {
            Location = evt.City,
            ActionsRemaining = movedPlayer.ActionsRemaining - 1,
            Hand = movedPlayer.Hand.Remove(PlayerCards.CityCard(evt.City))
        };

        return game with {Players = newPlayers.ToImmutableList()};
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
