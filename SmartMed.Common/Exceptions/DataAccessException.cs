using System;

namespace SmartMed.Common.Exceptions
{
    [Serializable]
    public class DataAccessException : AppException
    {
        public DataAccessException(string message)
            : base(message)
        {
        }

        public DataAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
