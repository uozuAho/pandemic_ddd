using pandemic.Values;

namespace pandemic.Events
{
    internal record PlayerCardsDealt(Role Role, PlayerCard[] Cards) : IEvent;
}
