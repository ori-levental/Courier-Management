using Dal;
using DalApi;
using DO;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DalTest;

internal class Program
{
    private static ICourier? s_dalCourier = new CourierImplementation();
    private static IDelivery? s_dalDelivery = new DeliveryImplementation();
    private static IOrder? s_dalOrder = new OrderImplementation();
    private static IConfig? s_dalConfig = new ConfigImplementation();
    private enum MainMenu
    {
        Exit,
        Courier,
        Delivery,
        Order,
        Init,
        PrintAll,
        Config,
        Reset
    }
    private enum CrudMenu
    {
        Back,
        Add,
        Show,
        ShowAll,
        Update,
        Delete,
        DeleteAll
    }
    private enum ConfigMenuOptions
    {
        Back,
        AdvanceClockBy1Minute,
        AdvanceClockBy1Hour,
        AdvanceClock1day,
        ShowCurrentClockValue,
        SetMaxAirDistance,
        ShowMaxAirDistance,
        ResetAllConfigurationsToDefault
    }

    static void Main(string[] args)
    {
        // Force Gregorian calendar and US date/number formatting
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        try
        {
            MainMenu choice;
            do
            {
                Console.WriteLine("\n--- Main Menu ---");
                Console.WriteLine(
                    "1. Courier Menu\n" +
                    "2. Delivery Menu\n" +
                    "3. Order Menu\n" +
                    "4. Initialize Data (Random)\n" +
                    "5. Print All Data\n" +
                    "6. Config Menu\n" +
                    "7. Reset All Data\n" +
                    "0. Exit"
                );
                Console.WriteLine("-----------------");
                choice = (MainMenu)GetInt("\nyour choice (0-7)");
                if (choice == 0) break;
                switch (choice)
                {
                    case MainMenu.Courier: CourierMenu(); break;
                    case MainMenu.Delivery: DeliveryMenu(); break;
                    case MainMenu.Order: OrderMenu(); break;
                    case MainMenu.Init: Init(); break;
                    case MainMenu.PrintAll: PrintAll(); break;
                    case MainMenu.Config: ConfigMenu(); break;
                    case MainMenu.Reset: Reset(); break;
                }

            } while (choice != MainMenu.Exit);
        }
        catch (Exception exp)
        { Console.WriteLine($"\n*** ERROR: {exp.Message} ***\n"); }
    }

