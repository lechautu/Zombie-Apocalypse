namespace GameFx.Core
{

    public struct Result<T>
    {
        public bool IsSuccess { get; }
        public T Payload { get; }
        public string ErrorMessage { get; }

        private Result(bool isSuccess, T payload, string errorMessage)
        {
            IsSuccess = isSuccess;
            Payload = payload;
            ErrorMessage = errorMessage;
        }

        public static Result<T> Success(T payload)
        {
            return new Result<T>(true, payload, null);
        }

        public static Result<T> Failure(string errorMessage)
        {
            return new Result<T>(false, default, errorMessage);
        }
    }
}