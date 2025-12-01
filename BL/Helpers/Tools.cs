using BlApi;
using BO;
using DalApi;
using DO;

namespace Helpers;

internal static class Tools
{
    private static IDal s_dal = Factory.Get;

    #region Calculations & Algorithms

    /// <summary>
    /// Calculates the aerial distance between two geographical coordinates using the Haversine formula.
    /// If target coordinates are not provided, defaults to the company's hub location.
    /// </summary>
    /// <param name="lat1">Source latitude.</param>
    /// <param name="lon1">Source longitude.</param>
    /// <param name="lat2">Target latitude (optional).</param>
    /// <param name="lon2">Target longitude (optional).</param>
    /// <returns>Distance in kilometers.</returns>
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
    /// <param name="delivery">The delivery data object containing distance and vehicle type.</param>
    /// <returns>Calculated estimated arrival time.</returns>
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
    /// <param name="order">The order data object.</param>
    /// <returns>The deadline DateTime.</returns>
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

    /// <summary>
    /// Determines the current schedule status (OnTime, Late, or InRisk) relative to the simulated clock.
    /// </summary>
    /// <param name="order">The order details.</param>
    /// <param name="delivery">The delivery details.</param>
    /// <returns>The calculated ScheduleStatus enum.</returns>
    private static ScheduleStatus ScheduleStatusCalculate(DO.Order order, DO.Delivery delivery)
    {
        TimeSpan maxDuration = s_dal.Config.MaxDeliveryTime;

        // Scenario A: Delivery is finished (Historically)
        if (delivery.EndType != null && delivery.EndOrderTime != null)
        {
            if (delivery.EndOrderTime - order.StartOrderTime <= maxDuration)
                return ScheduleStatus.OnTime;
            else
                return ScheduleStatus.Late;
        }
        // Scenario B: Delivery is in progress (Real-time check)
        else
        {
            DateTime deadline = order.StartOrderTime + maxDuration;
            // Use AdminManager.Now for simulated time, NOT DAL Clock directly
            DateTime now = Helpers.AdminManager.Now;

            // 1. If we passed the deadline -> Late
            if (now > deadline)
                return ScheduleStatus.Late;

            // 2. If we are close to the deadline (within risk range) -> InRisk
            // Example: Deadline is in 1 hour, RiskRange is 2 hours -> InRisk
            if (deadline - now <= (s_dal.Config.RiskRange))
                return ScheduleStatus.InRisk;

            // 3. Otherwise -> OnTime
            return ScheduleStatus.OnTime;
        }
    }

    #endregion Calculations & Algorithms

    #region Validations

    /// <summary>
    /// Validates that the phone number adheres to the Israeli mobile format (10 digits starting with '05').
    /// </summary>
    /// <param name="phoneNumber">The phone number string to validate.</param>
    /// <exception cref="BO.BLInvalidDataException">Thrown if format is invalid.</exception>
    internal static void CheckPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) ||
            phoneNumber.Length != 10 ||
            phoneNumber[0] != '0' ||
            phoneNumber[1] != '5' ||
            !phoneNumber.All(char.IsDigit))
        {
            throw new BO.BLInvalidDataException("ERROR: Invalid phone number. Must start with '05' and contain 10 digits.");
        }
    }

    /// <summary>
    /// Validates the Israeli ID number using the standard control digit algorithm (Luhn algorithm).
    /// </summary>
    /// <param name="id">The ID number to validate.</param>
    /// <exception cref="BO.BLInvalidDataException">Thrown if the ID is invalid.</exception>
    internal static void CheckId(int id)
    {
        // Basic range check
        if (id < 0)
            throw new BO.BLInvalidDataException("ERROR: Invalid ID number");

        // Pad to 9 digits for algorithm consistency
        string idString = id.ToString().PadLeft(9, '0');
        int sum = 0;

        // Calculate control digit sum
        for (int i = 0; i < 9; i++)
        {
            int digit = idString[i] - '0';
            int weight = (i % 2 == 0) ? 1 : 2;
            int step = digit * weight;

            if (step > 9)
                step -= 9;

            sum += step;
        }

        // Validate modulo 10
        if (sum % 10 != 0)
            throw new BO.BLInvalidDataException("ERROR: Invalid ID number");
    }

    /// <summary>
    /// Verifies that the requester has administrative privileges.
    /// </summary>
    /// <param name="requesterId">The ID of the user performing the action.</param>
    /// <exception cref="BO.BLAccessPermission">Thrown if the user is not a manager.</exception>
    internal static void AccessPermissionToManager(int requesterId)
    {
        if (requesterId != DalApi.Factory.Get.Config.ManagerId)
            throw new BO.BLAccessPermission("ERROR: No access permission");
    }

    #endregion Validations

    #region Business Logic & Object Generation

    /// <summary>
    /// Retrieves the active order for a specific courier and generates a comprehensive Business Object.
    /// Returns null if the courier is not currently handling any active delivery.
    /// </summary>
    /// <param name="courierId">The ID of the courier.</param>
    /// <returns>OrderInProgress object or null.</returns>
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
        DateTime clock = Helpers.AdminManager.Now;

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

            AirDistance = Tools.CalculateAirDistance(order.Latitude, order.Longitude),
            Distance = delivery.Distance,

            StartOrderTime = order.StartOrderTime,
            StartDeliveryTime = delivery.StartDeliveryTime,

            EstimatedArrivalTime = estimated,
            MaxArrivalTime = maxTime,
            TimeLeft = maxTime - clock,
            TimeToComplete = estimated - clock,

            // Fixed logic call for status
            ScheduleStatus = ScheduleStatusCalculate(order, delivery),
        };
    }

    #endregion Business Logic & Object Generation
}