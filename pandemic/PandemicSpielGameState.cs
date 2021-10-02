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
        private PandemicGame _game;
        private readonly PlayerMoveGenerator _moveGenerator = new ();

        public PandemicSpielGameState(PandemicGame game)
        {
            _game = game;
        }

        public bool IsTerminal => _game.IsOver;

        public override string ToString()
        {
            return _game.ToString();
        }

        public IEnumerable<int> LegalActionsInt()
        {
            // todo: how to map from meaningful actions to ints? Looks like OpenSpiel
            // wants a way to do this too, see https://github.com/deepmind/open_spiel/blob/master/docs/contributing.md
            // point 'Structured Action Spaces'
            throw new NotImplementedException();
        }

        public IEnumerable<PlayerMove> LegalActions()
        {
            return _moveGenerator.LegalMoves(_game);
        }

        public string ActionToString(int currentPlayer, int action)
        {
            return "todo: implement me";
        }

        public void ApplyActionInt(int action)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IEvent> ApplyAction(PlayerMove action)
        {
            switch (action.MoveType)
            {
                case MoveType.DriveOrFerry:
                    IEnumerable<IEvent> events;
                    (_game, events) = _game.DriveOrFerryPlayer(action.Role, action.City);
                    return events;
                default:
                    throw new ArgumentOutOfRangeException($"Unsupported action: {action}");
            }
        }
    }
}
