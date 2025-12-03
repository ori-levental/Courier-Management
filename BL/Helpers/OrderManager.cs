using BO;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;

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
        if (latestDelivery == null) status = ShipmentCompletionStatus.NotFound; // Open
        else if (latestDelivery.EndOrderTime == null) status = ShipmentCompletionStatus.Failed; // In Progress
        else status = (ShipmentCompletionStatus)latestDelivery.EndType!;

        // 3. Calculate Times
        DateTime maxTime = Helpers.Tools.MaxArrivalTimeCalculate(doOrder);
        TimeSpan timeLeft = (maxTime - Helpers.AdminManager.Now);

        // Fix: Clamp negative time if deadline passed or order is completed
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

        ShipmentCompletionStatus status = ShipmentCompletionStatus.NotFound;
        TimeSpan totalProcessing = TimeSpan.Zero;

        if (delivery != null)
        {
            if (delivery.EndOrderTime == null) status = ShipmentCompletionStatus.Failed; // In Progress
            else
            {
                status = (ShipmentCompletionStatus)delivery.EndType!;
                if (status == ShipmentCompletionStatus.Provided)
                    totalProcessing = delivery.EndOrderTime.Value - delivery.StartDeliveryTime;
            }
        }

        // Calculate Time Remaining
        TimeSpan timeRemaining = Tools.MaxArrivalTimeCalculate(doOrder) - Helpers.AdminManager.Now;

        // Fix: Clamp negative time for closed orders
        if (timeRemaining < TimeSpan.Zero || status == ShipmentCompletionStatus.Provided || status == ShipmentCompletionStatus.Cancelled)
            timeRemaining = TimeSpan.Zero;

        return new BO.OrderInList
        {
            OrderId = doOrder.Id,
            DeliveryId = delivery?.Id,
            OrderType = (OrderType)doOrder.Type,
            OrderStatus = status,
            AirDistance = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude),
            TimeRemaining = timeRemaining,
            TotalDeliveries = deliveries.Count(),
            TotalProcessingTime = totalProcessing
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
    }

    internal static void DeleteOrder(int orderId)
    {
        throw new BO.BlCannotCancel("Deleting orders is not allowed.");
    }

    #endregion CRUD & Status Logic

    #region Courier Operations

    internal static void OrderSelection(int courierId, int orderId)
    {
        // 1. Verify Order availability
        var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId);
        if (deliveries.Any(d =>
            d?.EndOrderTime == null ||
            d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||
            d?.EndType == DO.Enums.ShipmentCompletionStatus.Cancelled))
        {
            throw new BO.BlAlreadyExistsException("Order is already being handled, completed, or cancelled.");
        }

        // 2. Validate Courier & Order existence
        DO.Courier? courier = s_dal.Courier.Read(courierId);
        if (courier == null) throw new BO.BlDoesNotExistException("Courier not found");

        DO.Order? order = s_dal.Order.Read(orderId);
        if (order == null) throw new BO.BlDoesNotExistException("Order not found");

        // 3. Calculate Distance (Fix: Calculate and save distance at selection)
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
    }

    internal static void CloseOrder(int courierId, int deliveryId)
    {
        DO.Delivery? delivery = s_dal.Delivery.Read(deliveryId);

        if (delivery == null) throw new BO.BlDoesNotExistException("Delivery not found");
        if (delivery.CourierId != courierId) throw new BO.BlAccessPermission("Courier ID mismatch");

        if (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
            throw new BO.BlCannotClose("ERROR: already provided");

        DO.Delivery updatedDelivery = delivery with
        {
            EndType = DO.Enums.ShipmentCompletionStatus.Provided,
            EndOrderTime = Helpers.AdminManager.Now
        };
        s_dal.Delivery.Update(updatedDelivery);
    }

    #endregion Courier Operations

    #region List Logic (Filters & Sorts)

    internal static IEnumerable<BO.OrderInList> ListOfOrder(OrderInListEnum? filterBy, object? filterValue, OrderInListEnum? sortBy)
    {
        var query = s_dal.Order.ReadAll().Select(o => DOToOrderInList(o!));

        if (filterBy != null && filterValue != null)
        {
            if (filterBy == OrderInListEnum.OrderStatus)
                query = query.Where(o => o.OrderStatus == (ShipmentCompletionStatus)filterValue);
            else if (filterBy == OrderInListEnum.OrderType)
                query = query.Where(o => o.OrderType == (OrderType)filterValue);
        }

        if (sortBy != null)
        {
            query = sortBy switch
            {
                OrderInListEnum.OrderId => query.OrderBy(o => o.OrderId),
                OrderInListEnum.OrderStatus => query.OrderBy(o => o.OrderStatus),
                OrderInListEnum.AirDistance => query.OrderBy(o => o.AirDistance),
                _ => query.OrderBy(o => o.OrderId)
            };
        }

        return query;
    }

    internal static IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(int courierId, OrderType? filterBy, OpenOrderInListEnum? sortBy)
    {
        DO.Courier? courier = s_dal.Courier.Read(courierId);
        if (courier == null) throw new BO.BlDoesNotExistException("Courier not found");

        double maxDist = courier.DistanceToDelivery ?? double.MaxValue;
        var allOrders = s_dal.Order.ReadAll();
        List<BO.OpenOrderInList> result = new();

        foreach (var doOrder in allOrders)
        {
            if (doOrder == null) continue;

            // Filter out active or closed/cancelled orders
            var orderDeliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id);
            bool isActive = orderDeliveries.Any(d => d?.EndOrderTime == null);
            bool isClosed = orderDeliveries.Any(d =>
                d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||
                d?.EndType == DO.Enums.ShipmentCompletionStatus.Cancelled);

            if (isActive || isClosed) continue;

            double dist = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude);

            if (dist <= maxDist)
            {
                DateTime maxTime = Tools.MaxArrivalTimeCalculate(doOrder);

                // Fix: Calculate TimeRemaining with clamp
                TimeSpan timeLeft = maxTime - Helpers.AdminManager.Now;
                if (timeLeft < TimeSpan.Zero) timeLeft = TimeSpan.Zero;

                result.Add(new BO.OpenOrderInList
                {
                    OrderId = doOrder.Id,
                    OrderType = (OrderType)doOrder.Type,
                    FullAddress = doOrder.CustomerAddress,
                    AirDistance = dist,
                    IsHeavy = false,
                    MaxArrivalTime = maxTime,
                    TimeRemaining = timeLeft,
                    ScheduleStatus = ScheduleStatus.OnTime,
                    ActualDistance = dist,
                });
            }
        }

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
                          ? ShipmentCompletionStatus.NotFound
                          : (latestDelivery.EndType.HasValue ? (ShipmentCompletionStatus)latestDelivery.EndType.Value : ShipmentCompletionStatus.NotFound)
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

    internal static void AccessPermissionToManager(int requesterId)
    {
        if (requesterId != DalApi.Factory.Get.Config.ManagerId)
            throw new BO.BlAccessPermission("ERROR: No access permission");
    }

    internal static void CheckCorrectnessVariables(BO.Order boOrder)
    {
        // Fix: Added missing validation checks for Name and Address
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

        foreach (var delivery in activeDeliveries)
        {
            if (delivery == null) continue;

            DO.Order? order = s_dal.Order.Read(delivery.OrderId);
            if (order == null) continue;

            double dist = delivery.Distance ?? Tools.CalculateAirDistance(order.Latitude, order.Longitude);
            double speed = delivery.DeliveryShippingType switch
            {
                DO.Enums.ShippingType.Car => s_dal.Config.AvgCarSpeed,
                DO.Enums.ShippingType.Motorcycle => s_dal.Config.AvgMotorcycleSpeed,
                DO.Enums.ShippingType.Bicycle => s_dal.Config.AvgBicycleSpeed,
                _ => s_dal.Config.AvgWalkSpeed
            };
            if (speed <= 0) speed = 10;

            double hoursNeeded = dist / speed;
            DateTime estimatedArrival = delivery.StartDeliveryTime.AddHours(hoursNeeded);

            if (newClock >= estimatedArrival)
            {
                s_dal.Delivery.Update(delivery with
                {
                    EndOrderTime = estimatedArrival,
                    EndType = DO.Enums.ShipmentCompletionStatus.Provided
                });
            }
        }
    }
}