using BO;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;
    internal static ObserverManager Observers = new(); //stage 5 

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
        // 1. Retrieve all deliveries for this order
        var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id);

        // Identify the latest one for status calculation
        var latestDelivery = deliveries.MaxBy(d => d?.StartDeliveryTime);

        // 2. Calculate Status
        ShipmentCompletionStatus status;
        if (latestDelivery == null) status = ShipmentCompletionStatus.Open;
        else if (latestDelivery.EndOrderTime == null) status = ShipmentCompletionStatus.OnCare; // In Progress
        else status = (ShipmentCompletionStatus)latestDelivery.EndType!;

        // 3. Calculate Times
        DateTime maxTime = Helpers.Tools.MaxArrivalTimeCalculate(doOrder);
        TimeSpan timeLeft = (maxTime - Helpers.AdminManager.Now);

        // Clamp negative time if deadline passed or order is completed
        if (timeLeft < TimeSpan.Zero || status == ShipmentCompletionStatus.Provided || status == ShipmentCompletionStatus.Cancelled)
            timeLeft = TimeSpan.Zero;

        // 4. Build Delivery History List
        List<BO.DeliveryPerOrderInList> history = deliveries.Select(d =>
        {
            string courierName = s_dal.Courier.Read(d!.CourierId)?.FullName ?? "Unknown";

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

        return new BO.Order()
        {
            Id = doOrder.Id,
            Description = doOrder.Description,
            FullAddress = doOrder.CustomerAddress,
            PhoneNumber = doOrder.PhoneNumber,
            Latitude = doOrder.Latitude,
            OrderingName = doOrder.OrderingName,
            OrderType = (OrderType)doOrder.Type,
            Longitude = doOrder.Longitude,
            StartOrderTime = doOrder.StartOrderTime,

            // Calculated fields
            MaxArrivalTime = maxTime,
            TimeRemaining = timeLeft,
            OrderStatus = status,

            // History
            DeliveryHistory = history
        };
    }

    private static BO.OrderInList DOToOrderInList(DO.Order doOrder)
    {
        var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id);
        var delivery = deliveries.MaxBy(d => d?.StartDeliveryTime);

        ShipmentCompletionStatus status = ShipmentCompletionStatus.Open;
        TimeSpan totalProcessing = TimeSpan.Zero;

        if (delivery != null)
        {
            if (delivery.EndOrderTime == null) status = ShipmentCompletionStatus.OnCare; // In Progress
            else
            {
                status = (ShipmentCompletionStatus)delivery.EndType!;
                // Calculate Processing Time only if provided
                if (status == ShipmentCompletionStatus.Provided)
                    totalProcessing = delivery.EndOrderTime.Value - delivery.StartDeliveryTime;
            }
        }

        // Calculate Time Remaining
        TimeSpan timeRemaining = Tools.MaxArrivalTimeCalculate(doOrder) - Helpers.AdminManager.Now;

        // Clamp negative time for closed orders
        var dummyDelivery = delivery ?? new DO.Delivery();
        var scheduleStatus = Tools.ScheduleStatusCalculate(doOrder, dummyDelivery);

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
            ScheduleStatus = (BO.ScheduleStatus)scheduleStatus
        };
    }

    #endregion Data Translation

    #region CRUD & Status Logic

    internal static void AddOrder(int requesterId, BO.Order boOrder)
    {
        DO.Order doOrder = BOToDOOrder(boOrder);

        // Set StartOrderTime to current simulated clock
        doOrder = doOrder with { StartOrderTime = Helpers.AdminManager.Now };

        try
        {
            s_dal.Order.Create(doOrder);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Order with ID {doOrder.Id} already exists", ex);
        }
    }

    internal static BO.Order GetOrderDetails(int orderId)
    {
        DO.Order? order = s_dal.Order.Read(orderId);
        if (order == null) throw new BO.BlDoesNotExistException($"Order {orderId} not found");
        return DOToBOOrder(order);
    }

    internal static void UpdateOrder(BO.Order order)
    {
        DO.Order doOrder = BOToDOOrder(order);
        try
        {
            s_dal.Order.Update(doOrder);
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
        var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId);
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
            s_dal.Delivery.Create(mockDelivery);
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
        s_dal.Delivery.Update(updatedDelivery);
        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);
    }

    internal static void DeleteOrder(int orderId)
    {
        throw new BO.BlCannotCancel("Deleting orders is not allowed.");
    }

    #endregion CRUD & Status Logic

    #region Courier Operations

    internal static void OrderSelection(int courierId, int orderId)
    {
        // 1. Verify Order availability - only reject if ALREADY in progress or completed
        var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId).ToList();
        
        // בדוק אם יש דליברי פעיל (בתהליך) או שהסתיים
        if (deliveries.Any(d => 
            (d?.EndOrderTime == null && d?.CourierId != 0) ||  // בתהליך
            d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||  // סיים בהצלחה
            d?.EndType == DO.Enums.ShipmentCompletionStatus.Cancelled))  // בוטל
        {
            throw new BO.BlAlreadyExistsException("Order is already being handled, completed, or cancelled.");
        }

        // 2. Validate Courier & Order existence
        DO.Courier? courier = s_dal.Courier.Read(courierId);
        if (courier == null) throw new BO.BlDoesNotExistException("Courier not found");

        DO.Order? order = s_dal.Order.Read(orderId);
        if (order == null) throw new BO.BlDoesNotExistException("Order not found");

        // 3. Calculate Distance
        double dist = Tools.CalculateAirDistance(order.Latitude, order.Longitude);

        // 4. Create Delivery
        DO.Delivery newDelivery = new DO.Delivery
        {
            OrderId = orderId,
            CourierId = courierId,
            StartDeliveryTime = Helpers.AdminManager.Now,
            DeliveryShippingType = courier.DeliveryType ?? DO.Enums.ShippingType.Motorcycle,
            Distance = dist
        };
        s_dal.Delivery.Create(newDelivery);

        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(orderId);
    }

    internal static void CloseOrder(int courierId, int deliveryId)
    {
        DO.Delivery? delivery = s_dal.Delivery.Read(deliveryId) ?? throw new BO.BlDoesNotExistException("Delivery not found");
        if (delivery.CourierId != courierId) throw new BO.BlAccessPermission("Courier ID mismatch");

        if (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
            throw new BO.BlCannotClose("ERROR: already provided");

        DO.Delivery updatedDelivery = delivery with
        {
            EndType = DO.Enums.ShipmentCompletionStatus.Provided,
            EndOrderTime = Helpers.AdminManager.Now
        };
        s_dal.Delivery.Update(updatedDelivery);
        Observers.NotifyListUpdated();
        Observers.NotifyItemUpdated(delivery.OrderId);
    }

    #endregion Courier Operations

    #region List Logic (Filters & Sorts)

    internal static IEnumerable<BO.OrderInList> ListOfOrder(OrderInListEnum? filterBy, object? filterValue, OrderInListEnum? sortBy)
    {
        // 1. Retrieve all orders from DAL and convert them to BO.OrderInList
        var query = s_dal.Order.ReadAll().Select(o => DOToOrderInList(o!));

        // 2. Apply Filtering logic if parameters are provided
        if (filterBy != null && filterValue != null)
        {
            if (filterBy == OrderInListEnum.OrderStatus)
                query = query.Where(o => o.OrderStatus == (ShipmentCompletionStatus)filterValue);
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
        // 1. Validate Courier
        DO.Courier? courier = s_dal.Courier.Read(courierId);
        if (courier == null) throw new BO.BlDoesNotExistException("Courier not found");

        double maxDist = courier.DistanceToDelivery ?? double.MaxValue;
        var allOrders = s_dal.Order.ReadAll();

        // 2. Generate list using LINQ (replaces the previous foreach loop)
        List<BO.OpenOrderInList> result = allOrders
            .Where(doOrder => doOrder != null)
            // Filter out orders that are currently active, provided, or cancelled
            .Where(doOrder =>
            {
                var orderDeliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id);

                bool isActive = orderDeliveries.Any(d => d?.EndOrderTime == null);
                bool isClosed = orderDeliveries.Any(d =>
                    d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||
                    d?.EndType == DO.Enums.ShipmentCompletionStatus.Cancelled);

                // Keep only if NOT active and NOT closed
                return !isActive && !isClosed;
            })
            // Calculate distance (project to temporary object to avoid double calculation)
            .Select(doOrder => new
            {
                Order = doOrder,
                Distance = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude)
            })
            // Filter by maximum distance allowed for the courier
            .Where(item => item.Distance <= maxDist)
            // Calculate times and map to final Business Object
            .Select(item =>
            {
                DateTime maxTime = Tools.MaxArrivalTimeCalculate(item.Order);

                // Calculate TimeRemaining with clamp logic
                TimeSpan timeLeft = maxTime - Helpers.AdminManager.Now;
                if (timeLeft < TimeSpan.Zero) timeLeft = TimeSpan.Zero;

                // Create a dummy delivery object to calculate schedule status
                // (Open orders don't have a delivery yet, but we need one for ScheduleStatusCalculate)
                var dummyDelivery = new DO.Delivery
                {
                    OrderId = item.Order.Id,
                    CourierId = 0,
                    StartDeliveryTime = Helpers.AdminManager.Now,
                    DeliveryShippingType = DO.Enums.ShippingType.Motorcycle,
                    Distance = 0,
                    EndOrderTime = null,  // Not completed yet
                    EndType = null
                };

                return new BO.OpenOrderInList
                {
                    OrderId = item.Order.Id,
                    OrderType = (OrderType)item.Order.Type,
                    FullAddress = item.Order.CustomerAddress,
                    AirDistance = item.Distance,
                    IsHeavy = false,
                    MaxArrivalTime = maxTime,
                    TimeRemaining = timeLeft,
                    ScheduleStatus = (ScheduleStatus)Tools.ScheduleStatusCalculate(item.Order, dummyDelivery),
                    ActualDistance = item.Distance,
                };
            })
            .ToList();

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
                _ => query.OrderBy(o => o.AirDistance)
            };
        }

        return query;
    }
    internal static IEnumerable<BO.ClosedDeliveryInList> GetClosedOrdersForCourier(int courierId, ClosedDeliveryInListEnum? filterBy, object? filterValue, ClosedDeliveryInListEnum? sortBy)
    {
        var deliveries = s_dal.Delivery.ReadAll(d => d?.CourierId == courierId && d?.EndOrderTime != null);
        var list = deliveries.Select(d => new BO.ClosedDeliveryInList
        {
            DeliveryId = d!.Id,
            OrderId = d.OrderId,
            ShippingType = (ShippingType)d.DeliveryShippingType,
            DeliveryEndType = (ShipmentCompletionStatus?)d.EndType,
            DeliveryEndTime = d.EndOrderTime,
            FullAddress = s_dal.Order.Read(d.OrderId)?.CustomerAddress ?? "",
            ActualDistanceKm = d.Distance,
            TotalProcessingTime = (d.EndOrderTime ?? DateTime.MinValue) - d.StartDeliveryTime,
            OrderType = (OrderType)(s_dal.Order.Read(d.OrderId)?.Type ?? DO.Enums.OrderType.Standard)
        });

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
                          ? ShipmentCompletionStatus.Open
                          : (latestDelivery.EndType.HasValue ? (ShipmentCompletionStatus)latestDelivery.EndType.Value : ShipmentCompletionStatus.Open)
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
        var activeDeliveries = s_dal.Delivery.ReadAll(d => d?.EndOrderTime == null && d?.CourierId != 0);

        activeDeliveries
            // 1. Filter out null deliveries and join with Order data
            .Where(d => d != null)
            .Select(d => new { Delivery = d!, Order = s_dal.Order.Read(d!.OrderId) })

            // 2. Filter out missing orders
            .Where(item => item.Order != null)

            // 3. Calculate physics and estimated arrival time
            .Select(item =>
            {
                double dist = item.Delivery.Distance ?? Tools.CalculateAirDistance(item.Order!.Latitude, item.Order.Longitude);

                double speed = item.Delivery.DeliveryShippingType switch
                {
                    DO.Enums.ShippingType.Car => s_dal.Config.AvgCarSpeed,
                    DO.Enums.ShippingType.Motorcycle => s_dal.Config.AvgMotorcycleSpeed,
                    DO.Enums.ShippingType.Bicycle => s_dal.Config.AvgBicycleSpeed,
                    _ => s_dal.Config.AvgWalkSpeed
                };
                if (speed <= 0) speed = 1;

                double hoursNeeded = dist / speed;
                DateTime estimatedArrival = item.Delivery.StartDeliveryTime.AddHours(hoursNeeded);

                return new { item.Delivery, EstimatedArrival = estimatedArrival };
            })

            // 4. Filter only deliveries that have reached their destination by the new clock time
            .Where(item => newClock >= item.EstimatedArrival)

            // 5. Execute side-effects (Update DAL)
            .ToList()
            .ForEach(item =>
            {
                s_dal.Delivery.Update(item.Delivery with
                {
                    EndOrderTime = item.EstimatedArrival,
                    EndType = DO.Enums.ShipmentCompletionStatus.Provided
                });

                // Notify specific order update so single-item windows refresh
                Observers.NotifyItemUpdated(item.Delivery.OrderId);
            });

        Observers.NotifyListUpdated(); // Observer for list
    }
}