    internal static void CourierMenu()
    {
        CrudMenu choice;
        do
        {
            Console.WriteLine("\n--- Courier Menu ---");
            Console.WriteLine(
                "1. Add Courier\n" +
                "2. Show Courier (by ID)\n" +
                "3. Show All Couriers\n" +
                "4. Update Courier\n" +
                "5. Delete Courier\n" +
                "6. Delete All Couriers\n" +
                "0. Back to Main Menu"
            );
            Console.WriteLine("--------------------");
            choice = (CrudMenu)GetInt("\nyour choice (0-6)");
            if (choice == CrudMenu.Back) break;
            try // Add try-catch block to handle DAL exceptions gracefully
            {
                switch (choice)
                {
                    case CrudMenu.Add: AddCourier(); break;
                    case CrudMenu.Show: ShowCourier(); break;
                    case CrudMenu.ShowAll: ShowAllCourier(); break;
                    case CrudMenu.Update: UpdateCourier(); break;
                    case CrudMenu.Delete: DeleteCourier(); break;
                    case CrudMenu.DeleteAll: DeleteAllCourier(); break;
                }
            }
            catch (Exception exp)
            { Console.WriteLine($"\n*** ERROR: {exp.Message} ***\n"); }
        } while (true);
    }
    internal static void OrderMenu()
    {
        CrudMenu choice;
        do
        {
            Console.WriteLine("\n--- Order Menu ---");
            Console.WriteLine(
                "1. Add Order\n" +
                "2. Show Order (by ID)\n" +
                "3. Show All Orders\n" +
                "4. Update Order\n" +
                "5. Delete Order\n" +
                "6. Delete All Orders\n" +
                "0. Back to Main Menu"
            );
            Console.WriteLine("------------------");
            choice = (CrudMenu)GetInt("\nyour choice (0-6)");
            if (choice == CrudMenu.Back) break;
            try // Add try-catch block
            {
                switch (choice)
                {
                    case CrudMenu.Add: AddOrder(); break;
                    case CrudMenu.Show: ShowOrder(); break;
                    case CrudMenu.ShowAll: ShowAllOrder(); break;
                    case CrudMenu.Update: UpdateOrder(); break;
                    case CrudMenu.Delete: DeleteOrder(); break;
                    case CrudMenu.DeleteAll: DeleteAllOrder(); break;
                }
            }
            catch (Exception exp)
            { Console.WriteLine($"\n*** ERROR: {exp.Message} ***\n"); }
        } while (true);
    }
    internal static void DeliveryMenu()
    {
        CrudMenu choice;
        do
        {
            Console.WriteLine("\n--- Delivery Menu ---");
            Console.WriteLine(
                "1. Add Delivery\n" +
                "2. Show Delivery (by ID)\n" +
                "3. Show All Deliveries\n" +
                "4. Update Delivery\n" +
                "5. Delete Delivery\n" +
                "6. Delete All Deliveries\n" +
                "0. Back to Main Menu"
            );
            Console.WriteLine("---------------------");
            choice = (CrudMenu)GetInt("\nyour choice (0-6)");
            if (choice == CrudMenu.Back) break;
            try // Add try-catch block
            {
                switch (choice)
                {
                    case CrudMenu.Add: AddDelivery(); break;
                    case CrudMenu.Show: ShowDelivery(); break;
                    case CrudMenu.ShowAll: ShowAllDelivery(); break;
                    case CrudMenu.Update: UpdateDelivery(); break;
                    case CrudMenu.Delete: DeleteDelivery(); break;
                    case CrudMenu.DeleteAll: DeleteAllDelivery(); break;
                }
            }
            catch (Exception exp)
            { Console.WriteLine($"\n*** ERROR: {exp.Message} ***\n"); }
        } while (true);
    }
    internal static void Init()
    {
        Initialization.Do(s_dalCourier, s_dalDelivery, s_dalOrder, s_dalConfig);
    }
    internal static void PrintAll()
    {
        Console.WriteLine("\n--- Printing All Data ---");
        ShowAllCourier();
        ShowAllDelivery();
        ShowAllOrder();
    }
    internal static void ConfigMenu()
    {
        ConfigMenuOptions choice;
        do
        {
            Console.WriteLine("\n--- Config Menu ---");
            Console.WriteLine(
                "1. Advance Clock by 1 Minute\n" +
                "2. Advance Clock by 1 Hour\n" +
                "3. Advance Clock 1 day\n" +
                "4. Show Current Clock Value\n" +
                "5. Set Max Air Distance\n" +
                "6. Show Max Air Distance Value\n" +
                "7. Reset All configurations to default\n" +
                "0. Back to Main Menu"
            );
            Console.WriteLine("-----------------");
            choice = (ConfigMenuOptions)GetInt("\nyour choice (0-7)");

            if (choice == ConfigMenuOptions.Back) break;

            switch (choice)
            {
                case ConfigMenuOptions.AdvanceClockBy1Minute:
                    // DateTime is immutable, must re-assign the new value
                    s_dalConfig!.Clock = s_dalConfig.Clock.AddMinutes(1);
                    Console.WriteLine($"Clock advanced to: {s_dalConfig.Clock}");
                    break;
                case ConfigMenuOptions.AdvanceClockBy1Hour:
                    s_dalConfig!.Clock = s_dalConfig.Clock.AddHours(1);
                    Console.WriteLine($"Clock advanced to: {s_dalConfig.Clock}");
                    break;
                case ConfigMenuOptions.AdvanceClock1day:
                    s_dalConfig!.Clock = s_dalConfig.Clock.AddDays(1);
                    Console.WriteLine($"Clock advanced to: {s_dalConfig.Clock}");
                    break;
                case ConfigMenuOptions.ShowCurrentClockValue:
                    Console.WriteLine($"Current Clock Value: {s_dalConfig!.Clock}");
                    break;
                case ConfigMenuOptions.SetMaxAirDistance:
                    double? valueToSet = GetDouble("value to set");
                    s_dalConfig!.MaxAirDistance = valueToSet;
                    Console.WriteLine($"Max Air Distance set to: {s_dalConfig.MaxAirDistance}");
                    break;
                case ConfigMenuOptions.ShowMaxAirDistance:
                    Console.WriteLine($"Current Max Air Distance: {s_dalConfig!.MaxAirDistance}");
                    break;
                case ConfigMenuOptions.ResetAllConfigurationsToDefault:
                    s_dalConfig!.Reset();
                    Console.WriteLine("Configuration reset to defaults.");
                    break;
            }
        }
        while (true);
    }
    internal static void Reset()
    {
        s_dalCourier!.DeleteAll();
        s_dalDelivery!.DeleteAll();
        s_dalOrder!.DeleteAll();
        s_dalConfig!.Reset();
        Console.WriteLine("\nAll data has been reset.");
    }

