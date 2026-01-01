using BO;
using DalApi;
using DO;
using System.Collections; // Required for non-generic IEnumerable
using System.Reflection;  // Required for Reflection
using System.Text;

namespace Helpers;

internal static class Tools
{
    private static IDal s_dal = Factory.Get;

    #region Calculations & Algorithms
    /// <summary>
    /// Calculates aerial distance with safety checks for 0 or null coordinates.
    /// FIX: Prevents calculation errors when coordinates are missing.
    /// </summary>
    internal static double CalculateAirDistance(double lat1, double lon1, double? lat2 = null, double? lon2 = null)
    {
        if (lat1 == 0 && lon1 == 0) return 0;

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
    /// Estimates arrival time.
    /// Handles null Distance to prevent NullReferenceException.
    /// </summary>
    internal static DateTime EstimatedArrivalTimeCalculate(DO.Delivery delivery)
    {
        if (delivery == null) return DateTime.MinValue;

        double speed = delivery.DeliveryShippingType switch
        {
            DO.Enums.ShippingType.Walk => s_dal.Config.AvgWalkSpeed,
            DO.Enums.ShippingType.Bicycle => s_dal.Config.AvgBicycleSpeed,
            DO.Enums.ShippingType.Car => s_dal.Config.AvgCarSpeed,
            _ => s_dal.Config.AvgMotorcycleSpeed
        };

        if (speed <= 0) speed = 1;

        double distance = delivery.Distance ?? 0;

        double travelTimeInHours = distance / speed;
        return delivery.StartDeliveryTime.AddHours(travelTimeInHours);
    }

    /// <summary>
    /// Calculates max arrival time.
    /// Forces use of Config.MaxDeliveryTime (4 hours) instead of logic based on OrderType.
    /// This fixes the bug where orders showed "16 days left" instead of hours.
    /// </summary>
    internal static DateTime MaxArrivalTimeCalculate(DO.Order order)
    {
        return order.StartOrderTime + s_dal.Config.MaxDeliveryTime;
    }


    /// <summary>
    /// Determines the current schedule status (OnTime, Late, or InRisk).
    /// </summary>
    internal static BO.ScheduleStatus ScheduleStatusCalculate(DO.Order order, DO.Delivery? delivery)
    {
        TimeSpan riskRange = s_dal.Config.RiskRange;

        // Logic Check: Has the order been finalized?
        // We consider an order "Closed" for SLA purposes ONLY if it was Provided or Refused by the customer.
        // If it was Cancelled (e.g., courier cancelled), the order is still pending fulfillment, so the clock must keep running.
        bool isSuccessfullyClosed = delivery != null &&
                                    delivery.EndType != null &&
                                    (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||
                                     delivery.EndType == DO.Enums.ShipmentCompletionStatus.Refused);

        // Scenario A: Order is successfully closed (Provided/Refused)
        // We check the historical duration to see if it WAS on time.
        if (isSuccessfullyClosed && delivery!.EndOrderTime != null)
        {
            TimeSpan actualDuration = delivery.EndOrderTime.Value - order.StartOrderTime;
            TimeSpan maxDuration = s_dal.Config.MaxDeliveryTime;

            return (actualDuration <= maxDuration) ? BO.ScheduleStatus.OnTime : BO.ScheduleStatus.Late;
        }

        // Scenario B: Order is Open, Active (OnCare), or was Cancelled (needs re-delivery)
        // We check the current clock against the deadline.
        else
        {
            DateTime deadline = MaxArrivalTimeCalculate(order);
            DateTime now = Helpers.AdminManager.Now;
            TimeSpan timeRemaining = deadline - now;

            // 1. Deadline passed?
            if (timeRemaining < TimeSpan.Zero)
                return BO.ScheduleStatus.Late;

            // 2. Approaching deadline?
            if (timeRemaining <= riskRange)
                return BO.ScheduleStatus.InRisk;

            // 3. Plenty of time left
            return BO.ScheduleStatus.OnTime;
        }
    }   
    #endregion Calculations & Algorithms

    #region Validations

    internal static void CheckPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber) ||
            phoneNumber.Length != 10 ||
            phoneNumber[0] != '0' ||
            phoneNumber[1] != '5' ||
            !phoneNumber.All(char.IsDigit))
        {
            throw new BO.BlInvalidDataException("ERROR: Invalid phone number. Must start with '05' and contain 10 digits.");
        }
    }

    internal static void CheckId(int id)
    {
        if (id < 0)
            throw new BO.BlInvalidDataException("ERROR: Invalid ID number");

        string idString = id.ToString().PadLeft(9, '0');
        int sum = 0;

        for (int i = 0; i < 9; i++)
        {
            int digit = idString[i] - '0';
            int weight = (i % 2 == 0) ? 1 : 2;
            int step = digit * weight;

            if (step > 9) step -= 9;
            sum += step;
        }

        if (sum % 10 != 0)
            throw new BO.BlInvalidDataException("ERROR: Invalid ID number");
    }

    internal static void AccessPermissionToManager(int requesterId)
    {
        if (requesterId != DalApi.Factory.Get.Config.ManagerId)
            throw new BO.BlAccessPermission("ERROR: No access permission");
    }

    #endregion Validations

    #region Business Logic & Object Generation

    internal static BO.OrderInProgress? GenerateOrderInProgress(int courierId)
    {
        DO.Delivery? delivery = s_dal.Delivery
            .ReadAll(d => d?.CourierId == courierId && d?.EndOrderTime == null)
            .FirstOrDefault();

        if (delivery == null) return null;

        DO.Order? order = s_dal.Order.Read(delivery.OrderId);
        if (order == null) return null;

        DateTime estimated = EstimatedArrivalTimeCalculate(delivery);
        DateTime maxTime = MaxArrivalTimeCalculate(order);
        DateTime clock = Helpers.AdminManager.Now;

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
            ScheduleStatus = ScheduleStatusCalculate(order, delivery),
        };
    }

    #endregion Business Logic & Object Generation

    /// <summary>
    /// Generic extension method to generate a string representation of any object's public properties.
    /// Uses Reflection to iterate over properties and handles collections appropriately.
    /// </summary>
    public static string ToStringProperty<T>(this T obj)
    {
        if (obj == null) return "null";

        Type type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var result = new StringBuilder();
        result.AppendLine($"{type.Name}:");
        result.AppendLine(new string('*', type.Name.Length + 1));

        foreach (var property in properties)
        {
            try
            {
                var value = property.GetValue(obj, null);

                // Case 1: Value is null
                if (value == null)
                {
                    result.AppendLine($"{property.Name}: null");
                    continue;
                }

                // Case 2: Value is a String (Treat as single value, not collection)
                if (value is string strValue)
                {
                    result.AppendLine($"{property.Name}: {strValue}");
                }
                // Case 3: Value is a Collection
                else if (value is IEnumerable collection)
                {
                    result.AppendLine($"{property.Name}:");

                    bool isEmpty = true;
                    foreach (var item in collection)
                    {
                        result.AppendLine($"  - {item}");
                        isEmpty = false;
                    }

                    if (isEmpty)
                    {
                        result.AppendLine("  <Empty>");
                    }
                }
                // Case 4: Standard property value (int, double, Enum, etc.)
                else
                {
                    result.AppendLine($"{property.Name}: {value}");
                }
            }
            catch
            {
                result.AppendLine($"{property.Name}: <unable to retrieve>");
            }
        }
        return result.ToString();
    }

    internal static void CheckPasswordEntry(int id, string password)
    {
        // 1. cheack if the manager log in
        if (id == s_dal.Config.ManagerId)
        {
            if (password != s_dal.Config.ManagerPassword)
                throw new BO.BlInvalidDataException("ERROR: Incorrect manager password");

            return;
        }

        // 2. cheack if a courier log in
        DO.Courier? doCourier = s_dal.Courier.Read(id);

        if (doCourier == null || password != doCourier.Password)
            throw new BO.BlInvalidDataException("ERROR: Incorrect user ID or password");
    }

    internal static ScheduleStatus ScheduleStatusCalculate(DO.Order order)
    {
        throw new NotImplementedException();
    }
}