namespace pandemic.Values;

using System;

/// <summary>
/// Player tries to do something not only not in the rules, but absurd. Eg.
/// tries to build a city in "ASDFASDFASDFA"
/// </summary>
public class InvalidActionException : Exception
{
    public InvalidActionException() { }

    public InvalidActionException(string message, Exception innerException)
        : base(message, innerException) { }

    public InvalidActionException(string message) : base(message)
    {
    }
}
