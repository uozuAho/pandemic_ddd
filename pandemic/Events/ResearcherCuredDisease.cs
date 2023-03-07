using pandemic.Values;

namespace pandemic.Events;

internal record ResearcherCuredDisease(PlayerCityCard[] Cards) : IEvent;
