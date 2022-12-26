using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates.Game;
using pandemic.GameData;

namespace pandemic.Commands
{
    public class PlayerCommandGenerator
    {
        private readonly PlayerCommand[] _buffer = new PlayerCommand[100];
        private int _bufIdx = 0;

        public IEnumerable<PlayerCommand> LegalCommands(PandemicGame game)
        {
            if (game.IsOver) return Enumerable.Empty<PlayerCommand>();

            _bufIdx = 0;

            SetDiscardCommands(game);
            if (_bufIdx > 0)
                return new ArraySegment<PlayerCommand>(_buffer, 0, _bufIdx);

            if (game.CurrentPlayer.ActionsRemaining > 0)
            {
                SetDriveFerryCommands(game);
                SetBuildResearchStationCommands(game);
                SetCureCommands(game);
                SetDirectFlightCommands(game);
                SetCharterFlightCommands(game);
                SetShuttleFlightCommands(game);
            }

            return new ArraySegment<PlayerCommand>(_buffer, 0, _bufIdx);
        }

        private void SetDiscardCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Hand.Count > 7)
            {
                foreach (var card in game.CurrentPlayer.Hand)
                {
                    _buffer[_bufIdx++] = new DiscardPlayerCardCommand(game.CurrentPlayer.Role, card);
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
                if (!game.CureDiscovered[cureCards.Key])
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

        private static bool CurrentPlayerCanBuildResearchStation(PandemicGame game)
        {
            if (game.CityByName(game.CurrentPlayer.Location).HasResearchStation)
                return false;

            return game.CurrentPlayer.Hand.CityCards.Any(c => c.City.Name == game.CurrentPlayer.Location);
        }
    }
}
