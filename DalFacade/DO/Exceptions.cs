namespace DO;

/// <summary>
/// Exception thrown when the requested entity is not found (Read, Update, Delete).
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException() : base() { }

    /// <summary>Initializes a new instance with a specified error message.</summary>
    public DalDoesNotExistException(string? message) : base(message) { }

    /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
    public DalDoesNotExistException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when attempting to create a new entity whose ID already exists in the data source.
/// </summary>
[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException() : base() { }

    /// <summary>Initializes a new instance with a specified error message.</summary>
    public DalAlreadyExistsException(string? message) : base(message) { }

    /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
    public DalAlreadyExistsException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a physical error occurs during file loading, saving, or creation (XML I/O).
/// </summary>
[Serializable]
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException() : base() { }

    /// <summary>Initializes a new instance with a specified error message.</summary>
    public DalXMLFileLoadCreateException(string? message) : base(message) { }

    /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
    public DalXMLFileLoadCreateException(string? message, Exception? innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an error occurs during the dynamic loading or parsing of the external configuration file.
/// </summary>
[Serializable]
public class DalConfigException : Exception
{
    public DalConfigException() : base() { }

    /// <summary>Initializes a new instance with a specified error message.</summary>
    public DalConfigException(string? message) : base(message) { }

    /// <summary>Initializes a new instance, wrapping an inner exception.</summary>
    public DalConfigException(string? message, Exception? innerException) : base(message, innerException) { }
}