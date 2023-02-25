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
        private readonly IPlayerCommand[] _buffer = new IPlayerCommand[2000];
        private int _bufIdx = 0;

        public IEnumerable<IPlayerCommand> LegalCommands(PandemicGame game)
        {
            if (game.IsOver) return Enumerable.Empty<IPlayerCommand>();

            _bufIdx = 0;

            SetDiscardCommands(game);
            SetSpecialEventCommands(game);

            if (game.APlayerMustDiscard)
                return new ArraySegment<IPlayerCommand>(_buffer, 0, _bufIdx);

            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                SetDriveFerryCommands(game);
                SetBuildResearchStationCommands(game);
                SetCureCommands(game);
                SetDirectFlightCommands(game);
                SetCharterFlightCommands(game);
                SetShuttleFlightCommands(game);
                SetTreatDiseaseCommands(game);
                SetShareKnowledgeGiveCommands(game);
                SetShareKnowledgeTakeCommands(game);
                SetPassCommands(game);
            }

            return new ArraySegment<IPlayerCommand>(_buffer, 0, _bufIdx);
        }

        private void SetSpecialEventCommands(PandemicGame game)
        {
            foreach (var player in game.Players)
            {
                foreach (var card in player.Hand.Where(c => c is ISpecialEventCard))
                {
                    if (card is GovernmentGrantCard)
                    {
                        foreach (var city in game.Cities.Where(c => !c.HasResearchStation))
                        {
                            _buffer[_bufIdx++] = new GovernmentGrantCommand(player.Role, city.Name);
                        }
                    }

                    if (card is EventForecastCard)
                    {
                        var perms = new Permutations<InfectionCard>(game.InfectionDrawPile.Top(6), new InfectionCardComparer());
                        foreach (var perm in perms)
                        {
                            _buffer[_bufIdx++] = new EventForecastCommand(player.Role, perm.ToImmutableList());
                        }
                    }
                }
            }
        }

        private void SetPassCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                _buffer[_bufIdx++] = new PassCommand(game.CurrentPlayer.Role);
            }
        }

        public static IEnumerable<IPlayerCommand> AllPossibleCommands(PandemicGame game)
        {
            foreach (var city in game.Cities)
            {
                foreach (var player in game.Players)
                {
                    yield return new DiscardPlayerCardCommand(player.Role, PlayerCards.CityCard(city.Name));
                    yield return new BuildResearchStationCommand(player.Role, city.Name);
                    yield return new DriveFerryCommand(player.Role, city.Name);
                    yield return new DirectFlightCommand(player.Role, city.Name);
                    yield return new CharterFlightCommand(player.Role, PlayerCards.CityCard(player.Location), city.Name);
                    yield return new ShuttleFlightCommand(player.Role, city.Name);
                    foreach (var colour in ColourExtensions.AllColours)
                    {
                        yield return new TreatDiseaseCommand(player.Role, city.Name, colour);
                    }

                }
            }
            foreach (var player in game.Players)
            {
                yield return new PassCommand(player.Role);

                foreach (var cardsToCure in player.Hand.CityCards
                             .GroupBy(c => c.City.Colour)
                             .Where(g => g.Count() >= 5))
                {
                    yield return new DiscoverCureCommand(player.Role, cardsToCure.Select(g => g).ToArray());
                }

                foreach (var card in player.Hand.CityCards)
                {
                    foreach (var otherPlayer in game.Players)
                    {
                        yield return new ShareKnowledgeGiveCommand(player.Role, card.City.Name, otherPlayer.Role);
                        yield return new ShareKnowledgeTakeCommand(player.Role, card.City.Name, otherPlayer.Role);
                    }
                }
                foreach (var city in game.Cities)
                {
                    yield return new GovernmentGrantCommand(player.Role, city.Name);
                }
            }
        }

        private void SetDiscardCommands(PandemicGame game)
        {
            foreach (var player in game.Players)
            {
                if (player.Hand.Count <= 7) continue;

                foreach (var card in player.Hand)
                {
                    _buffer[_bufIdx++] = new DiscardPlayerCardCommand(player.Role, card);
                }
            }
        }

        private void SetDriveFerryCommands(PandemicGame game)
        {
            foreach (var city in game.Board.AdjacentCities[game.CurrentPlayer.Location])
            {
                _buffer[_bufIdx++] = new DriveFerryCommand(game.CurrentPlayer.Role, city);
            }
        }

        private void SetDirectFlightCommands(PandemicGame game)
        {
            foreach (var cityCard in game.CurrentPlayer.Hand.CityCards)
            {
                if (game.CurrentPlayer.Location != cityCard.City.Name)
                    _buffer[_bufIdx++] = new DirectFlightCommand(game.CurrentPlayer.Role, cityCard.City.Name);
            }
        }

        private void SetCureCommands(PandemicGame game)
        {
            if (!game.CityByName(game.CurrentPlayer.Location).HasResearchStation) return;

            foreach (var cureCards in game.CurrentPlayer.Hand
                .CityCards
                .GroupBy(c => c.City.Colour)
                .Where(g => g.Count() >= 5))
            {
                if (!game.IsCured(cureCards.Key))
                    // todo: yield all combinations if > 5 cards
                    _buffer[_bufIdx++] = new DiscoverCureCommand(game.CurrentPlayer.Role, cureCards.Take(5).ToArray());
            }
        }

        private void SetBuildResearchStationCommands(PandemicGame game)
        {
            if (game.ResearchStationPile == 0) return;

            if (CurrentPlayerCanBuildResearchStation(game))
                _buffer[_bufIdx++] = new BuildResearchStationCommand(game.CurrentPlayer.Role, game.CurrentPlayer.Location);
        }

        private void SetCharterFlightCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Hand.CityCards.All(c => c.City.Name != game.CurrentPlayer.Location)) return;

            foreach (var city in game
                         .Cities
                         .Select(c => c.Name)
                         .Except(new []{game.CurrentPlayer.Location}))
            {
                _buffer[_bufIdx++] = new CharterFlightCommand(
                    game.CurrentPlayer.Role,
                    PlayerCards.CityCard(game.CurrentPlayer.Location),
                    city);
            }
        }

        private void SetShuttleFlightCommands(PandemicGame game)
        {
            if (!game.CityByName(game.CurrentPlayer.Location).HasResearchStation) return;

            foreach (var city in game
                         .Cities
                         .Where(c => c.HasResearchStation)
                         .Select(c => c.Name)
                         .Except(new []{game.CurrentPlayer.Location}))
            {
                _buffer[_bufIdx++] = new ShuttleFlightCommand(game.CurrentPlayer.Role, city);
            }
        }

        private void SetTreatDiseaseCommands(PandemicGame game)
        {
            var currentLocation = game.CurrentPlayer.Location;
            var nonZeroCubeColours = game
                .CityByName(currentLocation).Cubes.Counts()
                .Where(kv => kv.Value > 0)
                .Select(kv => kv.Key);

            foreach (var colour in nonZeroCubeColours)
            {
                _buffer[_bufIdx++] = new TreatDiseaseCommand(game.CurrentPlayer.Role, currentLocation, colour);
            }
        }

        private void SetShareKnowledgeGiveCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players.Where(p =>
                         p != game.CurrentPlayer && p.Location == game.CurrentPlayer.Location))
            {
                foreach (var _ in game.CurrentPlayer.Hand.CityCards.Where(c => c.City.Name == game.CurrentPlayer.Location))
                {
                    _buffer[_bufIdx++] = new ShareKnowledgeGiveCommand(
                        game.CurrentPlayer.Role,
                        game.CurrentPlayer.Location,
                        otherPlayer.Role);
                }
            }
        }

        private void SetShareKnowledgeTakeCommands(PandemicGame game)
        {
            foreach (var otherPlayer in game.Players.Where(p =>
                         p != game.CurrentPlayer && p.Location == game.CurrentPlayer.Location))
            {
                foreach (var _ in otherPlayer.Hand.CityCards.Where(c => c.City.Name == game.CurrentPlayer.Location))
                {
                    _buffer[_bufIdx++] = new ShareKnowledgeTakeCommand(
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
    class InfectionCardComparer : IComparer<InfectionCard>
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

