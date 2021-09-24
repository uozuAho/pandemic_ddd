using System.Collections.Generic;
using System.Linq;
using pandemic.Values;

namespace pandemic.Events
{
    public record InfectionDeckSetUp : IEvent
    {
        public List<InfectionCard> Deck { get; }

        public InfectionDeckSetUp(IEnumerable<InfectionCard> deck)
        {
            Deck = deck.ToList();
        }
    }
}
