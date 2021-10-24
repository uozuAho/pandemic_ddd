using System.Collections.Generic;

namespace pandemic.client
{
    internal interface ISpielState
    {
        void ApplyAction(int action);
        IEnumerable<int> LegalActions();
        bool IsTerminal { get; }
    }
}
