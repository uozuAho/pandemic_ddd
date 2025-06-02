namespace pandemic.agents;

using System.Collections.Generic;
using System.Linq;
using Values;

public class CardCounter
{
    /// <summary>
    /// Player cards that haven't been discarded
    /// </summary>
    public Dictionary<Colour, int> CardsAvailable = ColourExtensions.AllColours.ToDictionary(
        c => c,
        _ => 12
    );

    public CardCounter Clone()
    {
        return new CardCounter
        {
            CardsAvailable = CardsAvailable.Select(c => c).ToDictionary(c => c.Key, c => c.Value),
        };
    }
}
