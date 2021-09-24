using System;

namespace pandemic.Values
{
    public class InvalidActionException : Exception
    {
        public InvalidActionException(string message): base(message)
        {
        }
    }
}
