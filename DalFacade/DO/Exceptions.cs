namespace DO;

[Serializable]
// Exception handling non-existent object
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException()
    {
    }

    public DalDoesNotExistException(string? message) : base(message) { }
}
// Exception handling already-existent object
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException()
    {
    }

    public DalAlreadyExistsException(string? message) : base(message) { }
}