    // --- Courier ---
    private static void AddCourier()
    {
        int id = GetInt("ID");
        string fullName = GetString("Full Name");
        string PhoneNumber = GetString("Phone Number");
        string Email = GetString("Email");
        string Password = GetString("Password");
        bool Active = GetBoolean("Active");
        double? DistanceToDelivery = GetDouble("Max Distance");
        Enums.ShippingType DeliveryType = GetShippingType("Delivery Type");
        DateTime EmploymentStartDate = GetDateTime("Employment Start Date");

        Courier newCourier = new Courier(id, fullName, PhoneNumber, Email, Password, Active, DistanceToDelivery, DeliveryType, EmploymentStartDate);
        s_dalCourier!.Create(newCourier);
        Console.WriteLine("\nCourier was added.");
    }
    private static void ShowCourier()
    {
        int id = GetInt("ID to show");
        var courier = s_dalCourier!.Read(id);
        Console.WriteLine(courier != null ? courier.ToString() : "Courier not found.");
    }
    private static void ShowAllCourier()
    {
        Console.WriteLine("\n--- All Couriers ---");
        var couriers = s_dalCourier?.ReadAll();
        if (couriers != null && couriers.Count > 0)
        {
            foreach (var courier in couriers)
                Console.WriteLine(courier);
        }
        else
        {
            Console.WriteLine("Couriers list is empty.");
        }
    }
    internal static void UpdateCourier()
    {
        int id = GetInt("ID to update");
        var existCourier = s_dalCourier?.Read(id);
        if (existCourier != null)
        {
            Console.WriteLine($"Updating Courier ID: {id}. Enter new values:");
            string fullName = GetString("Full Name");
            string PhoneNumber = GetString("Phone Number");
            string Email = GetString("Email");
            string Password = GetString("Password");
            bool Active = GetBoolean("Active");
            double? DistanceToDelivery = GetDouble("Max Distance");
            Enums.ShippingType DeliveryType = GetShippingType("Delivery Type");
            DateTime EmploymentStartDate = GetDateTime("Employment Start Date");

            Courier UpdateCourier = new Courier(id, fullName, PhoneNumber, Email, Password, Active, DistanceToDelivery, DeliveryType, EmploymentStartDate);
            s_dalCourier!.Update(UpdateCourier);
            Console.WriteLine("\nCourier was updated.");
        }
        else { Console.WriteLine("Courier not found."); }
    }
    internal static void DeleteCourier()
    {
        int id = GetInt("ID of the courier for deletion");
        s_dalCourier!.Delete(id);
        Console.WriteLine("\nCourier was deleted.");

    }
    internal static void DeleteAllCourier()
    {
        s_dalCourier!.DeleteAll();
        Console.WriteLine("\nAll couriers deleted.");
    }

    // --- Order ---
    private static void AddOrder()
    {
        Enums.OrderType OrderType = GetOrderType("Order Type");
        string? Description = GetString("Description");
        string Addres = GetString("Address");
        double Latitude = GetDouble("Latitude");
        double Longitude = GetDouble("Longitude");
        string OrderingName = GetString("Ordering Name");
        string phoneNumber = GetString("Phone Number");
        DateTime StartOrderTime = GetDateTime("Start Order Time");

        // ID is set to 0, DAL logic will assign a new ID
        Order newOrder = new Order(0, OrderType, Description, Addres, Latitude, Longitude, OrderingName, phoneNumber, StartOrderTime);
        s_dalOrder!.Create(newOrder);
        Console.WriteLine("\nOrder was added.");
    }
    private static void ShowOrder()
    {
        int id = GetInt("ID to show");
        var order = s_dalOrder!.Read(id);
        Console.WriteLine(order != null ? order.ToString() : "Order not found.");
    }
    private static void ShowAllOrder()
    {
        Console.WriteLine("\n--- All Orders ---");
        var orders = s_dalOrder?.ReadAll();
        if (orders != null && orders.Count > 0)
        {
            foreach (var order in orders)
                Console.WriteLine(order);
        }
        else
        {
            Console.WriteLine("Orders list is empty.");
        }
    }
    private static void UpdateOrder()
    {
        int id = GetInt("ID to update");
        var existOrder = s_dalOrder?.Read(id);
        if (existOrder != null)
        {
            Console.WriteLine($"Updating Order ID: {id}. Enter new values:");
            Enums.OrderType OrderType = GetOrderType("Order Type");
            string? Description = GetString("Description");
            string Addres = GetString("Address");
            double Latitude = GetDouble("Latitude");
            double Longitude = GetDouble("Longitude");
            string OrderingName = GetString("Ordering Name");
            string phoneNumber = GetString("Phone Number");
            DateTime StartOrderTime = GetDateTime("Start Order Time");

            Order newOrder = new Order(id, OrderType, Description, Addres, Latitude, Longitude, OrderingName, phoneNumber, StartOrderTime);
            s_dalOrder!.Update(newOrder);
            Console.WriteLine("\nOrder was updated.");
        }
        else { Console.WriteLine("Order not found."); }
    }
    private static void DeleteOrder()
    {
        int id = GetInt("ID of the order for deletion");
        s_dalOrder!.Delete(id);
        Console.WriteLine("\nOrder was deleted.");
    }
    private static void DeleteAllOrder()
    {
        s_dalOrder!.DeleteAll();
        Console.WriteLine("\nAll orders deleted.");
    }

