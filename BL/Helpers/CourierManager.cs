using BO;
using DalApi;
using System.Net.Mail;

namespace Helpers;

/// <summary>
/// Logic helper for Courier operations (BL Layer).
/// Handles conversions, validation, CRUD operations, and periodic maintenance.
/// </summary>
internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Manages notifications to the PL layer.
    /// </summary>
    internal static ObserverManager Observers = new();

    /// <summary>
    /// Mutex used to prevent overlapping executions of the periodic maintenance task by the simulator.
    /// </summary>
    private static readonly Helpers.AsyncMutex s_periodicMutex = new();

    /// <summary>
    /// Mutex used to prevent overlapping executions of the simulation task by the simulator.
    /// </summary>
    private static readonly Helpers.AsyncMutex s_simulationMutex = new(); //stage 7


    #region Data Translation

    /// <summary>
    /// Converts a Business Object Courier to a Data Object Courier.
    /// Ignores calculated fields like 'OrderInCare' which do not exist in the database.
    /// </summary>
    /// <param name="boCourier">The business entity to convert.</param>
    /// <returns>A data entity ready for DB operations.</returns>
    private static DO.Courier BOToDOCourier(BO.Courier boCourier)
    {
        DO.Courier doCourier = new()
        {
            Id = boCourier.Id,
            FullName = boCourier.FullName,
            Email = boCourier.Email,
            PhoneNumber = boCourier.PhoneNumber,
            Password = boCourier.Password,
            Active = boCourier.IsActive,
            DistanceToDelivery = boCourier.DistanceToDelivery,
            DeliveryType = (DO.Enums.ShippingType)boCourier.DeliveryType!,
            EmploymentStartDate = boCourier.EmploymentStartDate
        };
        return doCourier;
    }

    /// <summary>
    /// Converts a Data Object Courier to a Business Object Courier.
    /// Includes expensive calculations for statistics and active order details.
    /// </summary>
    /// <param name="doCourier">The data entity from the DB.</param>
    /// <returns>A fully populated business entity.</returns>
    private static BO.Courier DOToBOCourier(DO.Courier doCourier)
    {
        BO.Courier boCourier = new BO.Courier()
        {
            Id = doCourier.Id,
            FullName = doCourier.FullName,
            Email = doCourier.Email,
            PhoneNumber = doCourier.PhoneNumber,
            Password = doCourier.Password,
            IsActive = doCourier.Active,
            DistanceToDelivery = doCourier.DistanceToDelivery,
            DeliveryType = (BO.ShippingType?)doCourier.DeliveryType,
            EmploymentStartDate = doCourier.EmploymentStartDate,

            // Calculate historical statistics (Expensive operation, accesses DB multiple times)
            SumOrderInTime = SumOrderInTime(doCourier),
            SumOrderInLate = SumOrderInLate(doCourier),

            // Generate active order details using the shared Tools logic
            OrderInCare = Helpers.Tools.GenerateOrderInProgress(doCourier.Id)
        };
        return boCourier;
    }

    /// <summary>
    /// Converts a Data Object Courier to a lightweight CourierInList object.
    /// Optimized for displaying lists of couriers efficiently.
    /// </summary>
    /// <param name="doCourier">The data entity.</param>
    /// <returns>A lightweight object with summary details.</returns>
    private static BO.CourierInList DOToCourierInList(DO.Courier doCourier)
    {
        int? activeOrderId = null;

        // Stage 7: Lock database access to ensure consistent read
        lock (Helpers.AdminManager.BlMutex)
        {
            activeOrderId = s_dal.Delivery
                .ReadAll(d => d?.CourierId == doCourier.Id && d?.EndOrderTime == null)
                .FirstOrDefault()?.Id;
        }

        return new BO.CourierInList
        {
            Id = doCourier.Id,
            FullName = doCourier.FullName,
            IsActive = doCourier.Active,
            DeliveryType = (BO.ShippingType)doCourier.DeliveryType!,
            EmploymentStartDate = doCourier.EmploymentStartDate,
            SumOrderInTime = SumOrderInTime(doCourier),
            SumOrderInLate = SumOrderInLate(doCourier),

            // Efficiently find the ID of the SINGLE active delivery (if exists)
            IdOrderInCare = activeOrderId
        };
    }

    #endregion Data Translation

    #region Validation Logic

    /// <summary>
    /// Orchestrates validation for all courier properties before adding or updating.
    /// </summary>
    /// <param name="boCourier">The courier object to validate.</param>
    internal static void CheckCorrectnessVariables(BO.Courier boCourier)
    {
        CheckId(boCourier.Id);
        CheckEmail(boCourier.Email);
        CheckPhoneNumber(boCourier.PhoneNumber);
        CheckPassword(boCourier.Password);
        CheckPersonalMaxDistance(boCourier.DistanceToDelivery);
    }

    /// <summary>
    /// Validates the ID using the control digit algorithm via Tools helper.
    /// </summary>
    /// <param name="id">The ID to check.</param>
    internal static void CheckId(int id)
    {
        Tools.CheckId(id);
    }

    /// <summary>
    /// Validates the phone number format via Tools helper.
    /// </summary>
    /// <param name="phoneNumber">The phone number string.</param>
    internal static void CheckPhoneNumber(string phoneNumber)
    {
        Tools.CheckPhoneNumber(phoneNumber);
    }

    /// <summary>
    /// Validates email format using the System.Net.Mail.MailAddress parser.
    /// </summary>
    /// <param name="email">The email string.</param>
    /// <exception cref="BO.BlInvalidDataException">Thrown if email is empty or invalid format.</exception>
    internal static void CheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BO.BlInvalidDataException("ERROR: Email address cannot be empty");
        }

        try
        {
            var addr = new MailAddress(email);
            if (addr.Address != email)
                throw new BO.BlInvalidDataException("ERROR: Invalid email address format");
        }
        catch
        {
            throw new BO.BlInvalidDataException("ERROR: Invalid email address format");
        }
    }

    /// <summary>
    /// Enforces password complexity (Upper, Lower, Digit) and minimum length.
    /// </summary>
    /// <param name="password">The password string.</param>
    /// <exception cref="BO.BlInvalidDataException">Thrown if password does not meet criteria.</exception>
    internal static void CheckPassword(string password)
    {
        // 1. Check minimum length
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            throw new BO.BlInvalidDataException("ERROR: Invalid password, must contain at least 8 characters.");

        // 2. Check complexity (Must contain Upper, Lower, and Digit)
        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            throw new BO.BlInvalidDataException("ERROR: Invalid password, must contain uppercase, lowercase, and a number.");
    }

    /// <summary>
    /// Validates that the courier's personal max distance does not exceed company limits.
    /// </summary>
    /// <param name="maxDistance">The personal max distance (nullable).</param>
    /// <exception cref="BO.BlInvalidDataException">Thrown if negative or exceeds company limit.</exception>
    internal static void CheckPersonalMaxDistance(double? maxDistance)
    {
        if (maxDistance != null)
        {
            double companyMaxLimit = 0;

            // Stage 7: Lock configuration access
            lock (Helpers.AdminManager.BlMutex)
            {
                companyMaxLimit = s_dal.Config.MaxAirDistance ?? 0;
            }

            if (maxDistance <= 0)
                throw new BO.BlInvalidDataException("ERROR: Max distance must be positive.");

            // Verify against global limit only if global limit is enforced (> 0)
            if (companyMaxLimit > 0 && maxDistance > companyMaxLimit)
                throw new BO.BlInvalidDataException($"ERROR: Personal max distance ({maxDistance}) cannot exceed company limit ({companyMaxLimit}).");
        }
    }

    #endregion Validation Logic

    #region CRUD Operations

    /// <summary>
    /// Adds a new courier to the database. Thread-safe and updates observers.
    /// </summary>
    /// <param name="requesterId">The ID of the user requesting the action.</param>
    /// <param name="boCourier">The courier object to add.</param>
    /// <exception cref="BO.BLTemporaryNotAvailableException">Thrown if simulator is running.</exception>
    /// <exception cref="BO.BlAlreadyExistsException">Thrown if courier ID already exists.</exception>
    internal static void AddCourier(int requesterId, BO.Courier boCourier)
    {
        // Stage 7: Blocking simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        DO.Courier doCourier = BOToDOCourier(boCourier);
        try
        {
            // Stage 7: Lock database write
            lock (Helpers.AdminManager.BlMutex)
            {
                s_dal.Courier.Create(doCourier);
            }

            // Notify remains outside lock
            Observers.NotifyListUpdated();
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            // Wrap DAL exception with BL exception to maintain layer separation
            throw new BO.BlAlreadyExistsException($"Courier with ID {doCourier.Id} already exists", ex);
        }
    }

    /// <summary>
    /// Updates an existing courier in the database. Thread-safe and updates observers.
    /// </summary>
    /// <param name="boCourier">The courier object with updated details.</param>
    /// <exception cref="BO.BLTemporaryNotAvailableException">Thrown if simulator is running.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if courier ID not found.</exception>
    internal static void UpdateCourier(BO.Courier boCourier)
    {
        // Stage 7: Blocking simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        DO.Courier doCourier = BOToDOCourier(boCourier);
        try
        {
            // Stage 7: Lock database write
            lock (Helpers.AdminManager.BlMutex)
            {
                s_dal.Courier.Update(doCourier);
            }

            // Notify remains outside lock
            Observers.NotifyListUpdated();
            Observers.NotifyItemUpdated(boCourier.Id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {doCourier.Id} does not exist", ex);
        }
    }

    /// <summary>
    /// Deletes a courier from the database by ID. Thread-safe and updates observers.
    /// </summary>
    /// <param name="courierId">The ID of the courier to delete.</param>
    /// <exception cref="BO.BLTemporaryNotAvailableException">Thrown if simulator is running.</exception>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if courier ID not found.</exception>
    internal static void DeleteCourier(int courierId)
    {
        // Stage 7: Blocking simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        try
        {
            // Stage 7: Lock database delete
            lock (Helpers.AdminManager.BlMutex)
            {
                s_dal.Courier.Delete(courierId);
            }

            // Notify remains outside lock
            Observers.NotifyListUpdated();
            Observers.NotifyItemUpdated(courierId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} does not exist", ex);
        }
    }

    /// <summary>
    /// Checks if the courier is currently assigned to an active, unfinished delivery.
    /// Thread-safe.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>True if the courier has an open order, otherwise False.</returns>
    internal static bool CheckIfOrderOpen(int courierId)
    {
        IEnumerable<DO.Delivery?> deliveries;

        // Stage 7: Lock database read
        lock (Helpers.AdminManager.BlMutex)
        {
            deliveries = s_dal.Delivery.ReadAll();
        }

        // Return true if ANY delivery belongs to this courier AND has not ended yet
        bool hasOpenOrder = deliveries.Any(delivery => delivery?.CourierId == courierId && delivery?.EndType == null);
        return hasOpenOrder;
    }

    #endregion CRUD Operations

    #region Authentication & System Entry

    /// <summary>
    /// Determines the user role (Manager or Courier) based on ID.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <returns>The EmployType enum (Manager/Courier).</returns>
    internal static EmployType GetEmployType(int id)
    {
        // Manager ID is hardcoded in configuration
        int managerId;

        // Stage 7: Lock config read
        lock (Helpers.AdminManager.BlMutex)
        {
            managerId = s_dal.Config.ManagerId;
        }

        if (id == managerId)
            return EmployType.Manager;
        else
            return EmployType.Courier;
    }

    #endregion Authentication & System Entry

    #region List Management & Statistics

    /// <summary>
    /// Filters the courier list based on active status. Thread-safe.
    /// </summary>
    /// <param name="isActive">Optional boolean filter. If null, returns all.</param>
    /// <returns>A collection of CourierInList objects.</returns>
    internal static IEnumerable<BO.CourierInList> FilterByActive(bool? isActive = null)
    {
        IEnumerable<DO.Courier?> doList;

        // Stage 7: Lock database read
        lock (Helpers.AdminManager.BlMutex)
        {
            // Optimized query: if filter is null, fetch all; otherwise fetch by predicate
            if (isActive == null)
                doList = s_dal.Courier.ReadAll();
            else
                doList = s_dal.Courier.ReadAll(c => c?.Active == isActive);

            // Materialize to list inside the lock to ensure snapshot isolation
            doList = doList.ToList();
        }

        // Filter out any potential nulls from DAL and convert to BO
        return doList
                .Where(c => c != null)
                .Select(c => DOToCourierInList(c!));
    }

    /// <summary>
    /// Sorts the courier list based on the specified criteria Enum.
    /// </summary>
    /// <param name="courierInLists">The list to sort.</param>
    /// <param name="keySelector">The sorting criteria enum.</param>
    /// <returns>A sorted collection.</returns>
    internal static IEnumerable<BO.CourierInList> SortBy(IEnumerable<BO.CourierInList> courierInLists, BO.CourierInListEnum? keySelector)
    {
        return keySelector switch
        {
            BO.CourierInListEnum.Id => courierInLists.OrderBy(c => c.Id),
            BO.CourierInListEnum.FullName => courierInLists.OrderBy(c => c.FullName),
            BO.CourierInListEnum.EmploymentStartDate => courierInLists.OrderBy(c => c.EmploymentStartDate),
            BO.CourierInListEnum.DeliveryType => courierInLists.OrderBy(c => c.DeliveryType),

            // Nullable/Boolean fields handling:
            BO.CourierInListEnum.IdOrderInCare => courierInLists.OrderBy(c => c.IdOrderInCare), // Nulls first usually
            BO.CourierInListEnum.IsActive => courierInLists.OrderByDescending(c => c.IsActive), // Show Active (True) first

            // Statistics handling (Higher is better/worse, so Descending usually makes sense for lists)
            BO.CourierInListEnum.SumOrderInTime => courierInLists.OrderByDescending(c => c.SumOrderInTime),
            BO.CourierInListEnum.SumOrderInLate => courierInLists.OrderByDescending(c => c.SumOrderInLate),

            _ => courierInLists.OrderBy(c => c.Id)
        };
    }

    /// <summary>
    /// Calculates total deliveries completed on time for a specific courier.
    /// </summary>
    /// <param name="doCourier">The courier object.</param>
    /// <returns>Count of on-time deliveries.</returns>
    private static int SumOrderInTime(DO.Courier doCourier)
    {
        // Stage 7: Lock complex calculation reading from multiple tables
        lock (Helpers.AdminManager.BlMutex)
        {
            // 1. Fetch relevant deliveries: belonging to courier, finished, and successfully provided
            return s_dal.Delivery.ReadAll(d =>
                    d?.CourierId == doCourier.Id &&
                    d?.EndOrderTime != null &&
                    d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
            // 2. Count matches where Actual Duration <= Max Allowed Duration
            .Count(d => (d!.EndOrderTime!.Value - s_dal.Order.Read(d.OrderId)!.StartOrderTime)
                <= s_dal.Config.MaxDeliveryTime);
        }
    }

    /// <summary>
    /// Calculates total deliveries completed late for a specific courier.
    /// </summary>
    /// <param name="doCourier">The courier object.</param>
    /// <returns>Count of late deliveries.</returns>
    private static int SumOrderInLate(DO.Courier doCourier)
    {
        // Stage 7: Lock complex calculation reading from multiple tables
        lock (Helpers.AdminManager.BlMutex)
        {
            // 1. Fetch relevant deliveries: belonging to courier, finished, and successfully provided
            return s_dal.Delivery.ReadAll(d =>
                    d?.CourierId == doCourier.Id &&
                    d?.EndOrderTime != null &&
                    d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
            // 2. Count matches where Actual Duration > Max Allowed Duration
            .Count(d => (d!.EndOrderTime!.Value - s_dal.Order.Read(d.OrderId)!.StartOrderTime)
                <= s_dal.Config.MaxDeliveryTime);
        }
    }


    #endregion List Management & Statistics

    #region Periodic Maintenance

    /// <summary>
    /// Checks all active couriers and deactivates those who haven't performed a delivery in the last 6 months.
    /// Uses AsyncMutex to prevent overlapping executions by the simulator.
    /// </summary>
    internal static void DeactivateIdleCouriers()
    {
        // 1. Prevent overlapping runs
        if (s_periodicMutex.CheckAndSetInProgress())
            return;

        try
        {
            // 1. Set the threshold date (6 months ago relative to simulated clock)
            // Note: AdminManager.Now is volatile/thread-safe enough for reading
            DateTime limitDate = Helpers.AdminManager.Now.AddMonths(-6);
            List<DO.Courier> couriersToDeactivate = new();

            // 2. READ & FILTER PHASE (Locked)
            lock (Helpers.AdminManager.BlMutex)
            {
                // We perform the entire query inside the lock because it depends on consistent DB state
                // (Checking deliveries and courier status together)
                couriersToDeactivate = s_dal.Courier.ReadAll(c => c?.Active == true)
                    .Where(c => c != null && !CheckIfOrderOpen(c.Id)) // Uses internal helper (Recursive lock is safe)
                    .Where(c =>
                    {
                        var lastDelivery = s_dal.Delivery
                            .ReadAll(d => d?.CourierId == c!.Id && d?.EndOrderTime != null)
                            .MaxBy(d => d?.EndOrderTime)?.EndOrderTime;

                        var lastActivity = lastDelivery ?? c!.EmploymentStartDate ?? DateTime.MinValue;
                        return lastActivity < limitDate;
                    })
                    .Select(c => c!)
                    .ToList();
            }

            // 3. UPDATE PHASE (Granular Locks)
            foreach (var c in couriersToDeactivate)
            {
                lock (Helpers.AdminManager.BlMutex)
                {
                    s_dal.Courier.Update(c with { Active = false });
                }

                // Notify per item (Outside lock)
                Observers.NotifyItemUpdated(c.Id);
            }

            // 4. Notify list (Outside lock)
            if (couriersToDeactivate.Any())
            {
                Observers.NotifyListUpdated();
            }
        }
        finally
        {
            s_periodicMutex.UnsetInProgress();
        }
    }

    internal static async Task SimulateCourierActivityAsync()//stage 7
    {
        // 1. Prevent overlapping runs
        if (s_simulationMutex.CheckAndSetInProgress())
            return;

        try
        {
            Random random = new Random();
            List<(DO.Courier Courier, int OrderId)> selectionsToProcess = new();
            List<(int CourierId, int DeliveryId, DO.Enums.ShipmentCompletionStatus Status)> orderCompletions = new();

            // 2. READ PHASE (Locked): Fetch all active couriers and check their orders
            lock (Helpers.AdminManager.BlMutex)
            {
                // Fetch all active couriers
                var activeCouriers = s_dal.Courier.ReadAll(c => c?.Active == true)
                    .Where(c => c != null)
                    .ToList();

                foreach (var courier in activeCouriers)
                {
                    if (courier == null) continue;

                    // Check if courier has an active delivery (order in care)
                    var activeDelivery = s_dal.Delivery
                        .ReadAll(d => d?.CourierId == courier.Id && d?.EndOrderTime == null)
                        .FirstOrDefault();

                    // BRANCH 1: Courier HAS an active delivery
                    if (activeDelivery != null)
                    {
                        var order = s_dal.Order.Read(activeDelivery.OrderId);
                        if (order != null)
                        {
                            // Calculate time elapsed since delivery started
                            DateTime now = Helpers.AdminManager.Now;
                            TimeSpan elapsedTime = now - activeDelivery.StartDeliveryTime;

                            // Calculate "sufficient time" based on distance + random factor
                            // Base time: estimated arrival time from delivery start
                            DateTime estimatedArrival = Helpers.Tools.EstimatedArrivalTimeCalculate(activeDelivery);
                            TimeSpan timeRequired = estimatedArrival - activeDelivery.StartDeliveryTime;

                            // Add random variance (±20% of the required time)
                            double randomFactor = random.NextDouble() * 0.4 - 0.2; // -0.2 to +0.2
                            TimeSpan adjustedTimeRequired = TimeSpan.FromMilliseconds(
                                timeRequired.TotalMilliseconds * (1 + randomFactor)
                            );

                            // Decision 1: Has sufficient time elapsed?
                            if (elapsedTime >= adjustedTimeRequired)
                            {
                                // Close the order with one of several outcomes
                                DO.Enums.ShipmentCompletionStatus completionStatus = random.Next(0, 100) switch
                                {
                                    < 85 => DO.Enums.ShipmentCompletionStatus.Provided,  // 85% success
                                    < 95 => DO.Enums.ShipmentCompletionStatus.Refused,   // 10% refused
                                    _ => DO.Enums.ShipmentCompletionStatus.NotFound      // 5% not found
                                };

                                orderCompletions.Add((courier.Id, activeDelivery.Id, completionStatus));
                            }
                            else
                            {
                                // Decision 2: Not enough time - cancel with 10% probability
                                if (random.NextDouble() < 0.1)
                                {
                                    orderCompletions.Add((courier.Id, activeDelivery.Id, DO.Enums.ShipmentCompletionStatus.Cancelled));
                                }
                            }
                        }
                    }
                    // BRANCH 2: Courier DOESN'T have an active delivery
                    else
                    {
                        // Stage 1: Check if courier is available (15% probability)
                        if (random.NextDouble() < 0.15)
                        {
                            // Courier is available - get open orders for this courier
                            var openOrders = Helpers.OrderManager.GetOpenOrdersForCourier(courier.Id, null, null)
                                .ToList();

                            // If there are open orders available
                            if (openOrders.Count > 0)
                            {
                                // Stage 2: Decide whether to pick an order (50% probability)
                                if (random.NextDouble() < 0.5)
                                {
                                    // Random selection from available orders
                                    int selectedIndex = random.Next(0, openOrders.Count);
                                    int selectedOrderId = openOrders[selectedIndex].OrderId;

                                    selectionsToProcess.Add((courier, selectedOrderId));
                                }
                            }
                        }
                    }
                }
            }

            // 3. PROCESSING PHASE (Unlocked): Assign new orders and complete existing ones
            // This runs without the main BlMutex, allowing other threads to read data

            // 3A. Complete active orders (Locked per operation)
            foreach (var (courierId, deliveryId, completionStatus) in orderCompletions)
            {
                try
                {
                    lock (Helpers.AdminManager.BlMutex)
                    {
                        var delivery = s_dal.Delivery.Read(deliveryId);
                        if (delivery != null && delivery.EndOrderTime == null)
                        {
                            s_dal.Delivery.Update(delivery with
                            {
                                EndOrderTime = Helpers.AdminManager.Now,
                                EndType = completionStatus
                            });
                        }
                    }

                    // Notify outside lock
                    lock (Helpers.AdminManager.BlMutex)
                    {
                        var orderId = s_dal.Delivery.Read(deliveryId)?.OrderId;
                        if (orderId.HasValue)
                        {
                            Observers.NotifyItemUpdated(courierId);
                            Helpers.OrderManager.Observers.NotifyItemUpdated(orderId.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle silently - simulated activity can fail
                }
            }

            // 3B. Assign new orders to couriers
            foreach (var (courier, orderId) in selectionsToProcess)
            {
                try
                {
                    // Call the order selection logic asynchronously
                    // This will handle network calls for distance calculation, etc.
                    await Helpers.OrderManager.OrderSelectionAsync(courier.Id, orderId);
                }
                catch (Exception ex)
                {
                   
                }
            }

            // 4. Notify observers (Final bulk notification)
            if (orderCompletions.Count > 0 || selectionsToProcess.Count > 0)
            {
                Observers.NotifyListUpdated();
                Helpers.OrderManager.Observers.NotifyListUpdated();
            }
        }
        finally
        {
            s_simulationMutex.UnsetInProgress();
        }
    }
    #endregion Periodic Maintenance

    #region Data Retrieval

    /// <summary>
    /// Retrieves full details of a specific courier by ID.
    /// Thread-safe.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>A full BO.Courier object.</returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if not found.</exception>
    internal static BO.Courier SearchCourier(int courierId)
    {
        DO.Courier doCourier;

        // Stage 7: Lock read
        lock (Helpers.AdminManager.BlMutex)
        {
            // Read directly from DAL, throw exception immediately if null
            doCourier = s_dal.Courier.Read(courierId) ??
                throw new BlDoesNotExistException($"ERROR : courier with id {courierId} not exist");
        }

        // Convert to BO (Triggers calculation of all stats and active order)
        return DOToBOCourier(doCourier);
    }

    /// <summary>
    /// Helper to verify access permission (basic check).
    /// </summary>
    internal static bool AccessCourier(int requesterId, int courierId)
    {
        return (requesterId == courierId);
    }
    #endregion Data Retrieval

}