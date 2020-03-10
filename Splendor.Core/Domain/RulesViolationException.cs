using System;

namespace Splendor.Core
{
    public class RulesViolationException : Exception
    {
        public RulesViolationException() : this("This breaks the rules somehow")
        {
        }

        public RulesViolationException(string message) : base(message)
        {            
        }

        public RulesViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
