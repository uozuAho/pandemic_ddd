using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Combinatorics.Collections;
using pandemic.Aggregates.Game;
using pandemic.GameData;
using pandemic.Values;

namespace pandemic.Commands
{
    public class PlayerCommandGenerator
    {
        public IEnumerable<IPlayerCommand> AllLegalCommands(PandemicGame game)
        {
            if (game.IsOver) return Enumerable.Empty<IPlayerCommand>();

            var commands = new List<IPlayerCommand>();

            commands.AddRange(DiscardCommands(game));
            commands.AddRange(SpecialEventCommands(game));

            if (game.APlayerMustDiscard) return commands;

            if (game is { PhaseOfTurn: TurnPhase.DoActions, CurrentPlayer.ActionsRemaining: > 0 })
            {
                commands.AddRange(DriveFerryCommands(game));
                commands.AddRange(BuildResearchStationCommands(game));
                commands.AddRange(CureCommands(game));
                commands.AddRange(DirectFlightCommands(game));
                commands.AddRange(CharterFlightCommands(game));
                commands.AddRange(ShuttleFlightCommands(game));
                commands.AddRange(TreatDiseaseCommands(game));
                commands.AddRange(ShareKnowledgeGiveCommands(game));
                commands.AddRange(ShareKnowledgeTakeCommands(game));
                commands.AddRange(PassCommands(game));
                commands.AddRange(DispatcherCommands(game));
                commands.AddRange(OperationsExpertCommands(game));
                commands.AddRange(ResearcherCommands(game));
                commands.AddRange(ScientistCommands(game));
                commands.AddRange(ContingencyPlannerTakeCommands(game));
            }

            if (commands.All(c => c.IsSpecialEvent))
            {
                foreach (var group in commands.GroupBy(c => c.Role))
                {
                    commands.Add(new DontUseSpecialEventCommand(group.Key));
                }
            }

            return commands;
        }

        /// <summary>
        /// Aims to be faster than AllLegalCommands by only generating 'sensible' commands
        /// </summary>
        public IEnumerable<IPlayerCommand> AllSensibleCommands(PandemicGame game)
        {
            if (game.IsOver) return Enumerable.Empty<IPlayerCommand>();

            var commands = new List<IPlayerCommand>();

            commands.AddRange(DiscardCommands(game));
            commands.AddRange(SensibleSpecialEventCommands(game));

            if (game.APlayerMustDiscard) return commands;

            if (game is { PhaseOfTurn: TurnPhase.DoActions, CurrentPlayer.ActionsRemaining: > 0 })
            {
                commands.AddRange(DriveFerryCommands(game));
                commands.AddRange(BuildResearchStationCommands(game));
                commands.AddRange(CureCommands(game));
                commands.AddRange(DirectFlightCommands(game));
                commands.AddRange(CharterFlightCommands(game));
                commands.AddRange(ShuttleFlightCommands(game));
                commands.AddRange(TreatDiseaseCommands(game));
                commands.AddRange(ShareKnowledgeGiveCommands(game));
                commands.AddRange(ShareKnowledgeTakeCommands(game));
                commands.AddRange(PassCommands(game));
                commands.AddRange(DispatcherCommands(game));
                commands.AddRange(OperationsExpertCommands(game));
                commands.AddRange(ResearcherCommands(game));
                commands.AddRange(ScientistCommands(game));
                commands.AddRange(ContingencyPlannerTakeCommands(game));
            }

            if (commands.All(c => c.IsSpecialEvent))
            {
                foreach (var group in commands.GroupBy(c => c.Role))
                {
                    commands.Add(new DontUseSpecialEventCommand(group.Key));
                }
            }

            return commands;
        }

        private static IEnumerable<IPlayerCommand> ContingencyPlannerTakeCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Role != Role.ContingencyPlanner
                || game.CurrentPlayer.ActionsRemaining == 0
                || game.PhaseOfTurn != TurnPhase.DoActions) yield break;

            if (game.ContingencyPlannerStoredCard != null) yield break;