    // --- Delivery ---
    private static void AddDelivery()
    {
        int OrderId = GetInt("Order ID");
        int CourierId = GetInt("Courier ID");
        Enums.ShippingType DeliveryType = GetShippingType("Delivery Type");
        DateTime StartOrderTime = GetDateTime("Start Order Time");
        double? Distance = GetDouble("Distance (km)");
        Enums.ShipmentCompletionStatus? EndType = GetShipmentCompletionStatus("End Type (optional, press Enter to skip)");
        DateTime? EndOrderTime = GetNullableDateTime("End Order Time (optional, press Enter to skip)");

        // ID is set to 0, DAL logic will assign a new ID
        Delivery newDelivery = new Delivery(0, OrderId, CourierId, DeliveryType, StartOrderTime, Distance, EndType, EndOrderTime);
        s_dalDelivery!.Create(newDelivery);
        Console.WriteLine("\nDelivery was added.");
    }
    private static void ShowDelivery()
    {
        int id = GetInt("ID to show");
        var delivery = s_dalDelivery!.Read(id);
        Console.WriteLine(delivery != null ? delivery.ToString() : "Delivery not found.");
    }
    private static void ShowAllDelivery()
    {
        Console.WriteLine("\n--- All Deliveries ---");
        var deliverys = s_dalDelivery?.ReadAll();
        if (deliverys != null && deliverys.Count > 0)
        {
            foreach (var delivery in deliverys)
                Console.WriteLine(delivery);
        }
        else
        {
            Console.WriteLine("Deliveries list is empty.");
        }
    }
    private static void UpdateDelivery()
    {
        int id = GetInt("ID to update");
        var existDelivery = s_dalDelivery?.Read(id);
        if (existDelivery != null)
        {
            Console.WriteLine($"Updating Delivery ID: {id}. Enter new values:");
            int OrderId = GetInt("Order ID");
            int CourierId = GetInt("Courier ID");
            Enums.ShippingType DeliveryType = GetShippingType("Delivery Type");
            DateTime StartOrderTime = GetDateTime("Start Order Time");
            double? Distance = GetDouble("Distance (km)");
            Enums.ShipmentCompletionStatus? EndType = GetShipmentCompletionStatus("End Type (optional, press Enter to skip)");
            DateTime? EndOrderTime = GetNullableDateTime("End Order Time (optional, press Enter to skip)");

            Delivery newDelivery = new Delivery(id, OrderId, CourierId, DeliveryType, StartOrderTime, Distance, EndType, EndOrderTime);
            s_dalDelivery!.Update(newDelivery);
            Console.WriteLine("\nDelivery was updated.");
        }
        else { Console.WriteLine("Delivery not found."); }
    }
    private static void DeleteDelivery()
    {
        int id = GetInt("ID of the delivery for deletion");
        s_dalDelivery!.Delete(id);
        Console.WriteLine("\nDelivery was deleted.");
    }
    private static void DeleteAllDelivery()
    {
        s_dalDelivery!.DeleteAll();
        Console.WriteLine("\nAll deliveries deleted.");
    }


    // --- Input Getters (NOTE: Unsafe, assumes valid input) ---
    private static int GetInt(string prompt)
    {
        Console.Write($"{prompt}: ");
        return int.Parse(Console.ReadLine()!);
    }
    private static double GetDouble(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        return double.Parse(Console.ReadLine()!);
    }
    private static string GetString(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        return Console.ReadLine()!;
    }
    private static bool GetBoolean(string prompt)
    {
        Console.Write($"{prompt} (true/false): ");
        return bool.Parse(Console.ReadLine()!);
    }
    private static Enums.ShippingType GetShippingType(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        return (Enums.ShippingType)Enum.Parse(typeof(Enums.ShippingType), input, true);
    }
    private static Enums.OrderType GetOrderType(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        return (Enums.OrderType)Enum.Parse(typeof(Enums.OrderType), input, true);
    }
    private static Enums.ShipmentCompletionStatus? GetShipmentCompletionStatus(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        if (string.IsNullOrEmpty(input)) // Allow skipping for nullable enums
            return null;
        return (Enums.ShipmentCompletionStatus)Enum.Parse(typeof(Enums.ShipmentCompletionStatus), input, true);
    }
    private static DateTime GetDateTime(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        return DateTime.Parse(input);
    }

    // Special getter for nullable DateTime
    private static DateTime? GetNullableDateTime(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        if (string.IsNullOrEmpty(input)) // Allow skipping for nullable DateTime
            return null;
        return DateTime.Parse(input);
    }
}