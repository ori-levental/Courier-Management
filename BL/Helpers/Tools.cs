using BO;
using DalApi;
using DO;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using System.Threading.Tasks;          // Required for async/await
using System.Collections.Concurrent;   // Required for thread-safe caching

namespace Helpers;

/// <summary>
/// Static helper class containing general utilities, calculations, and network services.
/// </summary>
internal static class Tools
{
    private static IDal s_dal = Factory.Get;

    // Static HttpClient to prevent socket exhaustion
    private static readonly HttpClient client = new HttpClient();

    // Cache to store previously fetched coordinates to minimize network traffic.
    // Key: Address String, Value: (Latitude, Longitude) or null if not found.
    private static readonly ConcurrentDictionary<string, (double, double)?> _coordinateCache = new();

    /// <summary>
    /// Static constructor to initialize shared resources.
    /// </summary>
    static Tools()
    {
        // Set User-Agent globally once, as required by Nominatim API policies.
        client.DefaultRequestHeaders.Add("User-Agent", "DeliverySystemApp");
    }

    #region Calculations & Algorithms

    /// <summary>
    /// Calculates aerial distance with safety checks for 0 or null coordinates.
    /// Prevents calculation errors when coordinates are missing.
    /// </summary>
    /// <param name="lat1">Source Latitude</param>
    /// <param name="lon1">Source Longitude</param>
    /// <param name="lat2">Target Latitude (optional, defaults to company config)</param>
    /// <param name="lon2">Target Longitude (optional, defaults to company config)</param>
    /// <returns>Distance in Kilometers</returns>
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
    /// Estimates arrival time based on shipping type speed.
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
    /// Calculates max arrival time based on configuration.
    /// Forces use of Config.MaxDeliveryTime instead of logic based on OrderType.
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
        bool isSuccessfullyClosed = delivery != null &&
                                    delivery.EndType != null &&
                                    (delivery.EndType == DO.Enums.ShipmentCompletionStatus.Provided ||
                                     delivery.EndType == DO.Enums.ShipmentCompletionStatus.Refused);

        // Scenario A: Order is successfully closed (Provided/Refused)
        if (isSuccessfullyClosed && delivery!.EndOrderTime != null)
        {
            TimeSpan actualDuration = delivery.EndOrderTime.Value - order.StartOrderTime;
            TimeSpan maxDuration = s_dal.Config.MaxDeliveryTime;

            return (actualDuration <= maxDuration) ? BO.ScheduleStatus.OnTime : BO.ScheduleStatus.Late;
        }
        // Scenario B: Order is Open, Active (OnCare), or was Cancelled
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

                if (value == null)
                {
                    result.AppendLine($"{property.Name}: null");
                    continue;
                }

                if (value is string strValue)
                {
                    result.AppendLine($"{property.Name}: {strValue}");
                }
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
        // 1. Check if the manager logged in
        if (id == s_dal.Config.ManagerId)
        {
            if (password != s_dal.Config.ManagerPassword)
                throw new BO.BlInvalidDataException("ERROR: Incorrect manager password");

            return;
        }

        // 2. Check if a courier logged in
        DO.Courier? doCourier = s_dal.Courier.Read(id);

        if (doCourier == null || password != doCourier.Password)
            throw new BO.BlInvalidDataException("ERROR: Incorrect user ID or password");
    }

    /// <summary>
    /// Async method to get coordinates from OpenStreetMap (Nominatim).
    /// Uses caching to prevent redundant network requests.
    /// </summary>
    /// <param name="address">The address string to search for.</param>
    /// <returns>A tuple of (Latitude, Longitude) or null if not found/error.</returns>
    public static async Task<(double Latitude, double Longitude)?> GetCoordinatesAsync(string address)
    {
        // 1. Check Cache first
        if (_coordinateCache.TryGetValue(address, out var cachedResult))
        {
            return cachedResult;
        }

        string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

        try
        {
            // 2. Perform Async Network Request
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                // Network failed or bad request - cache as null and return
                _coordinateCache[address] = null;
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement root = doc.RootElement;

                // Check if the results array is empty (Address not found)
                if (root.GetArrayLength() == 0)
                {
                    _coordinateCache[address] = null;
                    return null;
                }

                double lat = double.Parse(
                    root[0].GetProperty("lat").GetString()!,
                    CultureInfo.InvariantCulture);

                double lon = double.Parse(
                    root[0].GetProperty("lon").GetString()!,
                    CultureInfo.InvariantCulture);

                var result = (lat, lon);

                // 3. Save to Cache
                _coordinateCache[address] = result;

                return result;
            }
        }
        catch
        {
            // In case of any network/parsing exception, return null to allow BL to handle gracefully
            return null;
        }
    }
}