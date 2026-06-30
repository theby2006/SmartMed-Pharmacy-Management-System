using System;

namespace SmartMed.Common.Exceptions
{
    [Serializable]
    public class AuthenticationException : AppException
    {
        public AuthenticationException(string message)
            : base(message)
        {
        }

        public AuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
