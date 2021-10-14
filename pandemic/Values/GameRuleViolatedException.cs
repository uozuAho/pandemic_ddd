using System;

namespace pandemic.Values
{
    /// <summary>
    /// Player attempts to do an action that is valid, but against the rules.
    /// </summary>
    public class GameRuleViolatedException : Exception
    {
        public GameRuleViolatedException(string message) : base(message)
        {
        }
    }
}
