using BlApi;
using BO;
using DalApi;
using DO;

namespace Helpers;

internal static class Tools
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Calculates the aerial distance between two geographical coordinates using the Haversine formula.
    /// If target coordinates are not provided, defaults to the company's hub location.
    /// </summary>
    internal static double CalculateAirDistance(double lat1, double lon1, double? lat2 = null, double? lon2 = null)
    {
        // Set target location: use provided coordinates or fallback to Hub configuration
        double targetLat = lat2 ?? s_dal.Config.Latitude ?? 0;
        double targetLon = lon2 ?? s_dal.Config.Longitude ?? 0;

        var r = 6371; // Earth radius in km
        var dLat = (targetLat - lat1) * (Math.PI / 180);
        var dLon = (targetLon - lon1) * (Math.PI / 180);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(targetLat * (Math.PI / 180)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return r * c;
    }

    /// <summary>
    /// Estimates the arrival time based on the distance and the average speed of the courier's vehicle type.
    /// </summary>
    private static DateTime EstimatedArrivalTimeCalculate(DO.Delivery delivery)
    {
        // Determine speed based on shipping type
        double speed = delivery.DeliveryShippingType switch
        {
            DO.Enums.ShippingType.Walk => s_dal.Config.AvgWalkSpeed,
            DO.Enums.ShippingType.Bicycle => s_dal.Config.AvgBicycleSpeed,
            DO.Enums.ShippingType.Car => s_dal.Config.AvgCarSpeed,
            _ => s_dal.Config.AvgMotorcycleSpeed
        };

        if (speed <= 0) speed = 1; // Validation to prevent division by zero

        double travelTimeInHours = delivery.Distance!.Value / speed;

        // Calculate arrival time relative to the delivery start time
        return delivery.StartDeliveryTime.AddHours(travelTimeInHours);
    }

    /// <summary>
    /// Calculates the maximum allowable delivery time (SLA) based on the order's urgency.
    /// </summary>
    private static DateTime MaxArrivalTimeCalculate(DO.Order order)
    {
        double MaxTime = order.Type switch
        {
            DO.Enums.OrderType.SameDay => 24,
            DO.Enums.OrderType.Express => 24 * 3,
            DO.Enums.OrderType.Scheduled => 24 * 7,
            _ => 24 * 14
        };
        return order.StartOrderTime.AddHours(MaxTime);
    }

    private static ScheduleStatus ScheduleStatusCalculate()
    {

    }

    /// <summary>
    /// Retrieves the active order for a specific courier and generates a comprehensive Business Object.
    /// Returns null if the courier is not currently handling any active delivery.
    /// </summary>
    internal static BO.OrderInProgress? GenerateOrderInProgress(int courierId)
    {
        // 1. Retrieve the single active delivery for the courier (where EndOrderTime is null)
        DO.Delivery? delivery = s_dal.Delivery
            .ReadAll(d => d?.CourierId == courierId && d?.EndOrderTime == null)
            .FirstOrDefault();

        // Return null if no active delivery exists
        if (delivery == null) return null;

        // 2. Retrieve the associated order details from the Data Layer
        DO.Order? order = s_dal.Order.Read(delivery.OrderId);

        // Return null if the order record is missing
        if (order == null) return null;

        // Calculate necessary time metrics for the BO
        DateTime estimated = EstimatedArrivalTimeCalculate(delivery);
        DateTime maxTime = MaxArrivalTimeCalculate(order);
        DateTime clock = s_dal.Config.Clock;

        // Construct and return the OrderInProgress object
        return new BO.OrderInProgress
        {
            DeliveryId = delivery.Id,
            OrderId = delivery.OrderId,
            OrderType = (BO.OrderType)order.Type,
            CustomerName = order.OrderingName,
            CustomerPhone = order.PhoneNumber,
            Address = order.CustomerAddress,
            Description = order.Description,

            AirDistance = CalculateAirDistance(order.Latitude, order.Longitude),
            Distance = delivery.Distance,

            StartOrderTime = order.StartOrderTime,
            StartDeliveryTime = delivery.StartDeliveryTime,

            EstimatedArrivalTime = estimated,
            MaxArrivalTime = maxTime,
            TimeLeft = maxTime - clock,
            TimeToComplete = estimated - clock,

            // Calculated Status Logic

            ///////////////////////////////  help   \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

            ScheduleStatus = ScheduleStatus.OnTime



            ///////////////////////////////////\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\
        };
    }
}