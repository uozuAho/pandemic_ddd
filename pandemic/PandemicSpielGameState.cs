using System;
using System.Collections.Generic;
using pandemic.Aggregates;
using pandemic.Events;

namespace pandemic
{
    /// <summary>
    /// Provides an OpenSpiel compatible interface for playing Pandemic.
    /// Work in progress. See todos.
    /// </summary>
    public class PandemicSpielGameState
    {
        public PandemicGame Game;
        private readonly PlayerCommandGenerator _commandGenerator = new ();

        public PandemicSpielGameState(PandemicGame game)
        {
            Game = game;
        }

        public bool IsTerminal => Game.IsOver;

        public override string ToString()
        {
            return Game.ToString();
        }

        public IEnumerable<int> LegalActionsInt()
        {
            // todo: how to map from meaningful actions to ints? Looks like OpenSpiel
            // wants a way to do this too, see https://github.com/deepmind/open_spiel/blob/master/docs/contributing.md
            // point 'Structured Action Spaces'
            throw new NotImplementedException();
        }

        public IEnumerable<PlayerCommand> LegalActions()
        {
            return _commandGenerator.LegalCommands(Game);
        }

        public string ActionToString(int currentPlayer, int action)
        {
            return "todo: implement me";
        }

        public void ApplyActionInt(int action)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEvent> ApplyAction(PlayerCommand action)
        {
            IEnumerable<IEvent> events;

            switch (action)
            {
                case DriveFerryCommand command:
                    (Game, events) = Game.DriveOrFerryPlayer(command.Role, command.City);
                    return events;
                case DiscardPlayerCardCommand command:
                    (Game, events) = Game.DiscardPlayerCard(command.Card);
                    return events;
                case BuildResearchStationCommand command:
                    (Game, events) = Game.BuildResearchStation(command.City);
                    return events;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported action: {action}");
            }
        }
    }
}
