using System.Collections.Generic;
using System.Linq;

namespace pandemic.Values;

/// <summary>
/// A deck of cards. Immutable.
/// </summary>
public class Deck<T>
{
    private readonly List<T> _cards;

    public int Count => _cards.Count;
    public IEnumerable<T> Cards => _cards.Select(c => c);
    public T TopCard => _cards.Last();
    public static Deck<T> Empty => new();
    public T BottomCard => _cards.First();

    private Deck()
    {
        _cards = new List<T>();
    }

    public Deck(IEnumerable<T> cards)
    {
        _cards = cards.Select(c => c).ToList();
    }

    public bool IsSameAs(Deck<T> otherDeck)
    {
        if (Count != otherDeck.Count) return false;

        foreach (var (card, other) in _cards.Zip(otherDeck.Cards))
        {
            if (!card!.Equals(other)) return false;
        }

        return true;
    }

    public IEnumerable<T> Top(int numCards)
    {
        return _cards.TakeLast(numCards);
    }

    public (Deck<T>, T) Draw()
    {
        return (new Deck<T>(_cards.Take(_cards.Count - 1)), TopCard);
    }

    public (Deck<T> newDrawPile, IEnumerable<T> cards) Draw(int numCards)
    {
        return (new Deck<T>(_cards.Take(_cards.Count - numCards)), Top(numCards));
    }

    /// <summary>
    /// Place the given cards onto the top of the deck, in 'left to right' order,
    /// ie. the last card given is at the top of the deck afterwards.
    /// </summary>
    public Deck<T> PlaceOnTop(IEnumerable<T> cards)
    {
        return new Deck<T>(_cards.Concat(cards));
    }

    /// <summary>
    /// Place the given cards onto the top of the deck, in 'left to right' order,
    /// ie. the last card given is at the top of the deck afterwards.
    /// </summary>
    public Deck<T> PlaceOnTop(params T[] cards)
    {
        return new Deck<T>(_cards.Concat(cards));
    }

    public Deck<T> PlaceOnTop(T card)
    {
        return new Deck<T>(_cards.Concat(new[] { card }));
    }
}
