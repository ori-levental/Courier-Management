using BlApi;

namespace BlImplementation;

/// <summary>
/// Main implementation of the Business Logic layer interface (IBl).
/// Acts as a centralized entry point to access all specific BL functionalities.
/// </summary>
internal class Bl : IBl
{
    /// <summary>
    /// Access to Courier-related logic operations.
    /// </summary>
    public ICourier Courier { get; } = new CourierImplementation();

    /// <summary>
    /// Access to Order-related logic operations.
    /// </summary>
    public IOrder Order { get; } = new OrderImplementation();

    /// <summary>
    /// Access to Administrative and System Configuration operations.
    /// </summary>
    public IAdmin Admin { get; } = new AdminImplementation();
}