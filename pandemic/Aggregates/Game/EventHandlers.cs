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
            DifficultySet d => game with { Difficulty = d.Difficulty },
            EpidemicPlayerCardDiscarded e => ApplyEpidemicCardDiscarded(game, e),
            InfectionCardDrawn i => ApplyInfectionCardDrawn(game, i),
            InfectionDeckSetUp s => game with { InfectionDrawPile = new Deck<InfectionCard>(s.Deck) },
            PlayerAdded p => ApplyPlayerAdded(game, p),
            PlayerDroveFerried p => ApplyPlayerMoved(game, p),
            ResearchStationBuilt r => ApplyResearchStationBuilt(game, r),
            PlayerCardPickedUp p => ApplyPlayerCardPickedUp(game),
            PlayerCardsDealt d => ApplyPlayerCardsDealt(game, d),
            PlayerDrawPileSetupWithEpidemicCards p => game with { PlayerDrawPile = new Deck<PlayerCard>(p.DrawPile) },
            PlayerDrawPileShuffledForDealing p => ApplyPlayerDrawPileSetUp(game, p),
            PlayerCardDiscarded p => ApplyPlayerCardDiscarded(game, p),
            CubeAddedToCity c => ApplyCubesAddedToCity(game, c),
            CureDiscovered c => ApplyCureDiscovered(game, c),
            GameLost g => game with { LossReason = g.Reason },
            TurnEnded t => ApplyTurnEnded(game),
            PlayerDirectFlewTo p => ApplyPlayerDirectFlewTo(game, p),
            PlayerCharterFlewTo p => ApplyPlayerCharterFlewTo(game, p),
            PlayerShuttleFlewTo p => ApplyPlayerShuttleFlewTo(game, p),
            TreatedDisease d => ApplyTreatedDisease(game, d),
            ShareKnowledgeGiven s => ApplyShareKnowledgeGiven(game, s),
            ShareKnowledgeTaken s => ApplyShareKnowledgeTaken(game, s),
            EpidemicInfectionCardDiscarded e => Apply(game, e),
            EpidemicCityInfected e => Apply(game, e),
            EpidemicIntensified e => Apply(game, e),
            InfectionRateIncreased e => Apply(game, e),
            TurnPhaseEnded e => Apply(game, e),
            EpidemicTriggered e => Apply(game, e),
            PlayerPassed p => Apply(game, p),
            DiseaseEradicated e => Apply(game, e),
            OutbreakOccurred e => Apply(game, e),
            GovernmentGrantUsed e => Apply(game, e),
            ChoseNotToUseSpecialEventCard e => Apply(game, e),
            EventForecastUsed e => Apply(game, e),
            AirliftUsed e => Apply(game, e),
            ResilientPopulationUsed e => Apply(game, e),
            OneQuietNightUsed e => Apply(game, e),
            OneQuietNightPassed e => Apply(game, e),
            DispatcherMovedPawnToOther e => Apply(game, e),
            DispatcherDroveFerriedPawn e => Apply(game, e),
            DispatcherDirectFlewPawn e => Apply(game, e),
            DispatcherCharterFlewPawn e => Apply(game, e),
            DispatcherShuttleFlewPawn e => Apply(game, e),
            OperationsExpertBuiltResearchStation e => Apply(game, e),
            OperationsExpertDiscardedToMoveFromStation e => Apply(game, e),
            MedicTreatedDisease e => Apply(game, e),
            MedicAutoRemovedCubes e => Apply(game, e),
            MedicPreventedInfection => game,
            _ => throw new ArgumentOutOfRangeException(nameof(@event), @event, null)
        };
    }

    private static PandemicGame Apply(PandemicGame game, MedicAutoRemovedCubes evt)
    {
        var city = game.CityByName(evt.City);
        var numberOfCubes = city.Cubes.NumberOf(evt.Colour);

        return game with
        {
            Cities = game.Cities.Replace(city, city with { Cubes = city.Cubes.RemoveAll(evt.Colour) }),
            Cubes = game.Cubes.AddCubes(evt.Colour, numberOfCubes)
        };
    }

    private static PandemicGame Apply(PandemicGame game, MedicTreatedDisease evt)
    {
        var medic = game.PlayerByRole(Role.Medic);
        var city = game.CityByName(evt.City);
        var numCubes = city.Cubes.NumberOf(evt.Colour);

        return game with
        {
            Players = game.Players.Replace(medic, medic with { ActionsRemaining = medic.ActionsRemaining - 1 }),
            Cities = game.Cities.Replace(city, city with
            {
                Cubes = city.Cubes.RemoveAll(evt.Colour)
            }),
            Cubes = game.Cubes.AddCubes(evt.Colour, numCubes),
        };
    }

    private static PandemicGame Apply(PandemicGame game, OperationsExpertDiscardedToMoveFromStation evt)
    {
        var opex = (OperationsExpert)game.PlayerByRole(Role.OperationsExpert);
        var card = opex.Hand.CityCards.Single(c => c.City.Name == evt.DiscardedCard);

        return game with
        {
            Players = game.Players.Replace(opex, opex with
            {
                Location = evt.Destination,
                ActionsRemaining = opex.ActionsRemaining - 1,
                Hand = opex.Hand.Remove(card),
                HasUsedDiscardAndMoveAbilityThisTurn = true
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(card)
        };
    }

    private static PandemicGame Apply(PandemicGame game, OperationsExpertBuiltResearchStation evt)
    {
        var opex = game.PlayerByRole(Role.OperationsExpert);
        var city = game.CityByName(opex.Location);

        return game with
        {
            Players = game.Players.Replace(opex, opex with { ActionsRemaining = opex.ActionsRemaining - 1 }),
            Cities = game.Cities.Replace(city, city with
            {
                HasResearchStation = true
            }),
            ResearchStationPile = game.ResearchStationPile - 1
        };
    }

    private static PandemicGame Apply(PandemicGame game, DispatcherShuttleFlewPawn evt)
    {
        var dispatcher = game.PlayerByRole(Role.Dispatcher);
        var playerToMove = game.PlayerByRole(evt.PlayerToMove);

        return game with
        {
            Players = game.Players.Replace(dispatcher, dispatcher with
            {
                ActionsRemaining = dispatcher.ActionsRemaining - 1
            }).Replace(playerToMove, playerToMove with { Location = evt.City })
        };
    }

    private static PandemicGame Apply(PandemicGame game, DispatcherCharterFlewPawn evt)
    {
        var dispatcher = game.PlayerByRole(Role.Dispatcher);
        var playerToMove = game.PlayerByRole(evt.PlayerToMove);
        var card = dispatcher.Hand.CityCards.Single(c => c.City.Name == playerToMove.Location);

        return game with
        {
            Players = game.Players.Replace(dispatcher, dispatcher with
            {
                ActionsRemaining = dispatcher.ActionsRemaining - 1,
                Hand = dispatcher.Hand.Remove(card)
            }).Replace(playerToMove, playerToMove with
            {
                Location = evt.Destination
            }),

            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(card)
        };
    }

    private static PandemicGame Apply(PandemicGame game, DispatcherDirectFlewPawn evt)
    {
        var dispatcher = game.PlayerByRole(Role.Dispatcher);
        var card = dispatcher.Hand.CityCards.Single(c => c.City.Name == evt.City);
        var playerToMove = game.PlayerByRole(evt.PlayerToMove);

        return game with
        {
            Players = game.Players.Replace(dispatcher, dispatcher with
            {
                ActionsRemaining = dispatcher.ActionsRemaining - 1,
                Hand = dispatcher.Hand.Remove(card)
            }).Replace(playerToMove, playerToMove with
            {
                Location = evt.City
            }),

            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(card)
        };
    }

    private static PandemicGame Apply(PandemicGame game, DispatcherDroveFerriedPawn evt)
    {
        var dispatcher = game.PlayerByRole(Role.Dispatcher);
        var player = game.PlayerByRole(evt.Role);

        return game with
        {
            Players = game.Players
                .Replace(player, player with { Location = evt.City })
                .Replace(dispatcher, dispatcher with { ActionsRemaining = dispatcher.ActionsRemaining - 1 })
        };
    }

    private static PandemicGame Apply(PandemicGame game, DispatcherMovedPawnToOther evt)
    {
        var dispatcher = game.PlayerByRole(Role.Dispatcher);
        var playerToMove = game.PlayerByRole(evt.Role);
        var destinationPlayer = game.PlayerByRole(evt.DestinationRole);

        return game with
        {
            Players = playerToMove.Role == Role.Dispatcher
                ? game.Players.Replace(playerToMove, playerToMove with
                {
                    Location = destinationPlayer.Location,
                    ActionsRemaining = playerToMove.ActionsRemaining - 1
                })
                : game.Players.Replace(playerToMove, playerToMove with
                {
                    Location = destinationPlayer.Location
                }).Replace(dispatcher, dispatcher with
                {
                    ActionsRemaining = dispatcher.ActionsRemaining - 1
                })
        };
    }

    private static PandemicGame Apply(PandemicGame game, OneQuietNightPassed evt)
    {
        return game with { SkipNextInfectPhase = false };
    }

    private static PandemicGame Apply(PandemicGame game, OneQuietNightUsed evt)
    {
        var player = game.PlayerByRole(evt.Role);
        var card = player.Hand.Single(c => c is OneQuietNightCard);

        return game with
        {
            SkipNextInfectPhase = true,
            Players = game.Players.Replace(player, player with { Hand = player.Hand.Remove(card) }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(card)
        };
    }

    private static PandemicGame Apply(PandemicGame game, EpidemicCityInfected evt)
    {
        return game with { SpecialEventCanBeUsed = true };
    }

    private static PandemicGame Apply(PandemicGame game, EpidemicTriggered evt)
    {
        return game with { PhaseOfTurn = TurnPhase.Epidemic };
    }

    private static PandemicGame Apply(PandemicGame game, ResilientPopulationUsed evt)
    {
        var player = game.PlayerByRole(evt.Role);
        var eventCard = player.Hand.Single(c => c is ResilientPopulationCard);

        return game with
        {
            InfectionDiscardPile = game.InfectionDiscardPile.Remove(evt.InfectionCard),
            InfectionCardsRemovedFromGame = game.InfectionCardsRemovedFromGame.Add(evt.InfectionCard),
            Players = game.Players.Replace(player, player with
            {
                Hand = player.Hand.Remove(eventCard)
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(eventCard)
        };
    }

    private static PandemicGame Apply(PandemicGame game, AirliftUsed evt)
    {
        var playerWithCard = game.PlayerByRole(evt.Role);
        var card = playerWithCard.Hand.Single(c => c is AirliftCard);
        var playerToMove = game.PlayerByRole(evt.PlayerToMove);

        ImmutableList<Player> updatedPlayers;
        if (playerWithCard == playerToMove)
        {
            updatedPlayers = game.Players.Replace(playerWithCard, playerWithCard with
            {
                Location = evt.City,
                Hand = playerWithCard.Hand.Remove(card)
            });
        }
        else
        {
            updatedPlayers = game.Players.Replace(playerWithCard, playerWithCard with
            {
                Hand = playerWithCard.Hand.Remove(card)
            }).Replace(playerToMove, playerToMove with
            {
                Location = evt.City
            });
        }

        return game with
        {
            Players = updatedPlayers,
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(card)
        };
    }

    private static PandemicGame Apply(PandemicGame game, EventForecastUsed evt)
    {
        var eventForecastCard = game.PlayerByRole(evt.Role).Hand.Single(c => c is EventForecastCard);
        var player = game.PlayerByRole(evt.Role);
        var cardsToPlace = evt.Cards.ToList();

        return game with
        {
            Players = game.Players.Replace(player, player with
            {
                Hand = player.Hand.Remove(eventForecastCard)
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(eventForecastCard),
            InfectionDrawPile = game.InfectionDrawPile.Remove(cardsToPlace).PlaceOnTop(cardsToPlace)
        };
    }

    private static PandemicGame Apply(PandemicGame game, ChoseNotToUseSpecialEventCard evt)
    {
        return game with
        {
            SpecialEventCanBeUsed = false
        };
    }

    private static PandemicGame Apply(PandemicGame game, GovernmentGrantUsed evt)
    {
        var player = game.PlayerByRole(evt.Role);
        var card = player.Hand.Single(c => c is GovernmentGrantCard);
        var city = game.CityByName(evt.City);

        return game with
        {
            ResearchStationPile = game.ResearchStationPile - 1,
            Cities = game.Cities.Replace(city, city with { HasResearchStation = true }),
            Players = game.Players.Replace(player, player with { Hand = player.Hand.Remove(card) }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(card)
        };
    }

    private static PandemicGame Apply(PandemicGame game, OutbreakOccurred evt)
    {
        return game with { OutbreakCounter = game.OutbreakCounter + 1 };
    }

    private static PandemicGame Apply(PandemicGame game, DiseaseEradicated evt)
    {
        var cureMarker = game.CuresDiscovered.Single(m => m.Colour == evt.Colour);

        return game with
        {
            CuresDiscovered = game.CuresDiscovered.Replace(cureMarker, cureMarker.Flip())
        };
    }

    private static PandemicGame Apply(PandemicGame game, PlayerPassed evt)
    {
        var player = game.PlayerByRole(evt.Role);

        return game with
        {
            SpecialEventCanBeUsed = false,
            Players = game.Players.Replace(player, player with { ActionsRemaining = 0 }),
        };
    }

    private static PandemicGame Apply(PandemicGame game, TurnPhaseEnded evt)
    {
        return game with
        {
            PhaseOfTurn = evt.NextPhase,
            CardsDrawn = game.PhaseOfTurn == TurnPhase.DrawCards ? 0 : game.CardsDrawn
        };
    }

    private static PandemicGame ApplyTreatedDisease(PandemicGame game, TreatedDisease evt)
    {
        var player = game.PlayerByRole(evt.Role);
        var city = game.CityByName(evt.City);

        return game with
        {
            Players = game.Players.Replace(player, player with{ActionsRemaining = player.ActionsRemaining - 1}),
            Cities = game.Cities.Replace(city, city.RemoveCube(evt.Colour)),
            Cubes = game.Cubes.AddCube(evt.Colour),
        };
    }

    private static PandemicGame ApplyEpidemicCardDiscarded(PandemicGame game, EpidemicPlayerCardDiscarded e)
    {
        var player = game.PlayerByRole(e.Role);
        var discardedCard = player.Hand.First(c => c is EpidemicCard);

        return game with
        {
            Players = game.Players.Replace(player, player with
            {
                Hand = player.Hand.Remove(discardedCard)
            }),
            PlayerDiscardPile = game.PlayerDiscardPile.PlaceOnTop(discardedCard)
        };
    }

    private static PandemicGame ApplyCureDiscovered(PandemicGame game, CureDiscovered c)
    {
        return game with
        {
            CuresDiscovered = game.CuresDiscovered.Add(new CureMarker(c.Colour, CureMarkerSide.Vial)),
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
        var city = game.CityByName(cubeAddedToCity.City);
        var colour = cubeAddedToCity.Colour;
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
            SpecialEventCanBeUsed = drawnCard is ISpecialEventCard || game.CurrentPlayer.Hand.Count > 6,
            CardsDrawn = game.CardsDrawn + 1,
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

    private static PandemicGame ApplyPlayerAdded(PandemicGame game, PlayerAdded evt)
    {
        var newPlayer = evt.Role == Role.OperationsExpert
            ? new OperationsExpert { Location = "Atlanta" }
            : new Player { Role = evt.Role, Location = "Atlanta" };

        return game with { Players = game.Players.Add(newPlayer) };
    }

    private static PandemicGame ApplyPlayerMoved(PandemicGame pandemicGame, PlayerDroveFerried playerDroveFerried)
    {
        var newPlayers = pandemicGame.Players.Select(p => p).ToList();
        var movedPlayerIdx = newPlayers.FindIndex(p => p.Role == playerDroveFerried.Role);
        var movedPlayer = newPlayers[movedPlayerIdx];

        newPlayers[movedPlayerIdx] = movedPlayer with
        {
            Location = playerDroveFerried.Location,
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

    private static PandemicGame Apply(PandemicGame game, EpidemicIntensified evt)
    {
        return game with
        {
            InfectionDiscardPile = Deck<InfectionCard>.Empty,
            InfectionDrawPile = game.InfectionDrawPile.PlaceOnTop(evt.ShuffledDiscardPile)
        };
    }

    private static PandemicGame Apply(PandemicGame game, InfectionRateIncreased _)
    {
        return game with
        {
            InfectionRateMarkerPosition = game.InfectionRateMarkerPosition + 1
        };
    }

    private static PandemicGame ApplyTurnEnded(PandemicGame game)
    {
        if (game.CurrentPlayer.Role == Role.OperationsExpert)
        {
            var opex = (OperationsExpert)game.CurrentPlayer;
            game = game with
            {
                Players = game.Players.Replace(opex, opex with { HasUsedDiscardAndMoveAbilityThisTurn = false })
            };
        }

        return game with
        {
            Players = game.Players.Replace(game.CurrentPlayer, game.CurrentPlayer with {ActionsRemaining = 4}),
            CurrentPlayerIdx = (game.CurrentPlayerIdx + 1) % game.Players.Count,
            PhaseOfTurn = TurnPhase.DoActions,
            SpecialEventCanBeUsed = true
        };
    }
}
