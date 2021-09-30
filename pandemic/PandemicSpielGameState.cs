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

        public PandemicSpielGameState(PandemicGame game)
        {
            _game = game;
        }

        public bool IsTerminal => _game.IsOver;

        public IEnumerable<int> LegalActions()
        {
            // todo: how to map from meaningful actions to ints? Looks like OpenSpiel
            // wants a way to do this too, see https://github.com/deepmind/open_spiel/blob/master/docs/contributing.md
            // point 'Structured Action Spaces'
            yield return 0;
        }

        public string ActionToString(int currentPlayer, int action)
        {
            return "todo: implement me";
        }

        public void ApplyAction(int action)
        {
        }
    }
}
