using Dal;
using DalApi;
using DO;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DalTest;

internal class Program
{
   // static readonly IDal s_dal = new DalList(); //stage 2
    static readonly IDal s_dal = new DalXml(); //stage 3
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

    // --- Generic CRUD Menu ---
    internal static void GenericCrudMenu(string title, Action addAction, Action showAction, Action showAllAction, Action updateAction, Action deleteAction, Action deleteAllAction)
    {
        CrudMenu choice;
        do
        {
            Console.WriteLine($"\n--- {title} Menu ---");
            Console.WriteLine(
                "1. Add\n" +
                "2. Show (by ID)\n" +
                "3. Show All\n" +
                "4. Update\n" +
                "5. Delete\n" +
                "6. Delete All\n" +
                "0. Back to Main Menu"
            );
            Console.WriteLine("--------------------");
            choice = (CrudMenu)GetInt("\nyour choice (0-6)");
            if (choice == CrudMenu.Back) break;
            try // Local catch block to handle errors without exiting menu
            {
                switch (choice)
                {
                    case CrudMenu.Add: addAction(); break;
                    case CrudMenu.Show: showAction(); break;
                    case CrudMenu.ShowAll: showAllAction(); break;
                    case CrudMenu.Update: updateAction(); break;
                    case CrudMenu.Delete: deleteAction(); break;
                    case CrudMenu.DeleteAll: deleteAllAction(); break;
                }
            }
            catch (Exception exp)
            { Console.WriteLine($"\n*** ERROR: {exp.Message} ***\n"); }
        } while (true);
    }

    // --- Refactored CRUD Menus ---
    internal static void CourierMenu()
    {
        GenericCrudMenu("Courier",
            AddCourier, ShowCourier, ShowAllCourier, UpdateCourier, DeleteCourier, DeleteAllCourier);
    }
    internal static void OrderMenu()
    {
        GenericCrudMenu("Order",
            AddOrder, ShowOrder, ShowAllOrder, UpdateOrder, DeleteOrder, DeleteAllOrder);
    }
    internal static void DeliveryMenu()
    {
        GenericCrudMenu("Delivery",
            AddDelivery, ShowDelivery, ShowAllDelivery, UpdateDelivery, DeleteDelivery, DeleteAllDelivery);
    }

