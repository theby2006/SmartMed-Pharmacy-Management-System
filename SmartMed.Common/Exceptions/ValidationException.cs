using System;

namespace SmartMed.Common.Exceptions
{
    [Serializable]
    public class ValidationException : AppException
    {
        public ValidationException(string message)
            : base(message)
        {
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
