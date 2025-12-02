using BlApi;
using BO;
using System.Globalization;

namespace BlTest;

internal class Program
{
    static readonly IBl s_bl = Factory.Get();

    // Enums
    private enum MainMenu { Exit, Courier, Order, Config }
    private enum CrudMenu { Back, Add, Show, ShowAll, Update, Delete }
    private enum OrderMenuOptions { Back, Add, Cancel, SelectOrder, CloseOrder, Show, ShowAll, Statistics, CourierOpenOrders }
    private enum ConfigMenuOptions { Back, GetClock, ForwardClock, ShowConfig, UpdateConfig, ResetDB, InitDB }

    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Console.WriteLine("=== BL Test Program (Ultimate Optimization) ===");

        RunMenu<MainMenu>("Main Menu", choice => choice switch
        {
            MainMenu.Courier => CourierMenu(),
            MainMenu.Order => OrderMenu(),
            MainMenu.Config => ConfigMenu(),
            MainMenu.Exit => true, // Exit signal
            _ => false
        });
    }

    // --- Entity Menus ---

    private static bool CourierMenu()
    {
        RunMenu<CrudMenu>("Courier Management", choice =>
        {
            if (choice == CrudMenu.Back) return true;
            int reqId = Get<int>("Manager ID");

            switch (choice)
            {
                case CrudMenu.Add:
                    s_bl.Courier.AddCourier(reqId, CreateCourierFromInput());
                    Console.WriteLine("Added.");
                    break;
                case CrudMenu.Show:
                    Console.WriteLine(s_bl.Courier.SearchCourier(reqId, Get<int>("Courier ID")));
                    break;
                case CrudMenu.ShowAll:
                    PrintList(s_bl.Courier.ListOfCourier(reqId, null, null));
                    break;
                case CrudMenu.Update:
                    var c = s_bl.Courier.SearchCourier(reqId, Get<int>("ID to Update"));
                    Console.WriteLine($"Editing {c.FullName}");
                    // Partial update example
                    c.FullName = Get<string>("New Name");
                    c.PhoneNumber = Get<string>("New Phone");
                    s_bl.Courier.UpdateCourier(reqId, c);
                    Console.WriteLine("Updated.");
                    break;
                case CrudMenu.Delete:
                    s_bl.Courier.DeleteCourier(reqId, Get<int>("ID to Delete"));
                    Console.WriteLine("Deleted.");
                    break;
            }
            return false;
        });
        return false; // Return for MainMenu
    }

    private static bool OrderMenu()
    {
        RunMenu<OrderMenuOptions>("Order Management", choice =>
        {
            if (choice == OrderMenuOptions.Back) return true;
            int reqId = Get<int>("Requester ID");

            switch (choice)
            {
                case OrderMenuOptions.Add:
                    s_bl.Order.AddOrder(reqId, CreateOrderFromInput());
                    Console.WriteLine("Added.");
                    break;
                case OrderMenuOptions.Cancel:
                    s_bl.Order.CancelOrder(reqId, Get<int>("Order ID"));
                    Console.WriteLine("Cancelled.");
                    break;
                case OrderMenuOptions.SelectOrder:
                    ((BlImplementation.OrderImplementation)s_bl.Order)
                        .OrderSelection(reqId, reqId, Get<int>("Order ID")); // reqId = courierId
                    Console.WriteLine("Selected.");
                    break;
                case OrderMenuOptions.CloseOrder:
                    s_bl.Order.CloseOrder(reqId, reqId, Get<int>("Delivery ID"));
                    Console.WriteLine("Closed.");
                    break;
                case OrderMenuOptions.Show:
                    Console.WriteLine(s_bl.Order.OrderDetails(reqId, Get<int>("Order ID")));
                    break;
                case OrderMenuOptions.ShowAll:
                    // Interactive Filter
                    var filterBy = Get<string>("Filter by Status? (y/n)") == "y" ? (OrderInListEnum?)OrderInListEnum.Status : null;
                    PrintList(s_bl.Order.ListOfOrder(reqId, filterBy, null));
                    break;
                case OrderMenuOptions.Statistics:
                    var stats = s_bl.Order.SumAmountOfOrders(reqId);
                    for (int i = 0; i < stats.Length; i++) Console.WriteLine($"{(OrderStatus)i}: {stats[i]}");
                    break;
                case OrderMenuOptions.CourierOpenOrders:
                    var impl = (BlImplementation.OrderImplementation)s_bl.Order;
                    PrintList(impl.GetOpenOrdersForCourier(reqId));
                    break;
            }
            return false;
        });
        return false;
    }

    private static bool ConfigMenu()
    {
        RunMenu<ConfigMenuOptions>("Configuration", choice =>
        {
            if (choice == ConfigMenuOptions.Back) return true;

            switch (choice)
            {
                case ConfigMenuOptions.GetClock:
                    Console.WriteLine(s_bl.Admin.GetClock());
                    break;
                case ConfigMenuOptions.ForwardClock:
                    s_bl.Admin.ForwardClock(Get<TimeUnit>("Unit"));
                    Console.WriteLine($"New Time: {s_bl.Admin.GetClock()}");
                    break;
                case ConfigMenuOptions.ShowConfig:
                    Console.WriteLine(s_bl.Admin.GetConfig().ToString());
                    break;
                case ConfigMenuOptions.UpdateConfig:
                    var conf = s_bl.Admin.GetConfig();
                    conf.MaxRange = Get<double>("New Max Range");
                    s_bl.Admin.SetConfig(conf);
                    Console.WriteLine("Updated.");
                    break;
                case ConfigMenuOptions.ResetDB:
                    if (Get<string>("Confirm? (y/n)") == "y") s_bl.Admin.ResetDB();
                    break;
                case ConfigMenuOptions.InitDB:
                    s_bl.Admin.InitializeDB();
                    break;
            }
            return false;
        });
        return false;
    }

    // -----------------------------------------------------------------------
    // Factories (Clean up the switch statements)
    // -----------------------------------------------------------------------

    private static BO.Courier CreateCourierFromInput() => new BO.Courier
    {
        Id = Get<int>("ID"),
        FullName = Get<string>("Name"),
        Email = Get<string>("Email"),
        PhoneNumber = Get<string>("Phone"),
        Password = Get<string>("Password"),
        IsActive = true,
        DistanceToDelivery = Get<double>("Max Distance"),
        DeliveryType = Get<ShippingType>("Vehicle Type"),
        EmploymentStartDate = DateTime.Now
    };

    private static BO.Order CreateOrderFromInput() => new BO.Order
    {
        OrderingName = Get<string>("Customer Name"),
        PhoneNumber = Get<string>("Phone"),
        FullAddress = Get<string>("Address"),
        Latitude = Get<double>("Latitude"),
        Longitude = Get<double>("Longitude"),
        Description = Get<string>("Description"),
        OrderType = Get<OrderType>("Type (Standard/Express)")
    };

    // -----------------------------------------------------------------------
    // Infrastructure (Menu Engine & Universal Input)
    // -----------------------------------------------------------------------

    private static void RunMenu<T>(string title, Func<T, bool> handler) where T : struct, Enum
    {
        while (true)
        {
            Console.WriteLine($"\n--- {title} ---");
            foreach (var o in Enum.GetValues<T>()) Console.WriteLine($"{(int)(object)o}. {o}");

            try
            {
                if (handler(Get<T>("Choice"))) break;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: {ex.Message}" + (ex.InnerException != null ? $" | {ex.InnerException.Message}" : ""));
                Console.ResetColor();
            }
        }
    }

    private static void PrintList<T>(IEnumerable<T> list)
    {
        foreach (var item in list) Console.WriteLine(item);
    }

    // The Universal Getter - Handles all types via pattern matching
    private static T Get<T>(string prompt)
    {
        Console.Write($"{prompt}: ");
        string input = Console.ReadLine() ?? "";

        try
        {
            // Handle String (no parsing needed)
            if (typeof(T) == typeof(string)) return (T)(object)input;

            // Handle Int
            if (typeof(T) == typeof(int)) return (T)(object)int.Parse(input);

            // Handle Double
            if (typeof(T) == typeof(double)) return (T)(object)double.Parse(input);

            // Handle Enum
            if (typeof(T).IsEnum) return (T)Enum.Parse(typeof(T), input, true);

            // Handle DateTime
            if (typeof(T) == typeof(DateTime)) return (T)(object)DateTime.Parse(input);

            throw new InvalidOperationException("Unsupported type");
        }
        catch
        {
            Console.WriteLine("Invalid input, try again.");
            return Get<T>(prompt); // Recursion for retry
        }
    }
}