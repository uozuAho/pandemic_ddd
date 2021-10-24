using System;
using System.Linq;

namespace pandemic.client
{
    class RandomBot
    {
        private readonly Random _rng = new();

        public int Step(ISpielState state)
        {
            var actions = state.LegalActions().ToList();
            var randomIdx = _rng.Next(actions.Count);
            return actions[randomIdx];
        }
    }
}
