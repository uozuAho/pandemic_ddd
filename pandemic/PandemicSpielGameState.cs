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
        private readonly PlayerMoveGenerator _moveGenerator = new ();

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
            return _moveGenerator.LegalMoves(Game);
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

            switch (action.MoveType)
            {
                case MoveType.DriveOrFerry:
                    (Game, events) = Game.DriveOrFerryPlayer(action.Role, action.City);
                    return events;
                case MoveType.Discard:
                    (Game, events) = Game.DiscardPlayerCard(action.Role, action.City);
                    return events;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported action: {action}");
            }
        }
    }
}
