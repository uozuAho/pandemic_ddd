using System;

namespace pandemic.Values
{
    /// <summary>
    /// Player tries to do something not only not in the rules, but absurd. Eg.
    /// tries to build a city in "ASDFASDFASDFA"
    /// </summary>
    public class InvalidActionException : Exception
    {
        public InvalidActionException(string message): base(message)
        {
        }
    }
}
