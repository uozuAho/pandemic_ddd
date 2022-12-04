using System;
using System.Collections.Generic;
using System.Linq;
using pandemic.Aggregates;
using pandemic.Aggregates.Game;

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
            }

            return new ArraySegment<PlayerCommand>(_buffer, 0, _bufIdx);
        }

        private void SetDiscardCommands(PandemicGame game)
        {
            if (game.CurrentPlayer.Hand.Count > 7)
            {
                foreach (var card in game.CurrentPlayer.Hand)
                {
                    _buffer[_bufIdx++] = new DiscardPlayerCardCommand(card);
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
                    _buffer[_bufIdx++] = new DiscoverCureCommand(cureCards.Take(5).ToArray());
            }
        }

        private void SetBuildResearchStationCommands(PandemicGame game)
        {
            if (game.ResearchStationPile == 0) return;

            if (CurrentPlayerCanBuildResearchStation(game))
                _buffer[_bufIdx++] = new BuildResearchStationCommand(game.CurrentPlayer.Location);
        }

        private static bool CurrentPlayerCanBuildResearchStation(PandemicGame game)
        {
            if (game.CityByName(game.CurrentPlayer.Location).HasResearchStation)
                return false;

            return game.CurrentPlayer.Hand.CityCards.Any(c => c.City.Name == game.CurrentPlayer.Location);
        }
    }
}
