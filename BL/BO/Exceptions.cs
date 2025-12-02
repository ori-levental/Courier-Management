namespace BO;

[Serializable]
public class BlGeneralException : Exception
{
    public BlGeneralException() : base() { }
    public BlGeneralException(string? message) : base(message) { }
    public BlGeneralException(string? message, Exception? innerException) : base(message, innerException) { }
}

#region DAL Parallel Exceptions
// These exceptions correspond to DO exceptions and are intended to wrap them when the DAL throws an error.

/// <summary>
/// Exception thrown when a requested entity is not found in the system (Logical lookups).
/// Wraps DalDoesNotExistException.
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException() : base() { }
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when attempting to add an entity that already exists (Logical duplication).
/// Wraps DalAlreadyExistsException.
/// </summary>
[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException() : base() { }
    public BlAlreadyExistsException(string? message) : base(message) { }
    public BlAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException) { }
}

#endregion DAL Parallel Exceptions

#region Data Validation Exceptions
// Exceptions thrown due to issues within the data itself (invalid format, null values, etc.).

[Serializable]
public class BlNotNullableException : Exception
{
    public BlNotNullableException() : base() { }
    public BlNotNullableException(string? message) : base(message) { }
    public BlNotNullableException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when data validation fails (e.g., negative ID, invalid email, wrong password format).
/// </summary>
[Serializable]
public class BlInvalidDataException : Exception
{
    public BlInvalidDataException() : base() { }
    public BlInvalidDataException(string? message) : base(message) { }
    public BlInvalidDataException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a required property is null or empty.
/// </summary>
[Serializable]
public class BlNullPropertyException : Exception
{
    public BlNullPropertyException() : base() { }
    public BlNullPropertyException(string? message) : base(message) { }
    public BlNullPropertyException(string? message, Exception? innerException) : base(message, innerException) { }
}

#endregion Data Validation Exceptions

#region Business Logic Constraints
// Exceptions thrown when data is technically valid, but the operation is forbidden by business logic.

/// <summary>
/// Exception thrown when an entity cannot be deleted due to logical constraints (e.g., Courier has active orders).
/// </summary>
[Serializable]
public class BlDeletionImpossibleException : Exception
{
    public BlDeletionImpossibleException() : base() { }
    public BlDeletionImpossibleException(string? message) : base(message) { }
    public BlDeletionImpossibleException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when the user does not have the required permissions to perform an action.
/// </summary>
[Serializable]
public class BlAccessPermission : Exception
{
    public BlAccessPermission() : base() { }
    public BlAccessPermission(string? message) : base(message) { }
    public BlAccessPermission(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an order cannot be canceled (e.g., it has already been delivered).
/// </summary>
[Serializable]
public class BlCannotCancel : Exception
{
    public BlCannotCancel() : base() { }
    public BlCannotCancel(string? message) : base(message) { }
    public BlCannotCancel(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an order/delivery cannot be closed/completed (e.g., wrong courier, already closed).
/// </summary>
[Serializable]
public class BlCannotClose : Exception
{
    public BlCannotClose() : base() { }
    public BlCannotClose(string? message) : base(message) { }
    public BlCannotClose(string? message, Exception? innerException) : base(message, innerException) { }
}

#endregion Business Logic Constraints