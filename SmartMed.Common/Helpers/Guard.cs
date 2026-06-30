using System;
using SmartMed.Common.Exceptions;

namespace SmartMed.Common.Helpers
{
    public static class Guard
    {
        public static void AgainstNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ValidationException($"Parameter '{parameterName}' cannot be null.");
            }
        }

        public static void AgainstNullOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ValidationException($"Parameter '{parameterName}' cannot be null, empty, or whitespace.");
            }
        }

        public static void AgainstNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ValidationException($"Parameter '{parameterName}' cannot be negative.");
            }
        }

        public static void AgainstZeroOrNegative(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ValidationException($"Parameter '{parameterName}' must be greater than zero.");
            }
        }

        public static void AgainstZeroOrNegative(decimal value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ValidationException($"Parameter '{parameterName}' must be greater than zero.");
            }
        }
    }
}
