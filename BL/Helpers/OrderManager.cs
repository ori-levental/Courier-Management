using BO;
using DalApi;
using DO;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    private static DO.Order BOToDOOrder(BO.Order BoOrder)
    {
        DO.Order doCourier = new DO.Order()
        {
            Id = BoOrder.Id,
            Description = BoOrder.Description,
            CustomerAddress = BoOrder.FullAddress,
            PhoneNumber = BoOrder.PhoneNumber,
            Latitude = BoOrder.Latitude,
            OrderingName = BoOrder.OrderingName,
            Type = (DO.Enums.OrderType)BoOrder.OrderType!,
            Longitude = BoOrder.Longitude,
            StartOrderTime = BoOrder.StartOrderTime,
        };
        return doCourier;
    }
    private static BO.Order DOToBOOrder(DO.Order doOrder)
    {
        BO.Order boOrder = new BO.Order()
        {
            Id = doOrder.Id,
            Description = doOrder.Description,
            FullAddress = doOrder.CustomerAddress,
            PhoneNumber = doOrder.PhoneNumber,
            Latitude = doOrder.Latitude,
            OrderingName = doOrder.OrderingName,
            OrderType = (OrderType)doOrder.Type!,
            Longitude = doOrder.Longitude,
            StartOrderTime = doOrder.StartOrderTime,
        };
        return boOrder;
    }
    internal static void AddOrder(int requesterId, BO.Order boOrder)
    {
        DO.Order doOrder = BOToDOOrder(boOrder);
        try
        {
            s_dal.Order.Create(doOrder);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Order with ID {doOrder.Id} already exists", ex);
        }
    }
    internal static void OrderIsOpen(int orderId)
    {//להחזיר שגיאה אם ההזמנה סגורה
        IEnumerable<DO.Delivery> deliveries = Factory.Get.Delivery.ReadAll();
        DO.Delivery? delivery = deliveries.Where(Order => Order.OrderId == orderId).FirstOrDefault();
        if (delivery == null)
            throw new BO.BlDoesNotExistException("ERROR: No delivery found for the given order ID");
        if (delivery.EndType == Enums.ShipmentCompletionStatus.Provided)
            throw new BO.BlCannotCancel("ERROR: cannot cancel an order that has been provided to the customer");
        if(delivery.EndType == Enums.ShipmentCompletionStatus.Refused)
            throw new BO.BlCannotCancel("ERROR: cannot cancel an order that has been refused by the customer");
        if(delivery.EndType == Enums.ShipmentCompletionStatus.Cancelled)
            throw new BO.BlCannotCancel("ERROR: cannot cancel an order that has already been cancelled");

        // Create a new Delivery object with updated EndType, then update in DAL
        DO.Delivery updatedDelivery = delivery with { EndType = Enums.ShipmentCompletionStatus.Cancelled,
        EndOrderTime =DateTime.Now};
        Factory.Get.Delivery.Update(updatedDelivery);
    }
    internal static void CancelOrder(int orderId)
    {
        OrderIsOpen(orderId);
    }
    internal static void CloseOrder(int courierId, int orderId)
    {//לאחד מתודות
        IEnumerable<DO.Delivery> deliveries = Factory.Get.Delivery.ReadAll();
        DO.Delivery? delivery = deliveries.Where(Order => Order.OrderId == orderId).FirstOrDefault();
        if (delivery == null)
            throw new BO.BlDoesNotExistException("ERROR: No delivery found for the given order ID");
        if (delivery.CourierId != courierId)
            throw new BO.BlAccessPermission("ERROR: No access permission to close this order");
        if (delivery.EndType == Enums.ShipmentCompletionStatus.Provided)
            throw new BO.BlCannotClose("ERROR: cannot close an order that has been provided to the customer");
        if (delivery.EndType == Enums.ShipmentCompletionStatus.Refused)
            throw new BO.BlCannotClose("ERROR: cannot close an order that has been refused by the customer");
        if (delivery.EndType == Enums.ShipmentCompletionStatus.Cancelled)
            throw new BO.BlCannotClose("ERROR: cannot close an order that has been cancelled");
        // Create a new Delivery object with updated EndType, then update in DAL
        DO.Delivery updatedDelivery = delivery with
        {
            EndType = Enums.ShipmentCompletionStatus.Provided,
            EndOrderTime = DateTime.Now
        };
        Factory.Get.Delivery.Update(updatedDelivery);
    }
    static internal IEnumerable<BO.ClosedDeliveryInList> CloseOrderByCourier(int requesterId, int courierId, OrderType? filteredBy, ClosedDeliveryInListEnum? sortBy)
    {
        IEnumerable<Delivery> deliveries = Factory.Get.Delivery.ReadAll();
        // Fix: Add select clause to the query
        IEnumerable<Delivery> closedDeliveries = from delivery in deliveries
                                                 where delivery.CourierId == courierId
                                                 select delivery;
        if(filteredBy != null)
            closedDeliveries = closedDeliveries.Where(delivery => delivery.DeliveryShippingType == (DO.Enums.ShippingType)filteredBy);
        if(sortBy != null)
        {
            closedDeliveries = sortBy switch
            {
                ClosedDeliveryInListEnum.DeliveryId => closedDeliveries.OrderBy(delivery => delivery.Id),
                ClosedDeliveryInListEnum.OrderId => closedDeliveries.OrderBy(delivery => delivery.OrderId),
                ClosedDeliveryInListEnum.OrderType => closedDeliveries.OrderBy(delivery => delivery.DeliveryShippingType),
                ClosedDeliveryInListEnum.ShippingType => closedDeliveries.OrderBy(delivery => delivery.DeliveryShippingType),
                ClosedDeliveryInListEnum.ActualDistanceKm => closedDeliveries.OrderBy(delivery => delivery.Distance),
                ClosedDeliveryInListEnum.TotalProcessingTime => closedDeliveries.OrderBy(delivery => (delivery.EndOrderTime - delivery.StartDeliveryTime)),
                ClosedDeliveryInListEnum.DeliveryEndType => closedDeliveries.OrderBy(delivery => delivery.EndType),
                ClosedDeliveryInListEnum.DeliveryEndTime => closedDeliveries.OrderBy(delivery => delivery.EndOrderTime),
                _ => closedDeliveries
            };
        }
        else
        {
            closedDeliveries = closedDeliveries.OrderBy(delivery => delivery.EndType);
        }
        return from delivery in closedDeliveries
               select new BO.ClosedDeliveryInList()
               {
                   DeliveryId = delivery.Id,
                   OrderId = delivery.OrderId,
                   OrderType = (OrderType)delivery.DeliveryShippingType,
                   FullAddress = Factory.Get.Order.Read(delivery.OrderId)!.CustomerAddress,
                   ShippingType = (ShippingType)delivery.DeliveryShippingType,
                   ActualDistanceKm = delivery.Distance,
                   TotalProcessingTime = (delivery.EndOrderTime.HasValue && delivery.StartDeliveryTime != default(DateTime))
                       ? delivery.EndOrderTime.Value - delivery.StartDeliveryTime
                       : TimeSpan.Zero,
                   DeliveryEndType = (ShipmentCompletionStatus)delivery.EndType!,
                   DeliveryEndTime = delivery.EndOrderTime
               };
    }
    internal static void AccessPermissionToManager(int requesterId)//tools
    {
        if (requesterId != DalApi.Factory.Get.Config.ManagerId)
            throw new BO.BlAccessPermission("ERROR: No access permission");
    }

    internal static void CheckCorrectnessVariables(BO.Order boOrder)
    {
        // Execute all individual property validations
        CheckPhoneNumber(boOrder.PhoneNumber);
        CheckAdress(boOrder.FullAddress);
        CheckLatitude(boOrder.Latitude);
        CheckLongtitude(boOrder.Longitude);
        CheckOrderingName(boOrder.OrderingName);
    }

    internal static void CheckOrderingName(string orderingName)
    {
       if(string.IsNullOrWhiteSpace(orderingName))
            throw new BO.BlInvalidDataException("ERROR: ordering name cannot be empty");
    }

    internal static void CheckAdress(string fullAddress)
    {
       if(string.IsNullOrWhiteSpace(fullAddress))
            throw new BO.BlInvalidDataException("ERROR: address cannot be empty");
    }

    internal static void CheckLongtitude(double longitude)
    {
        if (longitude < -180 || longitude > 180)
            throw new BO.BlInvalidDataException("ERROR: lontitide must be between -180 to 180 degrees");
    }

    internal static void CheckLatitude(double latitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new BO.BlInvalidDataException("ERROR: latitude must be between -90 to 90 degrees");
    }

    internal static void CheckPhoneNumber(string phoneNumber)
    {
        // Validate format: 10 digits, starts with '05', contains only numbers
        if (string.IsNullOrWhiteSpace(phoneNumber) ||
            phoneNumber.Length != 10 ||
            phoneNumber[0] != '0' ||
            phoneNumber[1] != '5' ||
            !phoneNumber.All(char.IsDigit))
        {
            throw new BO.BlInvalidDataException("ERROR: Invalid phone number. Must start with '05' and contain 10 digits.");
        }
    }

    internal static IEnumerable<OpenOrderInList> ListOfOrder(int requesterId,int courierId ,OrderInListEnum? filteredBy, OrderInListEnum? sortBy)
    {
        // 1. Authorization: requester must be either the courier or the manager
        if (requesterId != courierId && requesterId != Factory.Get.Config.ManagerId)
            throw new BO.BlAccessPermission("ERROR: No access permission to list orders for this courier");

        // 2. Read courier
        object? courierObj;
        try
        {
            courierObj = Factory.Get.Courier.Read(courierId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"ERROR: Courier with ID {courierId} does not exist", ex);
        }
        if (courierObj == null)
            throw new BO.BlDoesNotExistException($"ERROR: Courier with ID {courierId} does not exist");

        // Reflection helpers to extract numeric properties if present
        static double GetDoublePropertyOrDefault(object obj, string[] candidates, double defaultValue)
        {
            var t = obj.GetType();
            foreach (var name in candidates)
            {
                var p = t.GetProperty(name);
                if (p != null)
                {
                    var val = p.GetValue(obj);
                    if (val is double d) return d;
                    if (val is float f) return Convert.ToDouble(f);
                    if (val is decimal m) return Convert.ToDouble(m);
                    if (val is int i) return Convert.ToDouble(i);
                    if (val is long l) return Convert.ToDouble(l);
                    if (val is null) continue;
                    // try parse
                    if (double.TryParse(val.ToString(), out var parsed)) return parsed;
                }
            }
            return defaultValue;
        }

        double courierLat = GetDoublePropertyOrDefault(courierObj, new[] { "Latitude", "Lat", "LocationLatitude", "Y" }, 0.0);
        double courierLon = GetDoublePropertyOrDefault(courierObj, new[] { "Longitude", "Lon", "LocationLongitude", "X" }, 0.0);
        double courierMaxDistanceKm = GetDoublePropertyOrDefault(courierObj,
            new[] { "MaxDeliveryDistanceKm", "MaxDistanceKm", "MaxDistance", "DeliveryRangeKm", "MaxDeliveryDistance" },
            double.MaxValue);

        // 3. Read orders and deliveries
        IEnumerable<DO.Order> orders = Factory.Get.Order.ReadAll();
        IEnumerable<DO.Delivery> deliveries = Factory.Get.Delivery.ReadAll();

        // Local helper: Haversine formula (km)
        static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371.0; // Earth radius km
            double ToRad(double deg) => deg * Math.PI / 180.0;
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        // Helper to determine whether a delivery is in a final state
        static bool IsFinal(DO.Enums.ShipmentCompletionStatus? endType)
            => endType == DO.Enums.ShipmentCompletionStatus.Provided
               || endType == DO.Enums.ShipmentCompletionStatus.Refused
               || endType == DO.Enums.ShipmentCompletionStatus.Cancelled;

        // Helper to compute schedule status for an order's latest delivery
        static BO.ScheduleStatus ComputeScheduleStatus(DO.Delivery? latestDelivery, TimeSpan onTimeThreshold)
        {
            if (latestDelivery == null)
                return BO.ScheduleStatus.InRisk;
            if (latestDelivery.StartDeliveryTime == default(DateTime))
                return BO.ScheduleStatus.InRisk;
            if (latestDelivery.EndOrderTime.HasValue)
            {
                var duration = latestDelivery.EndOrderTime.Value - latestDelivery.StartDeliveryTime;
                return duration <= onTimeThreshold ? BO.ScheduleStatus.OnTime : BO.ScheduleStatus.Late;
            }
            return BO.ScheduleStatus.InRisk;
        }

        TimeSpan onTimeThreshold = TimeSpan.FromMinutes(30);
        TimeSpan defaultMaxArrivalWindow = TimeSpan.FromHours(1);

        // 4. Build open orders that fit courier distance
        var openOrders = from o in orders
                         let orderDeliveries = deliveries.Where(d => d.OrderId == o.Id)
                         let latestDelivery = orderDeliveries.OrderByDescending(d => d.StartDeliveryTime).FirstOrDefault()
                         let isOpen = latestDelivery == null || !IsFinal(latestDelivery.EndType)
                         where isOpen
                         let airDistance = HaversineKm(courierLat, courierLon, o.Latitude, o.Longitude)
                         where airDistance <= courierMaxDistanceKm
                         select new
                         {
                             Order = o,
                             LatestDelivery = latestDelivery,
                             AirDistance = airDistance,
                             TotalDeliveries = orderDeliveries.Count()
                         };

        // 5. Optionally filter by OrderType (if supplied)
        if (filteredBy != null)
        {
            openOrders = openOrders.Where(x => x.Order.Type == (DO.Enums.OrderType)filteredBy.Value);
        }

        // 6. Project to BO.OpenOrderInList
        var projected = openOrders.Select(x =>
        {
            var o = x.Order;
            var latest = x.LatestDelivery;
            BO.ScheduleStatus schedule = ComputeScheduleStatus(latest, onTimeThreshold);

            DateTime maxArrivalTime;
            try
            {
                // If DO.Order exposes an estimated or max arrival time via reflection, use it.
                var ordType = o.GetType();
                var maxProp = ordType.GetProperty("MaxArrivalTime") ?? ordType.GetProperty("EstimatedArrivalTime");
                if (maxProp != null && maxProp.GetValue(o) is DateTime dt && dt != default(DateTime))
                    maxArrivalTime = dt;
                else
                    maxArrivalTime = o.StartOrderTime == default(DateTime) ? DateTime.Now.Add(defaultMaxArrivalWindow) : o.StartOrderTime.Add(defaultMaxArrivalWindow);
            }
            catch
            {
                maxArrivalTime = o.StartOrderTime == default(DateTime) ? DateTime.Now.Add(defaultMaxArrivalWindow) : o.StartOrderTime.Add(defaultMaxArrivalWindow);
            }

            TimeSpan timeRemaining = maxArrivalTime - Factory.Get.Config.Clock; // use configured clock for determinism

            var openInList = new BO.OpenOrderInList()
            {
                OrderId = o.Id,
                CourierId = null, // open order not yet assigned
                OrderType = (BO.OrderType)o.Type,
                IsHeavy = false, // DO.Order doesn't expose IsHeavy in provided signature; default false
                FullAddress = o.CustomerAddress,
                AirDistance = x.AirDistance,
                ActualDistance = null,
                ActualTimeEstimation = null,
                MaxArrivalTime = maxArrivalTime,
                TimeRemaining = timeRemaining < TimeSpan.Zero ? TimeSpan.Zero : timeRemaining,
                ScheduleStatus = schedule
            };

            return new { Item = openInList, Meta = x, Schedule = schedule, TimeRemaining = openInList.TimeRemaining };
        });

        // 7. Sort according to sortBy
        IOrderedEnumerable<dynamic> ordered;
        if (sortBy == null)
        {
            // default sort by schedule status then by time remaining ascending
            ordered = projected.OrderBy(p => p.Schedule).ThenBy(p => p.TimeRemaining);
        }
        else
        {
            ordered = sortBy.Value switch
            {
                OrderInListEnum.OrderId => projected.OrderBy(p => p.Item.OrderId),
                OrderInListEnum.OrderType => projected.OrderBy(p => p.Item.OrderType),
                OrderInListEnum.AirDistance => projected.OrderBy(p => p.Item.AirDistance),
                OrderInListEnum.OrderStatus => projected.OrderBy(p => p.Meta.LatestDelivery?.EndType),
                OrderInListEnum.ScheduleStatus => projected.OrderBy(p => p.Schedule),
                OrderInListEnum.TimeRemaining => projected.OrderBy(p => p.TimeRemaining),
                OrderInListEnum.TotalProcessingTime => projected.OrderBy(p => TimeSpan.Zero), // open orders -> no processing time yet
                OrderInListEnum.TotalDeliveries => projected.OrderBy(p => p.Meta.TotalDeliveries),
                _ => projected.OrderBy(p => p.Schedule).ThenBy(p => p.TimeRemaining)
            };
        }

        // 8. Return the ordered BO.OpenOrderInList objects
        return ordered.Select(p => (BO.OpenOrderInList)p.Item);
    }

    internal static BO.Order OrderDetails(int orderId)
    {
        return DOToBOOrder(Factory.Get.Order.Read(orderId)!);
    }

    internal static void DeleteOrder(int orderId)
    {
        Factory.Get.Order.Delete(orderId);
    }

    internal static void OrderProcessing(int requesterId, int courierId, int orderId)
    {
        // 1. Authorization
        if (requesterId != courierId)
            throw new BO.BlAccessPermission("ERROR: No access permission to take this order for processing");

        // 2. Ensure order exists
        DO.Order? doOrder;
        try
        {
            doOrder = Factory.Get.Order.Read(orderId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"ERROR: Order with ID {orderId} does not exist", ex);
        }
        if (doOrder == null)
            throw new BO.BlDoesNotExistException($"ERROR: Order with ID {orderId} does not exist");

        // 3. Read deliveries for this order and validate state
        IEnumerable<DO.Delivery> deliveries = Factory.Get.Delivery.ReadAll().Where(d => d.OrderId == orderId);

        if (deliveries.Any(d => d.EndType == Enums.ShipmentCompletionStatus.Provided))
            throw new BO.BlCannotClose("ERROR: cannot start processing an order that has already been provided to the customer");

        if (deliveries.Any(d => d.EndType == Enums.ShipmentCompletionStatus.Refused))
            throw new BO.BlCannotClose("ERROR: cannot start processing an order that was refused by the customer");

        if (deliveries.Any(d => d.EndType == Enums.ShipmentCompletionStatus.Cancelled))
            throw new BO.BlCannotClose("ERROR: cannot start processing an order that has been cancelled");

        // If any delivery exists that is not in a final state, treat as in-progress
        bool inProgress = deliveries.Any(d =>
            d.EndType != Enums.ShipmentCompletionStatus.Provided &&
            d.EndType != Enums.ShipmentCompletionStatus.Refused &&
            d.EndType != Enums.ShipmentCompletionStatus.Cancelled);

        if (inProgress)
            throw new BO.BlAlreadyExistsException("ERROR: order is already assigned or in delivery");

        // 4. Ensure courier exists
        try
        {
            var courier = Factory.Get.Courier.Read(courierId);
            if (courier == null)
                throw new BO.BlDoesNotExistException($"ERROR: Courier with ID {courierId} does not exist");
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"ERROR: Courier with ID {courierId} does not exist", ex);
        }

        // 5. Create DO.Delivery with StartDeliveryTime from system clock
        DO.Delivery newDelivery = new DO.Delivery()
        {
            // Id may be assigned by DAL on Create; set to default 0 if required by the record/ctor
            OrderId = orderId,
            CourierId = courierId,
            StartDeliveryTime = Factory.Get.Config.Clock,
            // EndOrderTime = null, EndType = null -> leave defaults
        };

        // 6. Attempt to create delivery in DAL
        try
        {
            Factory.Get.Delivery.Create(newDelivery);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"ERROR: delivery for order {orderId} already exists", ex);
        }
    }

    internal static void UpdateOrder(BO.Order order)
    {
        DO.Order doOrder = BOToDOOrder(order);
        Factory.Get.Order.Update(doOrder);
    }

    internal static int[] SumAmoutOfOrders()
    {
   

        //  Prepare enums and result array
        var orderStatusValues = Enum.GetValues(typeof(ShipmentCompletionStatus)).Cast<ShipmentCompletionStatus>().ToArray();
        var scheduleStatusValues = Enum.GetValues(typeof(ScheduleStatus)).Cast<ScheduleStatus>().ToArray();
        int orderStatusCount = orderStatusValues.Length;
        int scheduleStatusCount = scheduleStatusValues.Length;
        int[] result = new int[orderStatusCount * scheduleStatusCount];

        // Data
        IEnumerable<DO.Order> orders = Factory.Get.Order.ReadAll();
        IEnumerable<DO.Delivery> deliveries = Factory.Get.Delivery.ReadAll();

        // On-time threshold (deterministic choice)
        TimeSpan onTimeThreshold = TimeSpan.FromMinutes(30);

        // Compute combined index per order, then GroupBy index (using LINQ GroupBy)
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

        // Group by index and count
        var groups = indexed.GroupBy(i => i).Select(g => new { Index = g.Key, Count = g.Count() });

        foreach (var g in groups)
        {
            if (g.Index >= 0 && g.Index < result.Length)
                result[g.Index] = g.Count;
        }

        return result;

        // Local helper function to determine schedule status for a delivery
        static ScheduleStatus ComputeScheduleStatus(DO.Delivery? delivery, TimeSpan onTimeThreshold)
        {
            if (delivery == null)
                return ScheduleStatus.InRisk;

            if (delivery.StartDeliveryTime == default(DateTime))
                return ScheduleStatus.InRisk;

            if (delivery.EndOrderTime.HasValue)
            {
                TimeSpan duration = delivery.EndOrderTime.Value - delivery.StartDeliveryTime;
                return duration <= onTimeThreshold ? ScheduleStatus.OnTime : ScheduleStatus.Late;
            }

            return ScheduleStatus.InRisk;
        }
    }
}
