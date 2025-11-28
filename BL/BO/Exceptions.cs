namespace BO;
 [Serializable]
 public class BLNotNullableException : Exception
 {
     public BLNotNullableException() : base() { }

     /// <summary>Initializes a new instance with a specified error message.</summary>
     public BLNotNullableException(string? message) : base(message) { }

     /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
     public BLNotNullableException(string? message, Exception? innerException) : base(message, innerException) { }
}

[Serializable]
public class BLInvalidDataException : Exception
{
    public BLInvalidDataException() : base() { }

    /// <summary>Initializes a new instance with a specified error message.</summary>
    public BLInvalidDataException(string? message) : base(message) { }

    /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
    public BLInvalidDataException(string? message, Exception? innerException) : base(message, innerException) { }
}

[Serializable]
public class BLGeneralException : Exception
{
    public BLGeneralException() : base() { }

    /// <summary>Initializes a new instance with a specified error message.</summary>
    public BLGeneralException(string? message) : base(message) { }

    /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
    public BLGeneralException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a requested entity is not found in the system (Logical lookups).
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
/// </summary>
[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException() : base() { }
    public BlAlreadyExistsException(string? message) : base(message) { }
    public BlAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException) { }
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

[Serializable]
public class BLAccessPermission : Exception
{
    public BLAccessPermission() : base() { }
    public BLAccessPermission(string? message) : base(message) { }
    public BLAccessPermission(string? message, Exception? innerException) : base(message, innerException) { }
}