    // --- Main Menu Functions ---
    internal static void Init()
    {
        Initialization.Do(s_dal);
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
                    s_dal.Config!.Clock = s_dal.Config.Clock.AddMinutes(1);
                    Console.WriteLine($"Clock advanced to: {s_dal.Config.Clock}");
                    break;
                case ConfigMenuOptions.AdvanceClockBy1Hour:
                    s_dal.Config!.Clock = s_dal.Config.Clock.AddHours(1);
                    Console.WriteLine($"Clock advanced to: {s_dal.Config.Clock}");
                    break;
                case ConfigMenuOptions.AdvanceClock1day:
                    s_dal.Config!.Clock = s_dal.Config.Clock.AddDays(1);
                    Console.WriteLine($"Clock advanced to: {s_dal.Config.Clock}");
                    break;
                case ConfigMenuOptions.ShowCurrentClockValue:
                    Console.WriteLine($"Current Clock Value: {s_dal.Config!.Clock}");
                    break;
                case ConfigMenuOptions.SetMaxAirDistance:
                    double? valueToSet = GetDouble("value to set");
                    s_dal.Config!.MaxAirDistance = valueToSet;
                    Console.WriteLine($"Max Air Distance set to: {s_dal.Config.MaxAirDistance}");
                    break;
                case ConfigMenuOptions.ShowMaxAirDistance:
                    Console.WriteLine($"Current Max Air Distance: {s_dal.Config!.MaxAirDistance}");
                    break;
                case ConfigMenuOptions.ResetAllConfigurationsToDefault:
                    s_dal.Config!.Reset();
                    Console.WriteLine("Configuration reset to defaults.");
                    break;
            }
        }
        while (true);
    }
    internal static void Reset()
    {
        s_dal.Courier!.DeleteAll();
        s_dal.Delivery!.DeleteAll();
        s_dal.Order!.DeleteAll();
        s_dal.Config!.Reset();
       //s_dal.ResetDB(); 
        Console.WriteLine("\nAll data has been reset.");
    }

    // --- Courier Functions ---
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
        s_dal.Courier!.Create(newCourier);
        Console.WriteLine("\nCourier was added.");
    }
    private static void ShowCourier()
    {
        int id = GetInt("ID to show");
        var courier = s_dal.Courier!.Read(id);
        Console.WriteLine(courier != null ? courier.ToString() : "Courier not found.");
    }
    private static void ShowAllCourier()
    {
        Console.WriteLine("\n--- All Couriers ---");
        var couriers = s_dal.Courier?.ReadAll();
        if (couriers != null && couriers.Count() > 0)
        {
            foreach (var courier in couriers)
                Console.WriteLine($"{courier}\n");
        }
        else
        {
            Console.WriteLine("Couriers list is empty.");
        }
    }
    internal static void UpdateCourier()
    {
        int id = GetInt("ID to update");
        var existCourier = s_dal.Courier?.Read(id);
        if (existCourier != null)
        {
            Console.WriteLine($"Current details: {existCourier}");
            Console.WriteLine("Enter new values:");
            string fullName = GetString("Full Name");
            string PhoneNumber = GetString("Phone Number");
            string Email = GetString("Email");
            string Password = GetString("Password");
            bool Active = GetBoolean("Active");
            double? DistanceToDelivery = GetDouble("Max Distance");
            Enums.ShippingType DeliveryType = GetShippingType("Delivery Type");
            DateTime EmploymentStartDate = GetDateTime("Employment Start Date");

            Courier UpdateCourier = new Courier(id, fullName, PhoneNumber, Email, Password, Active, DistanceToDelivery, DeliveryType, EmploymentStartDate);
            s_dal.Courier!.Update(UpdateCourier);
            Console.WriteLine("\nCourier was updated.");
        }
        else { Console.WriteLine("Courier not found."); }
    }
    internal static void DeleteCourier()
    {
        int id = GetInt("ID of the courier for deletion");
        s_dal.Courier!.Delete(id);
        Console.WriteLine("\nCourier was deleted.");

    }
    internal static void DeleteAllCourier()
    {
        s_dal.Courier!.DeleteAll();
        Console.WriteLine("\nAll couriers deleted.");
    }

    // --- Order Functions ---
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

        Order newOrder = new Order(0, OrderType, Description, Addres, Latitude, Longitude, OrderingName, phoneNumber, StartOrderTime);
        s_dal.Order!.Create(newOrder);
        Console.WriteLine("\nOrder was added.");
    }
    private static void ShowOrder()
    {
        int id = GetInt("ID to show");
        var order = s_dal.Order!.Read(id);
        Console.WriteLine(order != null ? order.ToString() : "Order not found.");
    }
    private static void ShowAllOrder()
    {
        Console.WriteLine("\n--- All Orders ---");
        var orders = s_dal.Order?.ReadAll();
        if (orders != null && orders.Count() > 0)
        {
            foreach (var order in orders)
                Console.WriteLine($"{order}\n");
        }
        else
        {
            Console.WriteLine("Orders list is empty.");
        }
    }
    private static void UpdateOrder()
    {
        int id = GetInt("ID to update");
        var existOrder = s_dal.Order?.Read(id);
        if (existOrder != null)
        {
            Console.WriteLine($"Current details: {existOrder}");
            Console.WriteLine("Enter new values:");
            Enums.OrderType OrderType = GetOrderType("Order Type");
            string? Description = GetString("Description");
            string Addres = GetString("Address");
            double Latitude = GetDouble("Latitude");
            double Longitude = GetDouble("Longitude");
            string OrderingName = GetString("Ordering Name");
            string phoneNumber = GetString("Phone Number");
            DateTime StartOrderTime = GetDateTime("Start Order Time");

            Order newOrder = new Order(id, OrderType, Description, Addres, Latitude, Longitude, OrderingName, phoneNumber, StartOrderTime);
            s_dal.Order!.Update(newOrder);
            Console.WriteLine("\nOrder was updated.");
        }
        else { Console.WriteLine("Order not found."); }
    }
    private static void DeleteOrder()
    {
        int id = GetInt("ID of the order for deletion");
        s_dal.Order!.Delete(id);
        Console.WriteLine("\nOrder was deleted.");
    }
    private static void DeleteAllOrder()
    {
        s_dal.Order!.DeleteAll();
        Console.WriteLine("\nAll orders deleted.");
    }

    // --- Delivery Functions ---
    private static void AddDelivery()
    {
        int OrderId = GetInt("Order ID");
        int CourierId = GetInt("Courier ID");
        Enums.ShippingType DeliveryType = GetShippingType("Delivery Type");
        DateTime StartOrderTime = GetDateTime("Start Order Time");
        double? Distance = GetDouble("Distance (km)");
        Enums.ShipmentCompletionStatus? EndType = GetShipmentCompletionStatus("End Type (optional, press Enter to skip)");
        DateTime? EndOrderTime = GetNullableDateTime("End Order Time (optional, press Enter to skip)");

        Delivery newDelivery = new Delivery(0, OrderId, CourierId, DeliveryType, StartOrderTime, Distance, EndType, EndOrderTime);
        s_dal.Delivery!.Create(newDelivery);
        Console.WriteLine("\nDelivery was added.");
    }
    private static void ShowDelivery()
    {
        int id = GetInt("ID to show");
        var delivery = s_dal.Delivery!.Read(id);
        Console.WriteLine(delivery != null ? delivery.ToString() : "Delivery not found.");
    }
    private static void ShowAllDelivery()
    {
        Console.WriteLine("\n--- All Deliveries ---");
        var deliverys = s_dal.Delivery?.ReadAll();
        if (deliverys != null && deliverys.Count() > 0)
        {
            foreach (var delivery in deliverys)
                Console.WriteLine($"{delivery}\n");
        }
        else
        {
            Console.WriteLine("Deliveries list is empty.");
        }
    }
    private static void UpdateDelivery()
    {
        int id = GetInt("ID to update");
        var existDelivery = s_dal.Delivery?.Read(id);
        if (existDelivery != null)
        {
            Console.WriteLine($"Current details: {existDelivery}");
            Console.WriteLine("Enter new values:");
            int OrderId = GetInt("Order ID");
            int CourierId = GetInt("Courier ID");
            Enums.ShippingType DeliveryType = GetShippingType("Delivery Type");
            DateTime StartOrderTime = GetDateTime("Start Order Time");
            double? Distance = GetDouble("Distance (km)");
            Enums.ShipmentCompletionStatus? EndType = GetShipmentCompletionStatus("End Type (optional, press Enter to skip)");
            DateTime? EndOrderTime = GetNullableDateTime("End Order Time (optional, press Enter to skip)");

            Delivery newDelivery = new Delivery(id, OrderId, CourierId, DeliveryType, StartOrderTime, Distance, EndType, EndOrderTime);
            s_dal.Delivery!.Update(newDelivery);
            Console.WriteLine("\nDelivery was updated.");
        }
        else { Console.WriteLine("Delivery not found."); }
    }
    private static void DeleteDelivery()
    {
        int id = GetInt("ID of the delivery for deletion");
        s_dal.Delivery!.Delete(id);
        Console.WriteLine("\nDelivery was deleted.");
    }
    private static void DeleteAllDelivery()
    {
        s_dal.Delivery!.DeleteAll();
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

    // --- Generic Enum Getters ---
    private static T GetEnum<T>(string prompt) where T : struct, Enum
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        return (T)Enum.Parse(typeof(T), input, true);
    }
    private static T? GetNullableEnum<T>(string prompt) where T : struct, Enum
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        if (string.IsNullOrEmpty(input))
            return null;
        return (T)Enum.Parse(typeof(T), input, true);
    }

    // --- Refactored Enum Getters ---
    private static Enums.ShippingType GetShippingType(string prompt)
    {
        return GetEnum<Enums.ShippingType>(prompt);
    }
    private static Enums.OrderType GetOrderType(string prompt)
    {
        return GetEnum<Enums.OrderType>(prompt);
    }
    private static Enums.ShipmentCompletionStatus? GetShipmentCompletionStatus(string prompt)
    {
        return GetNullableEnum<Enums.ShipmentCompletionStatus>(prompt);
    }

    // --- DateTime Getters ---
    private static DateTime GetDateTime(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        return DateTime.Parse(input);
    }
    private static DateTime? GetNullableDateTime(string prompt)
    {
        Console.Write($"Enter {prompt}: ");
        string input = Console.ReadLine()!;
        if (string.IsNullOrEmpty(input))
            return null;
        return DateTime.Parse(input);
    }
}