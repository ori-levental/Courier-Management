using BO;
using DalApi;
using DO;
using System; // Required for Exception
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Required for Async/Await

namespace Helpers;

/// <summary>
/// Logic helper for Order operations (BL Layer).
/// Handles conversions, calculations, and CRUD logic.
/// </summary>
internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;
    internal static ObserverManager Observers = new();

    // Stage 7: Mutex to prevent overlapping periodic updates
    private static readonly Helpers.AsyncMutex s_periodicMutex = new();

    #region Data Translation

    private static DO.Order BOToDOOrder(BO.Order BoOrder)
    {
        return new DO.Order()
        {
            Id = BoOrder.Id,
            Description = BoOrder.Description,
            CustomerAddress = BoOrder.FullAddress,
            PhoneNumber = BoOrder.PhoneNumber,
            Latitude = BoOrder.Latitude,
            OrderingName = BoOrder.OrderingName,
            Type = (DO.Enums.OrderType)BoOrder.OrderType,
            Longitude = BoOrder.Longitude,
            StartOrderTime = BoOrder.StartOrderTime,
        };
    }

    /// <summary>
    /// Converts a Data Object Order to a Business Object Order.
    /// Calculates dynamic status, time metrics, and populates delivery history.
    /// </summary>
    private static BO.Order DOToBOOrder(DO.Order doOrder)
    {
        IEnumerable<DO.Delivery?> deliveries;
        string? courierName;

        // Stage 7: Lock read from DB
        lock (Helpers.AdminManager.BlMutex)
        {
            // 1. Retrieve all deliveries for this order
            deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id).ToList();
        }

        // Identify the latest one for status calculation
        var latestDelivery = deliveries.MaxBy(d => d?.StartDeliveryTime);

        // 2. Calculate Status (התיקון הלוגי נמצא פה)
        BO.OrderStatus status;

        if (latestDelivery == null)
        {
            // Case 1: No deliveries at all
            status = BO.OrderStatus.Open;
        }
        else if (latestDelivery.EndOrderTime == null)
        {
            // Case 2: There is a delivery but it is still active (no end date)
            status = BO.OrderStatus.OnCare;
        }
        else
        {
            // Case 3: The delivery has ended - checking how it ended
            status = latestDelivery.EndType switch
            {
                DO.Enums.ShipmentCompletionStatus.Provided => BO.OrderStatus.Provided,
                DO.Enums.ShipmentCompletionStatus.Refused => BO.OrderStatus.Refused,
                DO.Enums.ShipmentCompletionStatus.Cancelled => BO.OrderStatus.Cancelled,

                DO.Enums.ShipmentCompletionStatus.Failed => BO.OrderStatus.Open,
                DO.Enums.ShipmentCompletionStatus.NotFound => BO.OrderStatus.Open,

                _ => BO.OrderStatus.Open
            };
        }

        // 3. Calculate Times
        DateTime maxTime = Helpers.Tools.MaxArrivalTimeCalculate(doOrder);
        TimeSpan timeLeft = (maxTime - Helpers.AdminManager.Now);

        // Clamp negative time if deadline passed or order is completed
        if (timeLeft < TimeSpan.Zero || status == BO.OrderStatus.Provided || status == BO.OrderStatus.Cancelled)
            timeLeft = TimeSpan.Zero;

        // 4. Build Delivery History List
        List<BO.DeliveryPerOrderInList> history = deliveries.Select(d =>
        {
            lock (Helpers.AdminManager.BlMutex)
            {
                courierName = s_dal.Courier.Read(d!.CourierId)?.FullName ?? "Unknown";
            }

            return new BO.DeliveryPerOrderInList
            {
                DeliveryId = d.Id,
                CourierId = d.CourierId,
                CourierName = courierName,
                ShippingType = (BO.ShippingType)d.DeliveryShippingType,
                DeliveryStartTime = d.StartDeliveryTime,
                DeliveryEndType = (BO.ShipmentCompletionStatus?)d.EndType,
                DeliveryEndTime = d.EndOrderTime
            };
        }).OrderByDescending(h => h.DeliveryStartTime).ToList();

        DateTime estimatedArrival = DateTime.MinValue;
        if (status == BO.OrderStatus.OnCare && latestDelivery != null)
        {
            estimatedArrival = Helpers.Tools.EstimatedArrivalTimeCalculate(latestDelivery);
        }

        return new BO.Order()
        {
            Id = doOrder.Id,
            Description = doOrder.Description,
            FullAddress = doOrder.CustomerAddress,
            PhoneNumber = doOrder.PhoneNumber,
            Latitude = doOrder.Latitude,
            Longitude = doOrder.Longitude,
            OrderingName = doOrder.OrderingName,
            OrderType = (BO.OrderType)doOrder.Type,
            StartOrderTime = doOrder.StartOrderTime,
            MaxArrivalTime = maxTime,
            TimeRemaining = timeLeft,
            OrderStatus = status,
            AirDistance = Helpers.Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude),
            ScheduleStatus = Helpers.Tools.ScheduleStatusCalculate(doOrder, latestDelivery),
            EstimatedArrivalTime = estimatedArrival,
            DeliveryHistory = history
        };
    }
    private static BO.OrderInList DOToOrderInList(DO.Order doOrder)
    {
        IEnumerable<DO.Delivery?> deliveries;
        DateTime currentClock;

        // Stage 7: Lock read operations
        lock (Helpers.AdminManager.BlMutex)
        {
            // 1. Retrieve the delivery history
            deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id).ToList();
            currentClock = s_dal.Config.Clock;
        }

        var delivery = deliveries.MaxBy(d => d?.StartDeliveryTime);

        // 2. Determine Status
        OrderStatus status = OrderStatus.Open;
        if (delivery != null)
        {
            if (delivery.EndOrderTime == null)
                status = OrderStatus.OnCare;
            else
                status = (OrderStatus)delivery.EndType!;
        }

        // 3. Time Logic
        DateTime maxArrivalTime = Helpers.Tools.MaxArrivalTimeCalculate(doOrder);

        bool isClosed = status == OrderStatus.Provided ||
                        status == OrderStatus.Refused ||
                        status == OrderStatus.Cancelled;

        TimeSpan timeRemaining;
        TimeSpan totalProcessing;

        if (isClosed)
        {
            timeRemaining = TimeSpan.Zero;
            DateTime endTime = delivery?.EndOrderTime ?? currentClock;
            totalProcessing = endTime - doOrder.StartOrderTime;
        }
        else
        {
            timeRemaining = maxArrivalTime - currentClock;
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
            totalProcessing = currentClock - doOrder.StartOrderTime;
        }

        // 4. Schedule Status
        var scheduleStatus = Tools.ScheduleStatusCalculate(doOrder, delivery);

        return new BO.OrderInList
        {
            OrderId = doOrder.Id,
            DeliveryId = delivery?.Id,
            OrderType = (OrderType)doOrder.Type,
            OrderStatus = status,
            AirDistance = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude),

            TimeRemaining = timeRemaining,
            TotalDeliveries = deliveries.Count(),
            TotalProcessingTime = totalProcessing,
            ScheduleStatus = scheduleStatus
        };
    }
    #endregion Data Translation

    #region CRUD & Status Logic

    /// <summary>
    /// Async method to add a new order.
    /// Fetches coordinates from the network asynchronously.
    /// </summary>
    internal static async Task AddOrderAsync(int requesterId, BO.Order boOrder)
    {
        // Stage 7: Block simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        DO.Order doOrder = BOToDOOrder(boOrder);

        // Set StartOrderTime to current simulated clock
        doOrder = doOrder with { StartOrderTime = Helpers.AdminManager.Now };

        // Automatically get coordinates from address if latitude/longitude are 0
        if (doOrder.Latitude == 0 && doOrder.Longitude == 0)
        {
            try
            {
                // Using Async method - NO LOCK HERE (Network call)
                var coords = await Helpers.Tools.GetCoordinatesAsync(doOrder.CustomerAddress);

                // Defensive check (technically Tools throws instead of returning null, but safe to keep)
                if (coords == null)
                    throw new BO.BlNetworkException("Unable to fetch coordinates (Unknown Error).");

                doOrder = doOrder with { Latitude = coords.Value.Latitude, Longitude = coords.Value.Longitude };
            }
            catch (BO.BlNetworkException)
            {
                throw; // Don't touch a network error! Let it reach the PL to display "Network Error"
            }
            catch (BO.BlInvalidDataException)
            {
                throw; // Don't touch an address error! Let it reach the PL to display "Address not found"
            }
            catch (Exception ex)
            {
                // Only unexpected system errors will be wrapped as data errors
                throw new BO.BlInvalidDataException($"Could not get coordinates for address: {ex.Message}");
            }
        }

        double companyLat = 0;
        double companyLon = 0;
        double maxDistance = 0;

        // Stage 7: Lock config read
        lock (Helpers.AdminManager.BlMutex)
        {
            // Company address data
            companyLat = s_dal.Config.Latitude ?? 0;
            companyLon = s_dal.Config.Longitude ?? 0;
            maxDistance = s_dal.Config.MaxAirDistance ?? 0;
        }

        // Calculate distance
        double distance = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude, companyLat, companyLon);

        // Check if too far
        if (maxDistance > 0 && distance > maxDistance)
            throw new BO.BlInvalidDataException($"Address is too far! Distance: {distance:F2}km, Max allowed: {maxDistance}km");


        try
        {
            // Stage 7: Lock DB write
            lock (Helpers.AdminManager.BlMutex)
            {
                s_dal.Order.Create(doOrder);
            }

            // Notify outside lock
            Observers.NotifyListUpdated();
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Order with ID {doOrder.Id} already exists", ex);
        }
    }

    internal static BO.Order GetOrderDetails(int orderId)
    {
        DO.Order? order;

        // Stage 7: Lock DB read
        lock (Helpers.AdminManager.BlMutex)
        {
            order = s_dal.Order.Read(orderId);
        }

        if (order == null) throw new BO.BlDoesNotExistException($"Order {orderId} not found");
        return DOToBOOrder(order);
    }

    /// <summary>
    /// Async method to update an order.
    /// Fetches coordinates from the network asynchronously if needed.
    /// </summary>
    internal static async Task UpdateOrderAsync(BO.Order order)
    {
        // Stage 7: Block simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        DO.Order doOrder = BOToDOOrder(order);

        // Automatically get coordinates if address changed and coords are 0
        try
        {
            // Using Async method - NO LOCK HERE
            var coords = await Tools.GetCoordinatesAsync(doOrder.CustomerAddress);


            // Defensive check (technically Tools throws instead of returning null, but safe to keep)
            if (coords == null)
                throw new BO.BlNetworkException("Unable to fetch coordinates (Unknown Error).");

            doOrder = doOrder with { Latitude = coords.Value.Latitude, Longitude = coords.Value.Longitude };
        }
        catch (BO.BlNetworkException)
        {
            throw; // Don't touch a network error! Let it reach the PL to display "Network Error"
        }
        catch (BO.BlInvalidDataException)
        {
            throw; // Don't touch an address error! Let it reach the PL to display "Address not found"
        }
        catch (Exception ex)
        {
            // Only unexpected system errors will be wrapped as data errors
            throw new BO.BlInvalidDataException($"Could not get coordinates for address: {ex.Message}");
        }

        double companyLat = 0;
        double companyLon = 0;
        double maxDistance = 0;

        // Stage 7: Lock Config read
        lock (Helpers.AdminManager.BlMutex)
        {
            // Company address data
            companyLat = s_dal.Config.Latitude ?? 0;
            companyLon = s_dal.Config.Longitude ?? 0;
            maxDistance = s_dal.Config.MaxAirDistance ?? 0;
        }

        // Calculate distance
        double distance = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude, companyLat, companyLon);

        // Check if too far
        if (maxDistance > 0 && distance > maxDistance)
            throw new BO.BlInvalidDataException($"Address is too far! Distance: {distance:F2}km, Max allowed: {maxDistance}km");


        try
        {
            // Stage 7: Lock DB Update
            lock (Helpers.AdminManager.BlMutex)
            {
                s_dal.Order.Update(doOrder);
            }

            // Notify outside lock
            Observers.NotifyListUpdated();
            Observers.NotifyItemUpdated(order.Id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order {order.Id} not found", ex);
        }
    }

    internal static void CancelOrder(int orderId)
    {
        // Stage 7: Block simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        IEnumerable<DO.Delivery?> deliveries;

        // Stage 7: Lock DB Read
        lock (Helpers.AdminManager.BlMutex)
        {
            deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId).ToList();
        }

        var delivery = deliveries.MaxBy(d => d?.StartDeliveryTime);

        // Case 1: Order is Open -> Create Mock Cancelled Delivery
        if (delivery == null)
        {
            DO.Delivery mockDelivery = new DO.Delivery
            {
                OrderId = orderId,
                CourierId = 0,
                StartDeliveryTime = Helpers.AdminManager.Now,
                EndOrderTime = Helpers.AdminManager.Now,
                EndType = DO.Enums.ShipmentCompletionStatus.Cancelled,
                Distance = 0
            };

            // Stage 7: Lock DB Create
            lock (Helpers.AdminManager.BlMutex)
            {
                s_dal.Delivery.Create(mockDelivery);
            }

            Observers.NotifyListUpdated();
            Observers.NotifyItemUpdated(orderId);
            return;
        }

        // Case 2: Check status constraints
        if (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
            throw new BO.BlCannotCancel("ERROR: cannot cancel an order that has been provided");

        if (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Cancelled)
            throw new BO.BlCannotCancel("ERROR: order is already cancelled");

        // Case 3: Update existing delivery to Cancelled
        DO.Delivery updatedDelivery = delivery with
        {
            EndType = DO.Enums.ShipmentCompletionStatus.Cancelled,
            EndOrderTime = Helpers.AdminManager.Now
        };

        // Stage 7: Lock DB Update
        lock (Helpers.AdminManager.BlMutex)
        {
            s_dal.Delivery.Update(updatedDelivery);
        }

        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);
    }

    internal static void DeleteOrder(int orderId)
    {
        throw new BO.BlCannotCancel("Deleting orders is not allowed.");
    }

    #endregion CRUD & Status Logic

    #region Courier Operations

    internal static async Task OrderSelectionAsync(int courierId, int orderId)
    {
        // Stage 7: Block simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        DO.Courier? courier;
        DO.Order? order;

        // Stage 1: Initial data fetch (fast read inside lock)
        lock (Helpers.AdminManager.BlMutex)
        {
            courier = s_dal.Courier.Read(courierId) ?? throw new BO.BlDoesNotExistException("Courier not found");
            order = s_dal.Order.Read(orderId) ?? throw new BO.BlDoesNotExistException("Order not found");

            // Initial check if the order is available (to save network read if not)
            var existingDeliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId);
            if (existingDeliveries.Any(d => d?.EndOrderTime == null && d?.CourierId != 0))
                throw new BO.BlAlreadyExistsException("Order is currently handled by another courier.");
        }

        // Step 2: Calculate actual distance (long network operation - no locking!)
        // We send the courier's vehicle type to determine the route
        double? routeDistance = await Tools.GetRouteDistanceAsync(
            order.Latitude, order.Longitude, // Origin (customer)
            s_dal.Config.Latitude ?? 0, s_dal.Config.Longitude ?? 0, // Destination (company)
            courier.DeliveryType ?? DO.Enums.ShippingType.Motorcycle // Vehicle type
        );

        // Step 3: Safeguard (Re-Lock)
        lock (Helpers.AdminManager.BlMutex)
        {
            // Double Check Locking in case someone grabbed the order while we were waiting for the network
            var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId).ToList();

            if (deliveries.Any(d =>
                (d?.EndOrderTime == null && d?.CourierId != 0) ||
                d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||
                d?.EndType == DO.Enums.ShipmentCompletionStatus.Cancelled))
            {
                throw new BO.BlAlreadyExistsException("Order was grabbed by someone else just now.");
            }

            // Create base delivery object
            DO.Delivery newDelivery = new()
            {
                OrderId = orderId,
                CourierId = courierId,
                StartDeliveryTime = Helpers.AdminManager.Now,
                DeliveryShippingType = courier.DeliveryType ?? DO.Enums.ShippingType.Motorcycle
            };

            // --- CRITICAL LOGIC FOR DISTANCE ---
            if (routeDistance != null)
            {
                // Success: Set distance and keep order open (On Care)
                newDelivery = newDelivery with { Distance = routeDistance };
            }
            else
            {
                // Failure (Network/API Error): Close order immediately as Failed/Refused
                newDelivery = newDelivery with
                {
                    Distance = null,
                    EndOrderTime = Helpers.AdminManager.Now, // Order is closed
                    EndType = DO.Enums.ShipmentCompletionStatus.Failed // Status: Failed
                };
            }

            s_dal.Delivery.Create(newDelivery);
        }

        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);
    }

    internal static void CloseOrder(int courierId, int deliveryId, BO.ShipmentCompletionStatus status)
    {
        // Stage 7: Block simulator
        Helpers.AdminManager.ThrowOnSimulatorIsRunning();

        int orderId = 0;

        // Stage 7: Lock transaction (Read + Update)
        lock (Helpers.AdminManager.BlMutex)
        {
            DO.Delivery? delivery = s_dal.Delivery.Read(deliveryId) ?? throw new BO.BlDoesNotExistException("Delivery not found");
            if (delivery.CourierId != courierId) throw new BO.BlAccessPermission("Courier ID mismatch");

            if (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
                throw new BO.BlCannotClose("ERROR: already provided");

            DO.Delivery updatedDelivery = delivery with
            {
                EndType = (DO.Enums.ShipmentCompletionStatus)status,
                EndOrderTime = Helpers.AdminManager.Now
            };
            s_dal.Delivery.Update(updatedDelivery);
            orderId = delivery.OrderId;
        }

        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);
    }

    #endregion Courier Operations

    #region List Logic (Filters & Sorts)

    internal static IEnumerable<BO.OrderInList> ListOfOrder(OrderInListEnum? filterBy, object? filterValue, OrderInListEnum? sortBy)
    {
        IEnumerable<BO.OrderInList> query;

        // Stage 7: Lock DB read
        lock (Helpers.AdminManager.BlMutex)
        {
            // 1. Retrieve all orders from DAL and convert them to BO.OrderInList
            // Materialize to list INSIDE lock
            query = s_dal.Order.ReadAll()
                .Select(o => DOToOrderInList(o!))
                .ToList();
        }

        // 2. Apply Filtering logic if parameters are provided
        if (filterBy != null && filterValue != null)
        {
            if (filterBy == OrderInListEnum.OrderStatus)
                query = query.Where(o => o.OrderStatus == (OrderStatus)filterValue);
            else if (filterBy == OrderInListEnum.OrderType)
                query = query.Where(o => o.OrderType == (OrderType)filterValue);
        }

        // 3. Apply Sorting logic based on the enum selection
        if (sortBy != null)
        {
            query = sortBy switch
            {
                // Identity Fields
                OrderInListEnum.OrderId => query.OrderBy(o => o.OrderId),
                OrderInListEnum.DeliveryId => query.OrderBy(o => o.DeliveryId), // Sort nulls first by default usually

                // Categorical Fields
                OrderInListEnum.OrderType => query.OrderBy(o => o.OrderType),
                OrderInListEnum.OrderStatus => query.OrderBy(o => o.OrderStatus),
                OrderInListEnum.ScheduleStatus => query.OrderBy(o => o.ScheduleStatus),

                // Numerical/Quantitative Fields
                // Sorting Distance descending to see furthest orders first
                OrderInListEnum.AirDistance => query.OrderByDescending(o => o.AirDistance),

                // Sorting TimeRemaining ascending (smallest time/most urgent first)
                OrderInListEnum.TimeRemaining => query.OrderBy(o => o.TimeRemaining),

                // Sorting Processing Time descending (longest active orders first)
                OrderInListEnum.TotalProcessingTime => query.OrderByDescending(o => o.TotalProcessingTime),

                // Sorting Total Deliveries descending (problematic orders first)
                OrderInListEnum.TotalDeliveries => query.OrderByDescending(o => o.TotalDeliveries),

                // Fallback default
                _ => query.OrderBy(o => o.OrderId)
            };
        }
        else
        {
            // Default behavior if no sort is selected
            query = query.OrderBy(o => o.OrderId);
        }

        return query;
    }
    internal static IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(int courierId, OrderType? filterBy, OpenOrderInListEnum? sortBy)
    {
        List<BO.OpenOrderInList> result;

        // Stage 7: Lock transaction
        lock (Helpers.AdminManager.BlMutex)
        {
            // 1. Validate Courier
            DO.Courier? courier = s_dal.Courier.Read(courierId);
            if (courier == null) throw new BO.BlDoesNotExistException("Courier not found");

            double maxDist = courier.DistanceToDelivery ?? double.MaxValue;
            var allOrders = s_dal.Order.ReadAll();

            // 2. Generate list using LINQ
            result = allOrders
                .Where(doOrder => doOrder != null)
                // Filter out orders that are currently active, provided, or cancelled
                .Where(doOrder =>
                {
                    var orderDeliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id);

                    bool isActive = orderDeliveries.Any(d => d?.EndOrderTime == null);
                    bool isClosed = orderDeliveries.Any(d =>
                        d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||
                        d?.EndType == DO.Enums.ShipmentCompletionStatus.Refused);

                    // Keep only if NOT active and NOT closed
                    return !isActive && !isClosed;
                })
                // Project to anonymous object to calculate distance once
                .Select(doOrder => new
                {
                    Order = doOrder,
                    // Use safe distance calculation from Tools
                    Distance = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude)
                })
                // Filter by maximum distance allowed for the courier
                .Where(item => item.Distance <= maxDist)
                // Calculate times and map to final Business Object
                .Select(item =>
                {
                    // A. Calculate Deadline & Time Remaining
                    DateTime maxTime = Tools.MaxArrivalTimeCalculate(item.Order);
                    TimeSpan timeLeft = maxTime - Helpers.AdminManager.Now;

                    // Clamp negative time to zero
                    if (timeLeft < TimeSpan.Zero) timeLeft = TimeSpan.Zero;

                    // B. Create a dummy delivery object to reuse existing calculation logic.
                    // We use the COURIER'S vehicle type to calculate the correct speed.
                    var dummyDelivery = new DO.Delivery
                    {
                        OrderId = item.Order.Id,
                        CourierId = 0, // Not relevant for speed calc
                        StartDeliveryTime = Helpers.AdminManager.Now,
                        DeliveryShippingType = courier.DeliveryType ?? DO.Enums.ShippingType.Motorcycle,
                        Distance = item.Distance,
                        EndOrderTime = null,
                        EndType = null
                    };

                    // C. Calculate Estimated Travel Time using the centralized Tools method
                    // (This avoids duplicating the switch-case speed logic here)
                    DateTime estimatedArrival = Tools.EstimatedArrivalTimeCalculate(dummyDelivery);
                    TimeSpan travelTime = estimatedArrival - dummyDelivery.StartDeliveryTime;

                    // D. Build the result object
                    return new BO.OpenOrderInList
                    {
                        OrderId = item.Order.Id,
                        OrderType = (OrderType)item.Order.Type,
                        FullAddress = item.Order.CustomerAddress,
                        AirDistance = item.Distance,
                        IsHeavy = false,
                        MaxArrivalTime = maxTime,
                        TimeRemaining = timeLeft,           // Clamped value
                        ActualTimeEstimation = travelTime,  // Calculated via Tools
                        ScheduleStatus = Tools.ScheduleStatusCalculate(item.Order, dummyDelivery),
                        ActualDistance = item.Distance,
                    };
                })
                .ToList();
        }

        // 3. Apply optional Filter and Sort on the resulting list
        var query = result.AsEnumerable();

        if (filterBy != null)
        {
            query = query.Where(o => o.OrderType == filterBy.Value);
        }

        if (sortBy != null)
        {
            query = sortBy switch
            {
                OpenOrderInListEnum.AirDistance => query.OrderBy(o => o.AirDistance),
                OpenOrderInListEnum.OrderId => query.OrderBy(o => o.OrderId),
                // Default: Sort by urgency (Time Remaining)
                _ => query.OrderBy(o => o.TimeRemaining)
            };
        }
        else
        {
            // Default behavior if no sort is selected
            query = query.OrderBy(o => o.TimeRemaining);
        }

        return query;
    }
    internal static IEnumerable<BO.ClosedDeliveryInList> GetClosedOrdersForCourier(int courierId, ClosedDeliveryInListEnum? filterBy, object? filterValue, ClosedDeliveryInListEnum? sortBy)
    {
        IEnumerable<BO.ClosedDeliveryInList> list;

        // Stage 7: Lock DB read
        lock (Helpers.AdminManager.BlMutex)
        {
            var deliveries = s_dal.Delivery.ReadAll(d => d?.CourierId == courierId && d?.EndOrderTime != null);
            list = deliveries.Select(d => new BO.ClosedDeliveryInList
            {
                DeliveryId = d!.Id,
                OrderId = d.OrderId,
                ShippingType = (ShippingType)d.DeliveryShippingType,
                DeliveryEndType = (ShipmentCompletionStatus?)d.EndType,
                DeliveryEndTime = d.EndOrderTime,
                FullAddress = s_dal.Order.Read(d.OrderId)?.CustomerAddress ?? "",
                ActualDistanceKm = d.Distance,
                TotalProcessingTime = (d.EndOrderTime ?? DateTime.MinValue) - d.StartDeliveryTime,
                OrderType = (OrderType)(s_dal.Order.Read(d.OrderId)?.Type ?? DO.Enums.OrderType.Business)
            }).ToList();
        }

        if (filterBy != null && filterValue != null)
        {
            if (filterBy == ClosedDeliveryInListEnum.DeliveryEndType)
                list = list.Where(i => i.DeliveryEndType == (ShipmentCompletionStatus)filterValue);
            else if (filterBy == ClosedDeliveryInListEnum.OrderType)
                list = list.Where(i => i.OrderType == (OrderType)filterValue);
        }

        if (sortBy != null)
        {
            if (sortBy == ClosedDeliveryInListEnum.DeliveryEndTime)
                list = list.OrderByDescending(i => i.DeliveryEndTime);
        }

        return list;
    }

    #endregion List Logic

    // --- STATISTICS & HELPERS ---

    internal static int[] SumAmountOfOrders()
    {
        // Stage 7: Lock the entire calculation to prevent data inconsistency during computation
        lock (Helpers.AdminManager.BlMutex)
        {
            var orderStatusValues = Enum.GetValues(typeof(ShipmentCompletionStatus)).Cast<ShipmentCompletionStatus>().ToArray();
            var scheduleStatusValues = Enum.GetValues(typeof(ScheduleStatus)).Cast<ScheduleStatus>().ToArray();
            int orderStatusCount = orderStatusValues.Length;
            int scheduleStatusCount = scheduleStatusValues.Length;
            int[] result = new int[orderStatusCount * scheduleStatusCount];

            IEnumerable<DO.Order> orders = Factory.Get.Order.ReadAll();
            IEnumerable<DO.Delivery> deliveries = Factory.Get.Delivery.ReadAll();
            TimeSpan onTimeThreshold = TimeSpan.FromMinutes(30);

            var indexed = from o in orders
                          let latestDelivery = deliveries
                              .Where(d => d.OrderId == o.Id)
                              .OrderByDescending(d => d.StartDeliveryTime)
                              .FirstOrDefault()
                          let orderStatus = latestDelivery == null
                              ? OrderStatus.Open
                              : (latestDelivery.EndType.HasValue ? (OrderStatus)latestDelivery.EndType.Value : OrderStatus.Open)
                          let scheduleStatus = ComputeScheduleStatus(latestDelivery, onTimeThreshold)
                          let index = ((int)orderStatus * scheduleStatusCount) + (int)scheduleStatus
                          select index;

            var groups = indexed.GroupBy(i => i).Select(g => new { Index = g.Key, Count = g.Count() });

            foreach (var g in groups)
            {
                if (g.Index >= 0 && g.Index < result.Length)
                    result[g.Index] = g.Count;
            }

            return result;
        }
    }

    static ScheduleStatus ComputeScheduleStatus(DO.Delivery? delivery, TimeSpan onTimeThreshold)
    {
        if (delivery == null) return ScheduleStatus.InRisk;
        if (delivery.StartDeliveryTime == default(DateTime)) return ScheduleStatus.InRisk;
        if (delivery.EndOrderTime.HasValue)
        {
            TimeSpan duration = delivery.EndOrderTime.Value - delivery.StartDeliveryTime;
            return duration <= onTimeThreshold ? ScheduleStatus.OnTime : ScheduleStatus.Late;
        }
        return ScheduleStatus.InRisk;
    }

    internal static void CheckCorrectnessVariables(BO.Order boOrder)
    {
        if (string.IsNullOrWhiteSpace(boOrder.OrderingName))
            throw new BO.BlInvalidDataException("ERROR: Customer Name cannot be empty.");
        if (string.IsNullOrWhiteSpace(boOrder.FullAddress))
            throw new BO.BlInvalidDataException("ERROR: Address cannot be empty.");

        Tools.CheckPhoneNumber(boOrder.PhoneNumber);
    }

    /// <summary>
    /// Updates order status based on simulated time progress.
    /// To be called periodically by AdminManager.UpdateClock.
    /// </summary>
    internal static void PeriodicOrdersUpdate(DateTime oldClock, DateTime newClock)
    {
        // 1. Check if previous run is still in progress (Prevents overlap)
        if (s_periodicMutex.CheckAndSetInProgress())
            return;

        try
        {
            List<int> ordersToNotify = new List<int>();

            // Temporary list to hold data for calculation (Delivery + Order details)
            // We use a Tuple or Anonymous type to hold copies of the data
            var itemsToProcess = new List<(DO.Delivery Delivery, DO.Order Order)>();

            // 2. READ PHASE (Locked): Fetch active deliveries and their orders
            lock (Helpers.AdminManager.BlMutex)
            {
                var activeDeliveries = s_dal.Delivery.ReadAll(d => d?.EndOrderTime == null && d?.CourierId != 0);

                // Materialize to list immediately inside lock to avoid lazy evaluation issues
                itemsToProcess = activeDeliveries
                    .Where(d => d != null)
                    .Select(d => (Delivery: d!, Order: s_dal.Order.Read(d!.OrderId)))
                    .Where(item => item.Order != null) // Filter out corrupt data
                    .Select(item => (item.Delivery, item.Order!))
                    .ToList();
            }

            // 3. CALCULATION PHASE (Unlocked): Loop through items
            // This runs without the main BlMutex, allowing other UI threads to read data
            foreach (var item in itemsToProcess)
            {
                // Calculate estimated arrival (Tools methods handle Config locks internally)
                // Note: We use Tools.EstimatedArrivalTimeCalculate which handles speed logic and distance

                // Recalculate distance if missing (Tools handles Config lock)
                if (item.Delivery.Distance == null)
                {
                    // This is just for logic, we don't save back unless updated
                }

                DateTime estimatedArrival = Tools.EstimatedArrivalTimeCalculate(item.Delivery);

                // Check if delivery is completed by the new clock time
                if (newClock >= estimatedArrival)
                {
                    // 4. UPDATE PHASE (Locked): Update specific delivery
                    lock (Helpers.AdminManager.BlMutex)
                    {
                        // Re-read or just update? Update is safe if ID matches.
                        s_dal.Delivery.Update(item.Delivery with
                        {
                            EndOrderTime = estimatedArrival,
                            EndType = DO.Enums.ShipmentCompletionStatus.Provided
                        });
                    }

                    // Collect ID for notification (outside lock)
                    ordersToNotify.Add(item.Order.Id);
                }
            }

            // 5. NOTIFY PHASE (Unlocked): Send notifications
            foreach (var id in ordersToNotify)
            {
                Observers.NotifyItemUpdated(id);
            }

            if (ordersToNotify.Any())
            {
                Observers.NotifyListUpdated();
            }
        }
        finally
        {
            // 6. Release the AsyncMutex
            s_periodicMutex.UnsetInProgress();
        }
    }

}