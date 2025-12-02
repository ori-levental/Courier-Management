using BlApi;
using BO;
using System.Globalization;

namespace BlTest;

internal class Program
{
    static readonly IBl s_bl = Factory.Get();

    static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        Console.WriteLine("=== BL Test Program ===");

        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Courier Management");
            Console.WriteLine("2. Order Management");
            Console.WriteLine("3. Admin / Configuration");
            Console.WriteLine("0. Exit");

            int choice = GetInt("Your choice");

            switch (choice)
            {
                case 1: CourierMenu(); break;
                case 2: OrderMenu(); break;
                case 3: ConfigMenu(); break;
                case 0: exit = true; break;
                default: Console.WriteLine("Invalid choice."); break;
            }
        }
    }

    #region Courier Menu
    private static void CourierMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Courier Menu ---");
            Console.WriteLine("1. Add Courier");
            Console.WriteLine("2. Show Courier");
            Console.WriteLine("3. Show All Couriers");
            Console.WriteLine("4. Update Courier");
            Console.WriteLine("5. Delete Courier");
            Console.WriteLine("0. Back");

            int choice = GetInt("Your choice");
            if (choice == 0) return;

            try
            {
                int reqId = GetInt("Manager ID");

                switch (choice)
                {
                    case 1: // Add
                        var newCourier = new BO.Courier
                        {
                            Id = GetInt("ID"),
                            FullName = GetString("Name"),
                            Email = GetString("Email"),
                            PhoneNumber = GetString("Phone"),
                            Password = GetString("Password"),
                            IsActive = true,
                            DistanceToDelivery = GetDouble("Max Distance"),
                            DeliveryType = GetEnum<BO.ShippingType>("Vehicle Type (0-Car, 1-Moto...)"),
                            EmploymentStartDate = DateTime.Now
                        };
                        s_bl.Courier.AddCourier(reqId, newCourier);
                        Console.WriteLine("Added successfully.");
                        break;

                    case 2: // Show
                        Console.WriteLine(s_bl.Courier.SearchCourier(reqId, GetInt("Courier ID")));
                        break;

                    case 3: // List
                        foreach (var item in s_bl.Courier.ListOfCourier(reqId, null, null))
                            Console.WriteLine(item);
                        break;

                    case 4: // Update
                        int updateId = GetInt("ID to Update");
                        var c = s_bl.Courier.SearchCourier(reqId, updateId);
                        Console.WriteLine($"Editing: {c.FullName}");
                        c.FullName = GetString("New Name (Enter to keep):");
                        string newPhone = GetString("New Phone (Enter to keep):");
                        if (newPhone != "") c.PhoneNumber = newPhone;

                        s_bl.Courier.UpdateCourier(reqId, c);
                        Console.WriteLine("Updated.");
                        break;

                    case 5: // Delete
                        s_bl.Courier.DeleteCourier(reqId, GetInt("ID to Delete"));
                        Console.WriteLine("Deleted.");
                        break;
                }
            }
            catch (Exception ex) { PrintException(ex); }
        }
    }
    #endregion

    #region Order Menu
    private static void OrderMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Order Menu ---");
            Console.WriteLine("1. Add Order");
            Console.WriteLine("2. Cancel Order");
            Console.WriteLine("3. Select Order (Courier)");
            Console.WriteLine("4. Close Order (Delivered)");
            Console.WriteLine("5. Show Order");
            Console.WriteLine("6. List Orders (Filter/Sort)");
            Console.WriteLine("7. Statistics");
            Console.WriteLine("8. Open Orders for Courier");
            Console.WriteLine("9. Courier History");
            Console.WriteLine("0. Back");

            int choice = GetInt("Your choice");
            if (choice == 0) return;

            try
            {
                int reqId = GetInt("Requester ID");

                switch (choice)
                {
                    case 1: // Add
                        var newOrder = new BO.Order
                        {
                            OrderingName = GetString("Customer Name"),
                            PhoneNumber = GetString("Phone"),
                            FullAddress = GetString("Address"),
                            Latitude = GetDouble("Latitude"),
                            Longitude = GetDouble("Longitude"),
                            Description = GetString("Description"),
                            OrderType = GetEnum<BO.OrderType>("Type (0-Standard, 1-Express)")
                        };
                        s_bl.Order.AddOrder(reqId, newOrder);
                        Console.WriteLine("Added.");
                        break;

                    case 2: // Cancel
                        s_bl.Order.CancelOrder(reqId, GetInt("Order ID"));
                        Console.WriteLine("Cancelled.");
                        break;

                    case 3: // Select
                        // Assuming OrderSelection is now in IOrder interface as agreed
                        s_bl.Order.OrderSelection(reqId, reqId, GetInt("Order ID"));
                        Console.WriteLine("Selected.");
                        break;

                    case 4: // Close
                        s_bl.Order.CloseOrder(reqId, reqId, GetInt("Delivery ID"));
                        Console.WriteLine("Closed.");
                        break;

                    case 5: // Show
                        Console.WriteLine(s_bl.Order.OrderDetails(reqId, GetInt("Order ID")));
                        break;

                    case 6: // List with Filter
                        BO.OrderInListEnum? filterBy = null;
                        object? filterVal = null;

                        if (GetString("Filter by Status? (y/n)") == "y")
                        {
                            filterBy = BO.OrderInListEnum.OrderStatus;
                            // Using ShipmentCompletionStatus logic
                            filterVal = GetEnum<BO.ShipmentCompletionStatus>("Enter Status");
                        }

                        var list = s_bl.Order.ListOfOrder(reqId, filterBy, filterVal, null);
                        foreach (var item in list) Console.WriteLine(item);
                        break;

                    case 7: // Statistics
                        var stats = s_bl.Order.SumAmountOfOrders(reqId);
                        // Using ShipmentCompletionStatus to display keys
                        foreach (BO.ShipmentCompletionStatus status in Enum.GetValues(typeof(BO.ShipmentCompletionStatus)))
                        {
                            // Safe printing, assuming stats array logic matches enum or similar
                            Console.WriteLine($"Status {status}");
                        }
                        // Printing raw values from array
                        Console.WriteLine("Counts: " + string.Join(", ", stats));
                        break;

                    case 8: // Open Orders for Courier
                        // Assuming GetOpenOrdersForCourier is in IOrder interface
                        var openOrders = s_bl.Order.GetOpenOrdersForCourier(reqId, reqId, null, null);
                        foreach (var item in openOrders) Console.WriteLine(item);
                        break;

                    case 9: // Courier History
                        // Assuming CloseOrderByCourier is in IOrder interface
                        var history = s_bl.Order.CloseOrderByCourier(reqId, reqId, null, null);
                        foreach (var item in history) Console.WriteLine(item);
                        break;
                }
            }
            catch (Exception ex) { PrintException(ex); }
        }
    }
    #endregion

    #region Config Menu
    private static void ConfigMenu()
    {
        while (true)
        {
            Console.WriteLine("\n--- Configuration ---");
            Console.WriteLine("1. Get Clock");
            Console.WriteLine("2. Forward Clock");
            Console.WriteLine("3. Show Config");
            Console.WriteLine("4. Update Config");
            Console.WriteLine("5. Reset DB");
            Console.WriteLine("6. Init DB");
            Console.WriteLine("0. Back");

            int choice = GetInt("Your choice");
            if (choice == 0) return;

            try
            {
                switch (choice)
                {
                    case 1:
                        Console.WriteLine($"Clock: {s_bl.Admin.GetClock()}");
                        break;
                    case 2:
                        var unit = GetEnum<BO.TimeUnit>("Unit (0=Sec, 1=Min, 2=Hour...)");
                        s_bl.Admin.ForwardClock(unit);
                        Console.WriteLine($"New Time: {s_bl.Admin.GetClock()}");
                        break;
                    case 3:
                        // This prints all properties using the helper
                        Console.WriteLine(s_bl.Admin.GetConfig().ToString());
                        break;
                    case 4:
                        var conf = s_bl.Admin.GetConfig();
                        Console.WriteLine($"Current Range: {conf.MaxRange}");
                        conf.MaxRange = GetDouble("New Max Range");
                        s_bl.Admin.SetConfig(conf);
                        Console.WriteLine("Updated.");
                        break;
                    case 5:
                        if (GetString("Confirm Reset? (y/n)") == "y")
                        {
                            s_bl.Admin.ResetDB();
                            Console.WriteLine("Database Reset.");

                            // FIX: Auto-Init to prevent Date crash due to empty clock
                            Console.WriteLine("Restoring valid state (InitDB)...");
                            s_bl.Admin.InitializeDB();
                        }
                        break;
                    case 6:
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine("Init Done.");
                        break;
                }
            }
            catch (Exception ex) { PrintException(ex); }
        }
    }
    #endregion

    #region Helpers
    private static void PrintException(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{ex.Message}");
        if (ex.InnerException != null) Console.WriteLine($"INNER: {ex.InnerException.Message}");
        Console.ResetColor();
    }

    private static int GetInt(string prompt)
    {
        int res;
        do { Console.Write($"{prompt}: "); } while (!int.TryParse(Console.ReadLine(), out res));
        return res;
    }

    private static double GetDouble(string prompt)
    {
        double res;
        do { Console.Write($"{prompt}: "); } while (!double.TryParse(Console.ReadLine(), out res));
        return res;
    }

    private static string GetString(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine() ?? "";
    }

    private static T GetEnum<T>(string prompt) where T : struct
    {
        T res;
        do { Console.Write($"{prompt}: "); } while (!Enum.TryParse(Console.ReadLine(), out res));
        return res;
    }
    #endregion
}