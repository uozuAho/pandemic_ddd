﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace pandemic.Values;

/// <summary>
/// A deck of cards. Immutable.
/// </summary>
public class Deck<T>
{
    /// <summary>
    /// The deck, in bottom to top order
    /// Index 0 | ---- bottom     top -----X highest index.
    /// </summary>
    private readonly List<T> _cards;

    public int Count => _cards.Count;

    /// <summary>
    /// Get the cards in the deck, in bottom to top order. Ie. the first card (index 0) is the bottom of the deck.
    /// </summary>
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

    public IEnumerable<T> Bottom(int numCards)
    {
        return _cards.Take(numCards);
    }

    public (Deck<T>, T) Draw()
    {
        return (new Deck<T>(_cards.Take(_cards.Count - 1)), TopCard);
    }

    public (Deck<T>, T) DrawFromBottom()
    {
        return (new Deck<T>(_cards.Skip(1)), BottomCard);
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

    public Deck<T> PlaceAtBottom(T card)
    {
        return new Deck<T>(new[] { card }.Concat(_cards));
    }

    public Deck<T> Remove(T card)
    {
        var newDeck = new Deck<T>(_cards.Where(c => c is not null && !c.Equals(card)));

        if (newDeck.Count == Count) throw new InvalidOperationException($"Deck does not contain {card}");

        return newDeck;
    }

    public Deck<T> Remove(IEnumerable<T> cards)
    {
        var set = cards.ToHashSet();
        var toRemove = _cards.Where(c => set.Contains(c)).ToHashSet();

        if (toRemove.Count < set.Count)
        {
            var missingCards = _cards.Except(set);

            throw new InvalidOperationException($"Deck does not contain {string.Join(',', missingCards)}");
        }

        return new Deck<T>(_cards.Where(c => !toRemove.Contains(c)));
    }

    public Deck<T> RemoveIfPresent(T card)
    {
        return new Deck<T>(_cards.Where(c => c is not null && !c.Equals(card)));
    }

    public Deck<T> RemoveIfPresent(IEnumerable<T> cards)
    {
        var set = new HashSet<T>(cards);
        return new Deck<T>(_cards.Where(c => c is not null && !set.Contains(c)));
    }
}
