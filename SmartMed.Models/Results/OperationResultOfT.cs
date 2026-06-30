namespace SmartMed.Models.Results
{
    public class OperationResult<T> : OperationResult
    {
        private OperationResult(bool isSuccess, string message, T data)
            : base(isSuccess, message)
        {
            Data = data;
        }

        public T Data { get; }

        public static OperationResult<T> Success(T data, string message = "Operation completed successfully.")
        {
            return new OperationResult<T>(true, message, data);
        }

        public static new OperationResult<T> Failure(string message)
        {
            return new OperationResult<T>(false, message, default(T));
        }
    }
}
