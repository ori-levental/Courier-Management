namespace DalTest;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Initialization
{
    private static IDal? s_dal;
    private static readonly Random s_rand = new();

    // Helper record for the pre-calculated address list
    private record AddressInfo(string Address, double Latitude, double Longitude, double RoadDistanceKm, double WalkDistanceKm);

    // HQ Location (Config) - Azrieli Center Tel Aviv
    private static readonly double s_hqLat = 32.0749;
    private static readonly double s_hqLon = 34.7923;

    #region dataToFullTheTest
    private static readonly List<AddressInfo> s_addresses = new()
    {
        new("Sarona Market, Tel Aviv", 32.0717, 34.7865, 1.0, 0.9),
        new("Kikar HaMedina, Tel Aviv", 32.0869, 34.7828, 2.0, 1.8),
        new("Habima Square, Tel Aviv", 32.0709, 34.7796, 1.9, 1.7),
        new("Dizengoff Center, Tel Aviv", 32.0760, 34.7770, 1.8, 1.5),
        new("Tel Aviv Museum of Art", 32.0778, 34.7878, 0.9, 0.8),
        new("Ichilov Hospital, Tel Aviv", 32.0831, 34.7876, 1.5, 1.3),
        new("Rabin Square, Tel Aviv", 32.0809, 34.7810, 1.7, 1.5),
        new("Arlozorov Train Station, Tel Aviv", 32.0868, 34.7951, 1.8, 1.6),
        new("Shuk HaCarmel, Tel Aviv", 32.0711, 34.7681, 2.7, 2.2),
        new("Old Jaffa Port", 32.0544, 34.7523, 5.2, 4.8),
        new("Tel Aviv University", 32.1138, 34.8048, 6.1, 5.5),
        new("Ramat Aviv Mall, Tel Aviv", 32.1150, 34.7997, 6.0, 5.5),
        new("Yarkon Park, Tel Aviv", 32.1054, 34.7942, 4.5, 4.0),
        new("Gordon Beach, Tel Aviv", 32.0833, 34.7681, 3.0, 2.7),
        new("Florentin, Tel Aviv", 32.0573, 34.7700, 3.5, 3.0),
        new("Namal Tel Aviv (Port)", 32.0950, 34.7739, 3.3, 3.0),
        new("Bursa (Diamond Exchange), Ramat Gan", 32.0815, 34.7995, 1.4, 1.2),
        new("Ramat Gan City Hall", 32.0827, 34.8113, 2.5, 2.2),
        new("Ayalon Mall, Ramat Gan", 32.1054, 34.8188, 5.0, 4.5),
        new("Bar Ilan University, Ramat Gan", 32.0694, 34.8430, 6.0, 5.5),
        new("Ramat Gan National Park", 32.0632, 34.8175, 3.3, 3.0),
        new("Sheba Medical Center, Tel HaShomer", 32.0468, 34.8468, 7.0, 6.5),
        new("Givatayim Mall", 32.0690, 34.8055, 2.0, 1.8),
        new("Givatayim Observatory", 32.0733, 34.8105, 2.2, 2.0),
        new("Beit Vilnai, Givatayim", 32.0768, 34.8080, 2.1, 1.9),
        new("Coca-Cola Factory, Bnei Brak", 32.0910, 34.8234, 4.0, 3.5),
        new("Mayanei Hayeshua Hospital", 32.0872, 34.8290, 4.3, 3.8),
        new("Design Museum Holon", 32.0116, 34.7816, 8.8, 8.0),
        new("Wolfson Medical Center, Holon", 32.0319, 34.7648, 6.9, 6.0),
        new("Holon Azrieli Center", 32.0163, 34.7954, 8.0, 7.5),
        new("Yamit 2000 Water Park", 32.0001, 34.7709, 10.5, 9.5),
        new("Holon Institute of Technology", 32.0142, 34.7963, 8.5, 7.8),
        new("Bat Yam Beach", 32.0147, 34.7471, 9.5, 8.5),
        new("Bat Yam Mall", 32.0194, 34.7570, 8.5, 7.8),
        new("Rishon LeZion Gold Mall (Kenyon HaZahav)", 31.9723, 34.7607, 14.5, 13.0),
        new("Superland, Rishon LeZion", 31.9840, 34.7538, 13.0, 12.0),
        new("IKEA Rishon LeZion", 31.9701, 34.7702, 14.0, 13.0),
        new("Rishon LeZion City Hall", 31.9634, 34.8078, 15.0, 14.0),
        new("Herzliya Marina", 32.1648, 34.8021, 12.1, 11.5),
        new("Reichman University (IDC), Herzliya", 32.1681, 34.8075, 12.5, 11.8),
        new("Arena Mall, Herzliya", 32.1666, 34.8016, 12.3, 11.7),
        new("Herzliya Medical Center", 32.1691, 34.8000, 12.6, 12.0),
        new("Beilinson Hospital, Petah Tikva", 32.0934, 34.8763, 9.5, 8.5),
        new("Petah Tikva Grand Mall (HaGadol)", 32.0872, 34.8690, 8.5, 7.8),
        new("Petah Tikva City Center", 32.0886, 34.8850, 10.0, 9.0),
        new("Kiryat Arye, Petah Tikva", 32.1000, 34.8480, 7.0, 6.5),
        new("Ramat HaHayal, Tel Aviv", 32.1105, 34.8407, 6.8, 6.0),
        new("Giv'at Shmuel Center", 32.0792, 34.8516, 6.5, 6.0),
        new("Or Yehuda Industrial Zone", 32.0292, 34.8576, 9.0, 8.0),
        new("Ben Gurion Airport (Terminal 3)", 32.0094, 34.8860, 15.0, 14.0),
        new("Kfar Saba Center", 32.1764, 34.9080, 16.0, 15.0),
        new("Ra'anana Park", 32.1869, 34.8660, 15.5, 14.5),
        new("Weizmann Institute, Rehovot", 31.9073, 34.8091, 22.0, 21.0),
        new("Ramat Hasharon Center", 32.1465, 34.8428, 9.0, 8.5),
        new("Kiryat Ono Center", 32.0642, 34.8557, 6.5, 6.0),
        new("Rosh HaAyin North", 32.1086, 34.9520, 15.0, 14.0),
        new("Yehud Center", 32.0310, 34.8880, 10.0, 9.0),
        new("Ness Ziona Center", 31.9281, 34.7980, 17.0, 16.0),
        new("Lod City Hall", 31.9540, 34.8880, 12.0, 11.0),
        new("Ramla Market", 31.9280, 34.8700, 15.0, 14.0)
    };

    private static readonly string[] s_courierNames =
    {
        "Moshe Cohen", "David Levi", "Yossi Mizrahi", "Avraham Biton", "Yaakov Dahan", "Itzhak Friedman", "Shimi Azulai",
        "Israel Israeli", "Eliran Peretz", "Meir Gabai", "Haim Malka","Tomer Ohayon", "Noam Katz", "Matan Segal",
        "Ronen Hadad", "Elad Mor", "Omer Atias", "Gal Dayan", "Lior Maimon", "Matan Ezra"
    };

    private static readonly string[] s_customerNames =
    {
        "Mia Leimberg", "Gabriela Leimberg", "Clara Marman", "Ditza Heiman", "Tami Metzger", "Ofelia Roitman", "Ada Sagi",
        "Norlin Agojo", "Rimon Kirsht Buchstab", "Meirav Tal", "Eitan Yahalomi", "Hanna Katzir", "Doron Katz-Asher",
        "Aviv Katz-Asher", "Raz Katz-Asher", "Ruti Munder", "Keren Munder", "Ohad Munder", "Yaffa Adar", "Adina Moshe",
        "Daniel Aloni", "Emilia Aloni", "Hanna Peri", "Sharon Avigdori", "Noam Avigdori", "Noam Or", "Alma Or",
        "Shiri Weiss", "Noga Weiss", "Emily Hand", "Hila Rotem Shoshani", "Maya Regev", "Shoshan Haran", "Adi Shoham",
        "Nave Shoham", "Avigail Idan", "Hagar Brodutch", "Ofri Brodutch", "Judith Raanan", "Natalie Raanan"
    };
    #endregion

    // Haversine formula for air distance calculation
    private static double GetAirDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = (lat2 - lat1) * (Math.PI / 180);
        var dLon = (lon2 - lon1) * (Math.PI / 180);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * (Math.PI / 180)) * Math.Cos(lat2 * (Math.PI / 180)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    // Gets configured speed by transport type
    private static double GetSpeed(Enums.ShippingType type)
    {
        return type switch
        {
            Enums.ShippingType.Car => s_dal!.Config!.AvgCarSpeed,
            Enums.ShippingType.Motorcycle => s_dal!.Config!.AvgMotorcycleSpeed,
            Enums.ShippingType.Bicycle => s_dal!.Config!.AvgBicycleSpeed,
            Enums.ShippingType.Walk => s_dal!.Config!.AvgWalkSpeed,
            _ => s_dal!.Config!.AvgWalkSpeed
        };
    }

    // Gets estimated real distance (road/walk) by transport type
    private static double GetDistanceByTransport(AddressInfo info, Enums.ShippingType type)
    {
        return type switch
        {
            Enums.ShippingType.Car => info.RoadDistanceKm,
            Enums.ShippingType.Motorcycle => info.RoadDistanceKm,
            Enums.ShippingType.Bicycle => info.WalkDistanceKm,
            Enums.ShippingType.Walk => info.WalkDistanceKm,
            _ => info.RoadDistanceKm
        };
    }

    private static void InitializeConfig()
    {
        s_dal!.Config!.Clock = new DateTime(2024, 11, 1, 12, 0, 0);
        s_dal!.Config!.ManagerId = 111111118;
        s_dal!.Config!.ManagerPassword = "nxnsj544bh@!";
        s_dal!.Config!.CompanyAddress = "Menachem Begin 132, Tel Aviv";
        s_dal!.Config!.Latitude = s_hqLat;
        s_dal!.Config!.Longitude = s_hqLon;
        s_dal!.Config!.MaxAirDistance = 20.0;
        s_dal!.Config!.AvgCarSpeed = 60;
        s_dal!.Config!.AvgMotorcycleSpeed = 80;
        s_dal!.Config!.AvgBicycleSpeed = 20;
        s_dal!.Config!.AvgWalkSpeed = 5;
        s_dal!.Config!.MaxDeliveryTime = TimeSpan.FromHours(4);
        s_dal!.Config!.RiskRange = TimeSpan.FromMinutes(30);
        s_dal!.Config!.CourierInactivityTime = TimeSpan.FromDays(180);
    }

    private static void CreateCouriers()
    {
        foreach (var name in s_courierNames)
        {
            int id;
            do
            {
                // 1. Generate the first 8 digits 
                // (Using range 20M-40M to ensure the final ID starts with 2, 3, or 4 and has 9 digits total)
                int first8Digits = s_rand.Next(20000000, 40000000);

                // 2. Calculate the control digit (Israeli ID / Luhn algorithm)
                int sum = 0;
                string idString = first8Digits.ToString();

                for (int i = 0; i < 8; i++)
                {
                    int digit = idString[i] - '0';
                    int weight = (i % 2 == 0) ? 1 : 2; // Weight alternates: 1, 2, 1, 2...
                    int step = digit * weight;

                    // If result is double-digit, sum its digits (e.g., 12 -> 1+2=3, equivalent to 12-9)
                    if (step > 9)
                        step -= 9;

                    sum += step;
                }

                // Calculate the complement to the nearest multiple of 10
                int checkDigit = (10 - (sum % 10)) % 10;

                // 3. Construct the full 9-digit ID
                id = (first8Digits * 10) + checkDigit;

            } while (s_dal!.Courier!.Read(id) != null); // Ensure uniqueness in the database

            string phone = $"05{s_rand.Next(0, 10)}{s_rand.Next(1000000, 10000000)}";

            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$";
            string password = new string(Enumerable.Range(1, 10)
                .Select(_ => validChars[s_rand.Next(validChars.Length)]).ToArray());
            password = password += "1.B";

            string email = name.Replace(" ", ".").ToLower() + "@example.com";

            bool active = s_rand.Next(0, 10) < 8; // 80% active

            Enums.ShippingType shipping = (Enums.ShippingType)s_rand.Next(0, 4);

            double? distanceToDelivery = null;
            if (s_rand.Next(0, 3) > 0) // ~66% of couriers get a max distance
            {
                // Assign logical distance based on transport type
                distanceToDelivery = shipping switch
                {
                    Enums.ShippingType.Walk => s_rand.Next(1, 6),
                    Enums.ShippingType.Bicycle => s_rand.Next(5, 16),
                    Enums.ShippingType.Motorcycle => s_rand.Next(10, (int)s_dal!.Config!.MaxAirDistance! + 1),
                    Enums.ShippingType.Car => s_rand.Next(10, (int)s_dal!.Config!.MaxAirDistance! + 1),
                    _ => s_rand.Next(5, 11)
                };
            }

            DateTime start = s_dal!.Config!.Clock.AddDays(-s_rand.Next(30, 365 * 5));

            s_dal!.Courier!.Create(new(id, name, phone, email, password, active, distanceToDelivery, shipping, start));
        }
    }

    private static void CreateOrders()
    {
        for (int i = 0; i < 50; i++)
        {
            Enums.OrderType Type = (Enums.OrderType)s_rand.Next(0, 3);
            string? description = "Order for: " + Type.ToString();

            var validAddresses = s_addresses.Where(a =>
                GetAirDistance(s_hqLat, s_hqLon, a.Latitude, a.Longitude) <= s_dal!.Config!.MaxAirDistance
            ).ToList();

            AddressInfo randomAddress = validAddresses[s_rand.Next(validAddresses.Count)];

            string address = randomAddress.Address;
            double latitude = randomAddress.Latitude;
            double longitude = randomAddress.Longitude;

            string orderingName = s_customerNames[s_rand.Next(s_customerNames.Length)];
            string phoneNumber = $"05{s_rand.Next(0, 10)}-{s_rand.Next(1000000, 10000000)}";

            // --- FIX: Realistic Time Initialization ---
            // Create orders ONLY within the last 6 hours relative to the current clock.
            // SLA is 4 hours.
            // 0 - 3.5 hours ago -> OnTime
            // 3.5 - 4.0 hours ago -> InRisk
            // 4.0 - 6.0 hours ago -> Late (Realistic backlog)

            int minutesAgo = s_rand.Next(5, 6 * 60); // Random time between 5 minutes and 6 hours ago
            DateTime startOrderTime = s_dal!.Config!.Clock.AddMinutes(-minutesAgo);

            // ------------------------------------------

            int id_check;
            do id_check = s_rand.Next(1000, 2000);
            while (s_dal!.Order!.Read(id_check) != null);

            s_dal!.Order!.Create(new(0, Type, description, address, latitude, longitude, orderingName, phoneNumber, startOrderTime));
        }
    }
    private static void CreateDeliveries()
    {
        var couriers = s_dal!.Courier.ReadAll();
        var orders = s_dal!.Order.ReadAll().ToList();
        var existingDeliveries = s_dal!.Delivery.ReadAll().ToList();

        int deliveriesToCreate = 30;
        int closedCount = 20;

        for (int i = 0; i < deliveriesToCreate; i++)
        {
            if (orders.Count == 0) break;

            Order order = orders[s_rand.Next(orders.Count)];

            AddressInfo? addressInfo = s_addresses.FirstOrDefault(a => a.Address == order.CustomerAddress);
            if (addressInfo == null) continue;

            double orderAirDistance = GetAirDistance(s_hqLat, s_hqLon, addressInfo.Latitude, addressInfo.Longitude);

            var availableCouriersByDistance = couriers.Where(c =>
            {
                if (!c.Active || !c.DeliveryType.HasValue) return false;
                return !c.DistanceToDelivery.HasValue || c.DistanceToDelivery >= orderAirDistance;
            }).ToList();

            if (availableCouriersByDistance.Count == 0) continue;

            DateTime potentialStartTime;

            if (i < closedCount) // Historical (Closed) Deliveries
            {
                potentialStartTime = order.StartOrderTime.AddMinutes(s_rand.Next(5, 60));
                if (potentialStartTime > s_dal!.Config!.Clock)
                    potentialStartTime = s_dal!.Config!.Clock.AddMinutes(-s_rand.Next(1, 30));
            }
            else // Active (OnCare) Deliveries
            {
                // Failing to do so causes short deliveries (motorcycle/short distance) 
                // to expire immediately upon initialization.
                potentialStartTime = s_dal!.Config!.Clock;
            }
            // -----------------------

            var trulyAvailableCouriers = new List<Courier>();
            var courierDurations = new Dictionary<int, double>();

            foreach (var courier in availableCouriersByDistance)
            {
                double speed = GetSpeed(courier.DeliveryType!.Value);
                double distance = GetDistanceByTransport(addressInfo, courier.DeliveryType.Value);
                double durationInHours = distance / speed;
                DateTime potentialEndTime = potentialStartTime.AddHours(durationInHours);

                var couriersDeliveries = existingDeliveries.Where(d => d.CourierId == courier.Id && d.EndOrderTime == null);

                bool hasOverlap = couriersDeliveries.Any(d =>
                    d.StartDeliveryTime < potentialEndTime && potentialStartTime < (d.EndOrderTime ?? DateTime.MaxValue));

                if (!hasOverlap)
                {
                    trulyAvailableCouriers.Add(courier);
                    courierDurations[courier.Id] = durationInHours;
                }
            }

            if (trulyAvailableCouriers.Count == 0) continue;

            Courier chosenCourier = trulyAvailableCouriers[s_rand.Next(trulyAvailableCouriers.Count)];
            double? deliveryDistance = GetDistanceByTransport(addressInfo, chosenCourier.DeliveryType!.Value);
            double deliveryDurationHours = courierDurations[chosenCourier.Id];

            DateTime? endOrderTime = null;
            Enums.ShipmentCompletionStatus? endType = null;

            if (i < closedCount)
            {
                endOrderTime = potentialStartTime.AddHours(deliveryDurationHours);
                if (endOrderTime > s_dal!.Config!.Clock)
                    endOrderTime = s_dal!.Config!.Clock.AddMinutes(-s_rand.Next(1, 15));

                endType = (Enums.ShipmentCompletionStatus)s_rand.Next(0, 5);
            }

            var newDelivery = new Delivery(0, order.Id, chosenCourier.Id, chosenCourier.DeliveryType.Value, potentialStartTime, deliveryDistance, endType, endOrderTime);
            s_dal!.Delivery!.Create(newDelivery);
            existingDeliveries.Add(newDelivery with { Id = s_dal!.Delivery.ReadAll().Last().Id });
            orders.Remove(order);
        }
    }
    public static void Do()
    {
        s_dal = DalApi.Factory.Get;

        Console.WriteLine("Resetting and Initializing Configuration...");
        s_dal.ResetDB();
        InitializeConfig();

        Console.WriteLine("Creating Couriers...");
        CreateCouriers();

        Console.WriteLine("Creating Orders...");
        CreateOrders();

        Console.WriteLine("Creating Deliveries (linking Orders and Couriers)...");
        CreateDeliveries();

        Console.WriteLine("Initialization complete.");
    }
}