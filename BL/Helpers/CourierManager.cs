using BO;
using DalApi;
using System.Net.Mail;

namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;
    internal static ObserverManager Observers = new(); //stage 5 

    #region Data Translation

    /// <summary>
    /// Converts a Business Object Courier to a Data Object Courier.
    /// </summary>
    private static DO.Courier BOToDOCourier(BO.Courier boCourier)
    {
        // Simple mapping, ignoring calculated fields like 'OrderInCare' which don't exist in DB
        DO.Courier doCourier = new DO.Courier()
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
    /// Converts a Data Object Courier to a Business Object Courier, including calculated statistics.
    /// </summary>
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
    /// Converts a Data Object Courier to a lightweight CourierInList object for list displays.
    /// </summary>
    private static BO.CourierInList DOToCourierInList(DO.Courier doCourier)
    {
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
            IdOrderInCare = s_dal.Delivery
                .ReadAll(d => d?.CourierId == doCourier.Id && d?.EndOrderTime == null)
                .FirstOrDefault()?.Id,
        };
    }

    #endregion Data Translation

    #region Validation Logic

    /// <summary>
    /// Orchestrates validation for all courier properties.
    /// </summary>
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
    internal static void CheckId(int id)
    {
        Tools.CheckId(id);
    }

    /// <summary>
    /// Validates the phone number format via Tools helper.
    /// </summary>
    internal static void CheckPhoneNumber(string phoneNumber)
    {
        Tools.CheckPhoneNumber(phoneNumber);
    }

    /// <summary>
    /// Validates email format using MailAddress parser.
    /// </summary>
    internal static void CheckEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BO.BlInvalidDataException("ERROR: Email address cannot be empty");
        }

        // Try-Catch block required because MailAddress constructor throws exception on invalid format
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
    /// Enforces password complexity and length policies.
    /// </summary>
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
    internal static void CheckPersonalMaxDistance(double? maxDistance)
    {
        // Logic: Personal limit is optional (nullable), but if set, it must respect global config.
        if (maxDistance != null)
        {
            double companyMaxLimit = s_dal.Config.MaxAirDistance ?? 0;

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
    /// Adds a new courier to the database after converting to DO.
    /// </summary>
    internal static void AddCourier(int requesterId, BO.Courier boCourier)
    {
        DO.Courier doCourier = BOToDOCourier(boCourier);
        try
        {
            s_dal.Courier.Create(doCourier);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            // Wrap DAL exception with BL exception to maintain layer separation
            throw new BO.BlAlreadyExistsException($"Courier with ID {doCourier.Id} already exists", ex);
        }
    }

    /// <summary>
    /// Updates an existing courier in the database.
    /// </summary>
    internal static void UpdateCourier(BO.Courier boCourier)
    {
        // Note: Statistics and Active Orders are ignored during update

        DO.Courier doCourier = BOToDOCourier(boCourier);
        try
        {
            s_dal.Courier.Update(doCourier);

            Observers.NotifyListUpdated();
            Observers.NotifyItemUpdated(boCourier.Id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {doCourier.Id} does not exist", ex);
        }
    }

    /// <summary>
    /// Deletes a courier from the database by ID.
    /// </summary>
    internal static void DeleteCourier(int courierId)
    {
        try
        {
            s_dal.Courier.Delete(courierId);

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
    /// </summary>
    internal static bool CheckIfOrderOpen(int courierId)
    {
        IEnumerable<DO.Delivery?> deliveries = s_dal.Delivery.ReadAll();

        // Return true if ANY delivery belongs to this courier AND has not ended yet
        bool hasOpenOrder = deliveries.Any(delivery => delivery?.CourierId == courierId && delivery?.EndType == null);
        return hasOpenOrder;
    }

    #endregion CRUD Operations

    #region Authentication & System Entry

    /// <summary>
    /// Validates login credentials against stored data.
    /// </summary>
    internal static void CheckPasswordEntry(int id, string password)
    {
        DO.Courier? doCourier = s_dal.Courier.Read(id);

        // Fail if user not found OR password mismatch
        if (doCourier == null || password != doCourier.Password)
            throw new BO.BlInvalidDataException("ERROR : userId or password are wrong");
    }

    /// <summary>
    /// Determines the user role (Manager or Courier) based on ID.
    /// </summary>
    internal static EmployType GetEmployType(int id)
    {
        // Manager ID is hardcoded in configuration
        if (id == s_dal.Config.ManagerId)
            return EmployType.Manager;
        else
            return EmployType.Courier;
    }

    #endregion Authentication & System Entry

    #region List Management & Statistics

    /// <summary>
    /// Filters the courier list based on active status. Returns all if filter is null.
    /// </summary>
    internal static IEnumerable<BO.CourierInList> FilterByActive(bool? isActive = null)
    {
        IEnumerable<DO.Courier?> doList;

        // Optimized query: if filter is null, fetch all; otherwise fetch by predicate
        if (isActive == null)
            doList = s_dal.Courier.ReadAll();
        else
            doList = s_dal.Courier.ReadAll(c => c?.Active == isActive);

        // Filter out any potential nulls from DAL and convert to BO
        return doList
                .Where(c => c != null)
                .Select(c => DOToCourierInList(c!));
    }

    /// <summary>
    /// Sorts the courier list based on the specified criteria Enum.
    /// </summary>
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
    private static int SumOrderInTime(DO.Courier doCourier)
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

    /// <summary>
    /// Calculates total deliveries completed late for a specific courier.
    /// </summary>
    private static int SumOrderInLate(DO.Courier doCourier)
    {
        // 1. Fetch relevant deliveries: belonging to courier, finished, and successfully provided
        return s_dal.Delivery.ReadAll(d =>
                d?.CourierId == doCourier.Id &&
                d?.EndOrderTime != null &&
                d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
        // 2. Count matches where Actual Duration > Max Allowed Duration
        .Count(d => (d!.EndOrderTime!.Value - s_dal.Order.Read(d.OrderId)!.StartOrderTime)
            > s_dal.Config.MaxDeliveryTime);
    }


    #endregion List Management & Statistics

    #region Periodic Maintenance

    /// <summary>
    /// Checks all active couriers and deactivates those who haven't performed a delivery in the last 6 months.
    /// Skips couriers who are currently handling an order.
    /// </summary>
    // Generated by Gemini based on the prompt: 
    // "Add a method that checks all couriers - and any courier who hasn't made a delivery in the last six months
    // will be considered inactive."
    internal static void DeactivateIdleCouriers()
    {
        // 1. Set the threshold date (6 months ago relative to simulated clock)
        DateTime limitDate = Helpers.AdminManager.Now.AddMonths(-6);

        // 2. LINQ Pipeline: Retrieve -> Filter
        var idleCouriers = s_dal.Courier.ReadAll(c => c?.Active == true) // Initial fetch: only active couriers
            .Where(c => c != null && !CheckIfOrderOpen(c.Id)) // Filter 1: Not currently working on an order
            .Where(c =>
            {
                // Filter 2: Check last activity date
                var lastDelivery = s_dal.Delivery
                    .ReadAll(d => d?.CourierId == c!.Id && d?.EndOrderTime != null)
                    .MaxBy(d => d?.EndOrderTime)?.EndOrderTime;

                // Fallback: If no deliveries exist, use EmploymentStartDate. Treat null as very old (MinValue).
                var lastActivity = lastDelivery ?? c!.EmploymentStartDate ?? DateTime.MinValue;

                // Return true if activity is older than the limit (candidate for deactivation)
                return lastActivity < limitDate;
            })
            .ToList(); // Materialize list to enable ForEach

        // 3. Update and Notify
        idleCouriers.ForEach(c =>
        {
            // Update in DAL
            s_dal.Courier.Update(c! with { Active = false });

            // Notify specific observer (Must be outside the 'with' expression)
            Observers.NotifyItemUpdated(c!.Id);
        });

        // 4. Notify list observer if any changes occurred
        if (idleCouriers.Any())
        {
            Observers.NotifyListUpdated();
        }
    }
    #endregion Periodic Maintenance

    #region Data Retrieval

    /// <summary>
    /// Retrieves full details of a specific courier by ID.
    /// </summary>
    internal static BO.Courier SearchCourier(int courierId)
    {
        // Read directly from DAL, throw exception immediately if null
        DO.Courier doCourier = s_dal.Courier.Read(courierId) ??
            throw new BlDoesNotExistException($"ERROR : courier with id {courierId} not exist");

        // Convert to BO (Triggers calculation of all stats and active order)
        return DOToBOCourier(doCourier);
    }


    internal static bool AccessCourier(int requesterId, int courierId)
    {
        return (requesterId == courierId);
    }
    #endregion Data Retrieval
}