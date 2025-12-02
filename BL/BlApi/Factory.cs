namespace BlApi;

/// <summary>
/// Factory class for creating instances of the Business Logic layer.
/// This is the only entry point for external layers (PL) to access the BL implementation.
/// </summary>
public static class Factory
{
    /// <summary>
    /// Creates and returns a new instance of the main BL interface.
    /// </summary>
    /// <returns>An object implementing IBl.</returns>
    public static IBl Get() => new BlImplementation.Bl();
}