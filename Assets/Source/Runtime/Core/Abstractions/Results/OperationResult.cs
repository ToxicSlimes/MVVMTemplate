namespace Core.Abstractions.Results
{
    public readonly struct OperationResult
    {
        public OperationResult(bool isSuccess, string message)
        {
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        public bool IsSuccess { get; }
        public string Message { get; }

        public static OperationResult Success(string message = "") => new(true, message);
        public static OperationResult Failure(string message) => new(false, message);
    }

    public readonly struct OperationResult<T>
    {
        public OperationResult(T value, bool isSuccess, string message)
        {
            Value = value;
            IsSuccess = isSuccess;
            Message = message ?? string.Empty;
        }

        public T Value { get; }
        public bool IsSuccess { get; }
        public string Message { get; }

        public static OperationResult<T> Success(T value, string message = "") => new(value, true, message);
        public static OperationResult<T> Failure(string message) => new(default, false, message);

        public static implicit operator OperationResult(OperationResult<T> result) =>
            new(result.IsSuccess, result.Message);
    }
}
