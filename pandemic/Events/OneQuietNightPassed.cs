namespace pandemic.Events;

/// <summary>
/// Signifies a skipped infect cities phase, caused by using
/// the 'one quiet night' special event card.
/// </summary>
internal record OneQuietNightPassed : IEvent;
