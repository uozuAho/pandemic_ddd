using System;
using System.Collections.Generic;
using pandemic.Aggregates;

namespace pandemic
{
    /// <summary>
    /// Provides an OpenSpiel compatible interface for playing Pandemic
    /// </summary>
    public class PandemicSpielGameState
    {
        private readonly PandemicGame _game;
        private readonly PlayerMoveGenerator _moveGenerator = new ();

        public PandemicSpielGameState(PandemicGame game)
        {
            _game = game;
        }

        public bool IsTerminal => _game.IsOver;

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

        public void ApplyAction(PlayerMove action)
        {
        }
    }
}
