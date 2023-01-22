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
            PlayerCharterFlewTo p => ApplyPlayerCharterFlewTo(game, p),
            PlayerShuttleFlewTo p => ApplyPlayerShuttleFlewTo(game, p),
            TreatedDisease d => ApplyTreatedDisease(game, d),
            ShareKnowledgeGiven s => ApplyShareKnowledgeGiven(game, s),
            ShareKnowledgeTaken s => ApplyShareKnowledgeTaken(game, s),
            EpidemicInfectionCardDiscarded e => Apply(game, e),
            EpidemicInfectionDiscardPileShuffledAndReplaced e => Apply(game, e),
            InfectionRateMarkerProgressed e => Apply(game, e),
            TurnPhaseEnded e => Apply(game, e),
            EpidemicTriggered => game,
            PlayerPassed p => Apply(game, p),
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };
    }

    private static PandemicGame Apply(PandemicGame game, PlayerPassed evt)
    {
        var player = game.PlayerByRole(evt.Role);

        return game with
        {
            Players = game.Players.Replace(player, player with { ActionsRemaining = player.ActionsRemaining - 1 })
        };
    }

    private static PandemicGame Apply(PandemicGame game, TurnPhaseEnded evt)
    {
        var nextPhase = game.PhaseOfTurn switch
        {
            TurnPhase.DoActions => TurnPhase.DrawCards,
            TurnPhase.DrawCards => TurnPhase.InfectCities,
            TurnPhase.InfectCities => TurnPhase.DoActions,
            _ => throw new ArgumentOutOfRangeException()
        };

        return game with { PhaseOfTurn = nextPhase };
    }

    private static PandemicGame ApplyTreatedDisease(PandemicGame game, TreatedDisease evt)
    {
        var player = game.PlayerByRole(evt.Role);
        var city = game.CityByName(evt.City);

        if (game.IsCured(evt.Colour))
        {
            var numRemainingCubes = game.Cities.Select(c => c.Cubes.NumberOf(evt.Colour)).Sum();
        }

        return game with
        {
            Players = game.Players.Replace(player, player with{ActionsRemaining = player.ActionsRemaining - 1}),
            Cities = game.Cities.Replace(city, city.RemoveCube(evt.Colour)),
            Cubes = game.Cubes.AddCube(evt.Colour),
            // todo: make sure this breaks some existing tests
            Eradicated = game.Eradicated.SetItem(Colour.Blue, true)
        };
    }

    private static PandemicGame ApplyEpidemicCardDiscarded(PandemicGame game, EpidemicCardDiscarded e)
    {
        var player = game.PlayerByRole(e.Player.Role);
        var discardedCard = player.Hand.First(c => c is EpidemicCard);

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
            }),
            ResearchStationPile = game.ResearchStationPile - 1
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
            InfectionDiscardPile = game.InfectionDiscardPile.PlaceOnTop(drawnCard),
        };
    }

    private static PandemicGame ApplyCubesAddedToCity(PandemicGame game, CubeAddedToCity cubeAddedToCity)
    {
        var city = game.CityByName(cubeAddedToCity.City.Name);
        var colour = cubeAddedToCity.City.Colour;
        var newCity = city.AddCube(colour);

        return game with
        {
            Cities = game.Cities.Replace(city, newCity),
            Cubes = game.Cubes.RemoveCube(colour)
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

    private static PandemicGame ApplyPlayerCardDiscarded(PandemicGame game, PlayerCardDiscarded evt)
    {
        var player = game.PlayerByRole(evt.Role);

        return game with
        {
            Players = game.Players.Replace(player, player with
            {
                Hand = player.Hand.Remove(evt.Card)
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(evt.Card)
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
        var player = game.PlayerByRole(evt.Role);
        var playedCityCard = PlayerCards.CityCard(evt.Destination);

        return game with
        {
            Players = game.Players.Replace(player, player with
            {
                Location = evt.Destination,
                ActionsRemaining = player.ActionsRemaining - 1,
                Hand = player.Hand.Remove(playedCityCard)
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(playedCityCard)
        };
    }

    private static PandemicGame ApplyPlayerCharterFlewTo(PandemicGame game, PlayerCharterFlewTo evt)
    {
        var player = game.PlayerByRole(evt.Role);
        var card = PlayerCards.CityCard(player.Location);

        return game with
        {
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(card),
            Players = game.Players.Replace(player, player with
            {
                Location = evt.City,
                ActionsRemaining = player.ActionsRemaining - 1,
                Hand = player.Hand.Remove(card)
            })
        };
    }

    private static PandemicGame ApplyPlayerShuttleFlewTo(PandemicGame game, PlayerShuttleFlewTo evt)
    {
        var player = game.PlayerByRole(evt.Role);

        return game with
        {
            Players = game.Players.Replace(player, player with
            {
                Location = evt.City,
                ActionsRemaining = player.ActionsRemaining - 1
            })
        };
    }

    private static PandemicGame ApplyShareKnowledgeGiven(PandemicGame game, ShareKnowledgeGiven evt)
    {
        var giver = game.PlayerByRole(evt.Role);
        var card = giver.Hand.CityCards.Single(c => c.City.Name == evt.City);
        var receiver = game.PlayerByRole(evt.ReceivingRole);

        return game with
        {
            Players = game.Players
                .Replace(giver, giver with
                {
                    Hand = giver.Hand.Remove(card),
                    ActionsRemaining = giver.ActionsRemaining - 1
                })
                .Replace(receiver, receiver with
                {
                    Hand = receiver.Hand.Add(card)
                })
        };
    }

    private static PandemicGame ApplyShareKnowledgeTaken(PandemicGame game, ShareKnowledgeTaken evt)
    {
        var taker = game.PlayerByRole(evt.Role);
        var takenFromPlayer = game.PlayerByRole(evt.TakenFromRole);
        var card = takenFromPlayer.Hand.CityCards.Single(c => c.City.Name == evt.City);

        return game with
        {
            Players = game.Players
                .Replace(taker, taker with
                {
                    Hand = taker.Hand.Add(card),
                    ActionsRemaining = taker.ActionsRemaining - 1
                })
                .Replace(takenFromPlayer, takenFromPlayer with
                {
                    Hand = takenFromPlayer.Hand.Remove(card),
                })
        };
    }

    private static PandemicGame Apply(PandemicGame game, EpidemicInfectionCardDiscarded evt)
    {
        var (newDrawPile, bottomCard) = game.InfectionDrawPile.DrawFromBottom();
        if (bottomCard != evt.Card) throw new InvalidOperationException("doh");

        return game with
        {
            InfectionDrawPile = newDrawPile,
            InfectionDiscardPile = game.InfectionDiscardPile.PlaceOnTop(evt.Card),
        };
    }

    private static PandemicGame Apply(PandemicGame game, EpidemicInfectionDiscardPileShuffledAndReplaced evt)
    {
        return game with
        {
            InfectionDiscardPile = Deck<InfectionCard>.Empty,
            InfectionDrawPile = game.InfectionDrawPile.PlaceOnTop(evt.ShuffledDiscardPile)
        };
    }

    private static PandemicGame Apply(PandemicGame game, InfectionRateMarkerProgressed _)
    {
        return game with
        {
            InfectionRateMarkerPosition = game.InfectionRateMarkerPosition + 1
        };
    }

    private static PandemicGame ApplyTurnEnded(PandemicGame game)
    {
        return game with
        {
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with {ActionsRemaining = 4}),
            CurrentPlayerIdx = (game.CurrentPlayerIdx + 1) % game.Players.Count,
            PhaseOfTurn = TurnPhase.DoActions
        };
    }
}