            foreach (var card in game.PlayerDiscardPile.Cards
                         .Where(c => c is ISpecialEventCard).Cast<ISpecialEventCard>())
            {
                yield return new ContingencyPlannerTakeEventCardCommand(card);
            }
        }

        private static IEnumerable<IPlayerCommand> ScientistCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Role != Role.Scientist
                || game.CurrentPlayer.ActionsRemaining == 0
                || game.PhaseOfTurn != TurnPhase.DoActions) yield break;

            if (!game.CityByName(game.CurrentPlayer.Location).HasResearchStation) yield break;

            foreach (var cureCards in game.CurrentPlayer.Hand
                         .CityCards
                         .GroupBy(c => c.City.Colour)
                         .Where(g => g.Count() >= 4))
            {
                if (!game.IsCured(cureCards.Key))
                    // todo: yield all combinations if > 4 cards
                    yield return new ScientistDiscoverCureCommand(cureCards.Take(4).ToArray());
            }
        }

        private static IEnumerable<IPlayerCommand> ResearcherCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.ActionsRemaining == 0
                || game.PhaseOfTurn != TurnPhase.DoActions) yield break;

            if (!game.Players.Any(p => p.Role == Role.Researcher)) yield break;

            var researcher = game.PlayerByRole(Role.Researcher);

            foreach (var card in researcher.Hand.CityCards)
            {
                if (game.CurrentPlayer.Role == Role.Researcher)
                {
                    foreach (var otherPlayer in game.Players.Where(p =>
                                 p.Role != Role.Researcher && p.Location == researcher.Location))
                    {

                        yield return new ResearcherShareKnowledgeGiveCommand(otherPlayer.Role, card.City.Name);
                    }
                }
                else if (game.CurrentPlayer.Location == researcher.Location)
                    yield return new ShareKnowledgeTakeFromResearcherCommand(game.CurrentPlayer.Role, card.City.Name);
            }
        }

        private static IEnumerable<IPlayerCommand> OperationsExpertCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Role != Role.OperationsExpert
                || game.CurrentPlayer.ActionsRemaining == 0
                || game.PhaseOfTurn != TurnPhase.DoActions) yield break;

            var opex = (OperationsExpert)game.PlayerByRole(Role.OperationsExpert);
            var city = game.CityByName(opex.Location);

            if (!city.HasResearchStation && game.ResearchStationPile > 0)
                yield return new OperationsExpertBuildResearchStation();

            if (city.HasResearchStation && !opex.HasUsedDiscardAndMoveAbilityThisTurn)
            {
                foreach (var card in opex.Hand.CityCards)
                {
                    foreach (var city2 in game.Cities.Where(c => c.Name != opex.Location))
                    {
                        yield return new OperationsExpertDiscardToMoveFromStation(card, city2.Name);
                    }
                }
            }
        }

        private IEnumerable<IPlayerCommand> DispatcherCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Role != Role.Dispatcher
                || game.CurrentPlayer.ActionsRemaining == 0
                || game.PhaseOfTurn != TurnPhase.DoActions) yield break;

            foreach (var x in DispatcherMovePawnToOtherPawns(game)) yield return x;
            foreach (var x in DispatcherDriveFerryCommands(game)) yield return x;
            foreach (var x in DispatcherDirectFlightCommands(game)) yield return x;
            foreach (var x in DispatcherCharterFlightCommands(game)) yield return x;
            foreach (var x in DispatcherShuttleFlightCommands(game)) yield return x;
        }

        private static IEnumerable<IPlayerCommand> DispatcherShuttleFlightCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players)
            {
                if (otherPlayer.Role == Role.Dispatcher) continue;

                if (game.CityByName(otherPlayer.Location).HasResearchStation)
                {
                    foreach (var city in game.Cities.Where(c => c.Name != otherPlayer.Location && c.HasResearchStation))
                    {
                        yield return new DispatcherShuttleFlyPawnCommand(otherPlayer.Role, city.Name);
                    }
                }
            }
        }

        private static IEnumerable<IPlayerCommand> DispatcherCharterFlightCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players)
            {
                if (otherPlayer.Role == Role.Dispatcher) continue;

                foreach (var card in game.PlayerByRole(Role.Dispatcher).Hand.CityCards)
                {
                    if (otherPlayer.Location != card.City.Name) continue;

                    foreach (var city in game.Cities.Where(city => city.Name != otherPlayer.Location))
                    {
                        yield return new DispatcherCharterFlyPawnCommand(otherPlayer.Role, city.Name);
                    }
                }
            }
        }

        private static IEnumerable<IPlayerCommand> DispatcherDirectFlightCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players)
            {
                if (otherPlayer.Role == Role.Dispatcher) continue;

                foreach (var card in game.PlayerByRole(Role.Dispatcher).Hand.CityCards)
                {
                    if (otherPlayer.Location != card.City.Name)
                        yield return new DispatcherDirectFlyPawnCommand(otherPlayer.Role, card.City.Name);
                }
            }
        }

        private static IEnumerable<IPlayerCommand> DispatcherDriveFerryCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players)
            {
                if (otherPlayer.Role == Role.Dispatcher) continue;

                foreach (var adjacentCity in game.Board.AdjacentCities[otherPlayer.Location])
                {
                    yield return new DispatcherDriveFerryPawnCommand(otherPlayer.Role, adjacentCity);
                }
            }
        }

        private static IEnumerable<IPlayerCommand> DispatcherMovePawnToOtherPawns(PandemicGame game)
        {
            foreach (var player1 in game.Players)
            {
                foreach (var player2 in game.Players)
                {
                    if (player1 == player2) continue;
                    if (player1.Location == player2.Location) continue;

                    yield return new DispatcherMovePawnToOtherPawnCommand(player1.Role, player2.Role);
                }
            }
        }

        private static IEnumerable<IPlayerCommand> SpecialEventCommands(PandemicGame game)
        {
            if (game.SpecialEventWasRecentlySkipped) yield break;
            if (game.PhaseOfTurn == TurnPhase.Epidemic) yield break;

            foreach (var playerWithCard in game.Players)
            {
                var cards = playerWithCard.Hand.Where(c => c is ISpecialEventCard).ToList();
                if (playerWithCard.Role == Role.ContingencyPlanner)
                {
                    var planner = (ContingencyPlanner)playerWithCard;
                    if (planner.StoredEventCard != null)
                        cards.Add((PlayerCard)planner.StoredEventCard);
                }

                foreach (var card in cards)
                {
                    if (card is GovernmentGrantCard)
                        foreach (var x in GovernmentGrants(game, playerWithCard)) yield return x;
                    else if (card is EventForecastCard)
                        foreach (var x in EventForecasts(game, playerWithCard)) yield return x;
                    else if (card is AirliftCard)
                        foreach (var x in Airlifts(game, playerWithCard)) yield return x;
                    else if (card is ResilientPopulationCard)
                        foreach (var x in ResilientPopulations(game, playerWithCard)) yield return x;
                    else if (card is OneQuietNightCard)
                        foreach (var x in OneQuietNights(game, playerWithCard)) yield return x;
                }
            }
        }

        private static IEnumerable<IPlayerCommand> SensibleSpecialEventCommands(PandemicGame game)
        {
            if (game.SpecialEventWasRecentlySkipped) yield break;
            if (game.PhaseOfTurn == TurnPhase.Epidemic) yield break;

            foreach (var playerWithCard in game.Players)
            {
                var cards = playerWithCard.Hand.Where(c => c is ISpecialEventCard).ToList();
                if (playerWithCard.Role == Role.ContingencyPlanner)
                {
                    var planner = (ContingencyPlanner)playerWithCard;
                    if (planner.StoredEventCard != null)
                        cards.Add((PlayerCard)planner.StoredEventCard);
                }

                foreach (var card in cards)
                {
                    if (card is GovernmentGrantCard)
                        foreach (var x in GovernmentGrants(game, playerWithCard)) yield return x;
                    else if (card is EventForecastCard)
                        foreach (var x in SensibleEventForecasts(game, playerWithCard)) yield return x;
                    else if (card is AirliftCard)
                        foreach (var x in Airlifts(game, playerWithCard)) yield return x;
                    else if (card is ResilientPopulationCard)
                        foreach (var x in ResilientPopulations(game, playerWithCard)) yield return x;
                    else if (card is OneQuietNightCard)
                        foreach (var x in OneQuietNights(game, playerWithCard)) yield return x;
                }
            }
        }

        private static IEnumerable<IPlayerCommand> OneQuietNights(PandemicGame game, Player playerWithCard)
        {
            yield return new OneQuietNightCommand(playerWithCard.Role);
        }

        private static IEnumerable<IPlayerCommand> ResilientPopulations(PandemicGame game, Player playerWithCard)
        {
            foreach (var infectionCard in game.InfectionDiscardPile.Cards)
            {
                yield return new ResilientPopulationCommand(playerWithCard.Role, infectionCard);
            }
        }

        private static IEnumerable<IPlayerCommand> Airlifts(PandemicGame game, Player playerWithCard)
        {
            foreach (var playerToAirlift in game.Players)
            {
                foreach (var city in game.Cities.Where(c => c.Name != playerToAirlift.Location))
                {
                    yield return new AirliftCommand(playerWithCard.Role, playerToAirlift.Role, city.Name);
                }
            }
        }

        private static IEnumerable<IPlayerCommand> EventForecasts(PandemicGame game, Player playerWithCard)
        {
            var perms = new Permutations<InfectionCard>(game.InfectionDrawPile.Top(6), new InfectionCardComparer());
            foreach (var perm in perms)
            {
                yield return new EventForecastCommand(playerWithCard.Role, perm);
            }
        }

        private static IEnumerable<IPlayerCommand> SensibleEventForecasts(PandemicGame game, Player playerWithCard)
        {
            // todo: write a test for this. Get the order right!
            var cards = game.InfectionDrawPile.Top(6)
                .OrderByDescending(card => game.CityByName(card.City).MaxNumCubes);

            yield return new EventForecastCommand(playerWithCard.Role, cards.ToArray());
        }

        private static IEnumerable<IPlayerCommand> GovernmentGrants(PandemicGame game, Player playerWithCard)
        {
            foreach (var city in game.Cities.Where(c => !c.HasResearchStation))
            {
                yield return new GovernmentGrantCommand(playerWithCard.Role, city.Name);
            }
        }

        private static IEnumerable<IPlayerCommand> PassCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                yield return new PassCommand(game.CurrentPlayer.Role);
            }
        }

        private static IEnumerable<IPlayerCommand> DiscardCommands(PandemicGame game)
        {
            if (game is { PhaseOfTurn: TurnPhase.DrawCards, CardsDrawn: 1 }) yield break;
            if (game.PhaseOfTurn is TurnPhase.Epidemic or TurnPhase.EpidemicIntensify) yield break;

            foreach (var player in game.Players)
            {
                if (player.Hand.Count <= 7) continue;

                foreach (var card in player.Hand)
                {
                    yield return new DiscardPlayerCardCommand(player.Role, card);
                }
            }
        }

        private static IEnumerable<IPlayerCommand> DriveFerryCommands(PandemicGame game)
        {
            foreach (var city in game.Board.AdjacentCities[game.CurrentPlayer.Location])
            {
                yield return new DriveFerryCommand(game.CurrentPlayer.Role, city);
            }
        }

        private static IEnumerable<IPlayerCommand> DirectFlightCommands(PandemicGame game)
        {
            foreach (var cityCard in game.CurrentPlayer.Hand.CityCards)
            {
                if (game.CurrentPlayer.Location != cityCard.City.Name)
                    yield return new DirectFlightCommand(game.CurrentPlayer.Role, cityCard.City.Name);
            }
        }

        private static IEnumerable<IPlayerCommand> CureCommands(PandemicGame game)
        {
            if (!game.CityByName(game.CurrentPlayer.Location).HasResearchStation) yield break;

            foreach (var cureCards in game.CurrentPlayer.Hand
                .CityCards
                .GroupBy(c => c.City.Colour)
                .Where(g => g.Count() >= 5))
            {
                if (!game.IsCured(cureCards.Key))
                    // todo: yield all combinations if > 5 cards
                    yield return new DiscoverCureCommand(game.CurrentPlayer.Role, cureCards.Take(5).ToArray());
            }
        }

        private static IEnumerable<IPlayerCommand> BuildResearchStationCommands(PandemicGame game)
        {
            if (game.ResearchStationPile == 0) yield break;

            if (CurrentPlayerCanBuildResearchStation(game))
                yield return new BuildResearchStationCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Location);
        }

        private static IEnumerable<IPlayerCommand> CharterFlightCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Hand.CityCards.All(c => c.City.Name != game.CurrentPlayer.Location)) yield break;

            foreach (var city in game
                         .Cities
                         .Select(c => c.Name)
                         .Except(new []{game.CurrentPlayer.Location}))
            {
                yield return new CharterFlightCommand(
                    game.CurrentPlayer.Role,
                    PlayerCards.CityCard(game.CurrentPlayer.Location),
                    city);
            }
        }

        private static IEnumerable<IPlayerCommand> ShuttleFlightCommands(PandemicGame game)
        {
            if (!game.CityByName(game.CurrentPlayer.Location).HasResearchStation) yield break;

            foreach (var city in game
                         .Cities
                         .Where(c => c.HasResearchStation)
                         .Select(c => c.Name)
                         .Except(new []{game.CurrentPlayer.Location}))
            {
                yield return new ShuttleFlightCommand(game.CurrentPlayer.Role, city);
            }
        }

        private static IEnumerable<IPlayerCommand> TreatDiseaseCommands(PandemicGame game)
        {
            var currentLocation = game.CurrentPlayer.Location;
            var nonZeroCubeColours = game
                .CityByName(currentLocation).Cubes.Counts
                .Where(kv => kv.Value > 0)
                .Select(kv => kv.Key);

            foreach (var colour in nonZeroCubeColours)
            {
                yield return new TreatDiseaseCommand(game.CurrentPlayer.Role, currentLocation, colour);
            }
        }

        private static IEnumerable<IPlayerCommand> ShareKnowledgeGiveCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players.Where(p =>
                         p != game.CurrentPlayer && p.Location == game.CurrentPlayer.Location))
            {
                foreach (var _ in game.CurrentPlayer.Hand.CityCards.Where(c => c.City.Name == game.CurrentPlayer.Location))
                {
                    yield return new ShareKnowledgeGiveCommand(
                        game.CurrentPlayer.Role,
                        game.CurrentPlayer.Location,
                        otherPlayer.Role);
                }
            }
        }

        private static IEnumerable<IPlayerCommand> ShareKnowledgeTakeCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players.Where(p =>
                         p != game.CurrentPlayer && p.Location == game.CurrentPlayer.Location))
            {
                foreach (var _ in otherPlayer.Hand.CityCards.Where(c => c.City.Name == game.CurrentPlayer.Location))
                {
                    yield return new ShareKnowledgeTakeCommand(
                        game.CurrentPlayer.Role,
                        game.CurrentPlayer.Location,
                        otherPlayer.Role);
                }
            }
        }

        private static bool CurrentPlayerCanBuildResearchStation(PandemicGame game)
        {
            if (game.CityByName(game.CurrentPlayer.Location).HasResearchStation)
                return false;

            return game.CurrentPlayer.Hand.CityCards.Any(c => c.City.Name == game.CurrentPlayer.Location);
        }
    }

    /// <summary>
    /// Needed for generating permutations of event forecasts
    /// </summary>
    internal class InfectionCardComparer : IComparer<InfectionCard>
    {
        public int Compare(InfectionCard? x, InfectionCard? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return string.Compare(x.City, y.City, StringComparison.Ordinal);
        }
    }
}

