namespace BO;

    [Serializable]
    public class BLTemporaryNotAvailableException : Exception
    {
        public BLTemporaryNotAvailableException() : base() { }

        /// <summary>Initializes a new instance with a specified error message.</summary>
        public BLTemporaryNotAvailableException(string? message) : base(message) { }

        /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
        public BLTemporaryNotAvailableException(string? message, Exception? innerException) : base(message, innerException) { }
    }

