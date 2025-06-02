namespace pandemic.Values;

using System;

/// <summary>
/// Player attempts to do an action that is valid, but against the rules.
/// </summary>
public class GameRuleViolatedException : Exception
{
    public GameRuleViolatedException() { }

    public GameRuleViolatedException(string message, Exception innerException)
        : base(message, innerException) { }

    public GameRuleViolatedException(string message) : base(message)
    {
    }
}
