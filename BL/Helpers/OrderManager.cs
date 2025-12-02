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

    private static BO.Order DOToBOOrder(DO.Order doOrder)
    {
        // To get full details, we need to calculate status logic
        var delivery = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id).MaxBy(d => d?.StartDeliveryTime);

        // Calculate status
        ShipmentCompletionStatus status;
        if (delivery == null) status = ShipmentCompletionStatus.NotFound; // Means "Open" / No delivery yet
        else if (delivery.EndOrderTime == null) status = ShipmentCompletionStatus.Failed; // In Progress (Need to map correctly)
        else status = (ShipmentCompletionStatus)delivery.EndType!;

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
            OrderStatus = status,
            // Add other calculations like ScheduleStatus if needed
        };
    }

    private static BO.OrderInList DOToOrderInList(DO.Order doOrder)
    {
        // Logic to calculate status based on latest delivery
        var delivery = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id).MaxBy(d => d?.StartDeliveryTime);

        ShipmentCompletionStatus status = ShipmentCompletionStatus.NotFound; // Default for Open
        if (delivery != null)
        {
            if (delivery.EndOrderTime == null)
                // It is in progress. Map to a status that represents 'In Progress' in your Enum (e.g., Failed or add InProgress)
                status = ShipmentCompletionStatus.Failed;
            else
                status = (ShipmentCompletionStatus)delivery.EndType!;
        }

        return new BO.OrderInList
        {
            OrderId = doOrder.Id,
            OrderType = (OrderType)doOrder.Type,
            OrderStatus = status,
            // Calculate other fields if needed
        };
    }

    #endregion Data Translation

    #region CRUD & Status Logic

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
        // Logic: Find latest delivery
        var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId);
        var delivery = deliveries.MaxBy(d => d?.StartDeliveryTime);

        // Case 1: Order is Open (No delivery exists) -> Create Mock Cancelled Delivery
        if (delivery == null)
        {
            DO.Delivery mockDelivery = new DO.Delivery
            {
                OrderId = orderId,
                CourierId = 0, // Mock ID
                StartDeliveryTime = Helpers.AdminManager.Now,
                EndOrderTime = Helpers.AdminManager.Now,
                EndType = DO.Enums.ShipmentCompletionStatus.Cancelled
            };
            s_dal.Delivery.Create(mockDelivery);
            return;
        }

        // Case 2: Order is In Progress or Closed
        if (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Provided)
            throw new BO.BlCannotCancel("ERROR: cannot cancel an order that has been provided");

        if (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Cancelled)
            throw new BO.BlCannotCancel("ERROR: order is already cancelled");

        // Update to Cancelled
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

    // Replaces OrderIsOpen logic
    internal static void OrderSelection(int courierId, int orderId)
    {
        // 1. Check if order is already handled
        var deliveries = s_dal.Delivery.ReadAll(d => d?.OrderId == orderId);
        if (deliveries.Any(d => d?.EndOrderTime == null || d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided))
        {
            throw new BO.BlAlreadyExistsException("Order is already being handled or completed.");
        }

        // 2. Validate Courier
        if (s_dal.Courier.Read(courierId) == null)
            throw new BO.BlDoesNotExistException("Courier not found");

        // 3. Create new Delivery
        DO.Delivery newDelivery = new DO.Delivery
        {
            OrderId = orderId,
            CourierId = courierId,
            StartDeliveryTime = Helpers.AdminManager.Now,
            DeliveryShippingType = DO.Enums.ShippingType.Motorcycle // Default or derived
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

        // Filter
        if (filterBy != null && filterValue != null)
        {
            if (filterBy == OrderInListEnum.OrderStatus)
                query = query.Where(o => o.OrderStatus == (ShipmentCompletionStatus)filterValue);
            // Add other filters as needed
        }

        // Sort
        if (sortBy != null)
        {
            query = sortBy switch
            {
                OrderInListEnum.OrderId => query.OrderBy(o => o.OrderId),
                OrderInListEnum.OrderStatus => query.OrderBy(o => o.OrderStatus),
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

            // Check if open (no active delivery)
            bool isOpen = !s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id && d?.EndOrderTime == null).Any();
            bool isProvided = s_dal.Delivery.ReadAll(d => d?.OrderId == doOrder.Id && d?.EndType == DO.Enums.ShipmentCompletionStatus.Provided).Any();

            if (!isOpen || isProvided) continue;

            double dist = Tools.CalculateAirDistance(doOrder.Latitude, doOrder.Longitude);

            if (dist <= maxDist)
            {
                result.Add(new BO.OpenOrderInList
                {
                    OrderId = doOrder.Id,
                    OrderType = (OrderType)doOrder.Type,
                    FullAddress = doOrder.CustomerAddress,
                    AirDistance = dist,
                    // Additional fields...
                });
            }
        }

        var query = result.AsEnumerable();

        // Filter
        if (filterBy != null)
        {
            query = query.Where(o => o.OrderType == filterBy.Value);
        }

        // Sort
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
            FullAddress = s_dal.Order.Read(d.OrderId)?.CustomerAddress ?? ""
        });

        // Filter
        if (filterBy != null && filterValue != null)
        {
            if (filterBy == ClosedDeliveryInListEnum.DeliveryEndType)
                list = list.Where(i => i.DeliveryEndType == (ShipmentCompletionStatus)filterValue);
        }

        // Sort
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
        // Reusing your logic exactly
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
        Tools.CheckPhoneNumber(boOrder.PhoneNumber);
    }
}