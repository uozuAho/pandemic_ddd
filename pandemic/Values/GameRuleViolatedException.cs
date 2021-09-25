using System;

namespace pandemic.Values
{
    public class GameRuleViolatedException : Exception
    {
        public GameRuleViolatedException(string message) : base(message)
        {
        }
    }
}
