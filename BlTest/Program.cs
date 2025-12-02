using BlApi;
using BO;
using System.Globalization;

namespace BlTest;

internal class Program
{
    // Initialize the Business Logic layer interface via the Factory (Singleton access)
    static readonly IBl s_bl = Factory.Get();

    // Menu Definitions
    private enum MainMenu { Exit, Courier, Order, Config }
    private enum CrudMenu { Back, Add, Show, ShowAll, Update, Delete }
    private enum OrderMenuOptions { Back, Add, Cancel, SelectOrder, CloseOrder, Show, ShowAll }
    private enum ConfigMenuOptions { Back, GetClock, ForwardClock, ShowConfig, UpdateConfig, ResetDB, InitDB }

    static void Main(string[] args)
    {
        // Set culture to ensure consistent number/date formatting (e.g., dot for decimals)
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

        Console.WriteLine("=== BL Test Program ===");

        try
        {
            MainMenu choice;
            do
            {
                Console.WriteLine("\n--- Main Menu ---");
                Console.WriteLine(
                    "1. Courier Management\n" +
                    "2. Order Management\n" +
                    "3. Admin / Configuration\n" +
                    "0. Exit"
                );

                // Safe input parsing using helper method
                choice = (MainMenu)GetInt("\nYour choice (0-3)");

                switch (choice)
                {
                    case MainMenu.Courier: CourierMenu(); break;
                    case MainMenu.Order: OrderMenu(); break;
                    case MainMenu.Config: ConfigMenu(); break;
                    case MainMenu.Exit: Console.WriteLine("Exiting..."); break;
                    default: Console.WriteLine("Invalid choice."); break;
                }

            } while (choice != MainMenu.Exit);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n*** CRITICAL ERROR: {ex.Message} ***\n");
        }
    }

    #region Courier Menu
    private static void CourierMenu()
    {
        CrudMenu choice;
        do
        {
            Console.WriteLine("\n--- Courier Menu ---");
            Console.WriteLine(
                "1. Add Courier\n" +
                "2. Show Courier Details\n" +
                "3. List All Couriers\n" +
                "4. Update Courier\n" +
                "5. Delete Courier\n" +
                "0. Back"
            );

            choice = (CrudMenu)GetInt("Your choice");
            if (choice == CrudMenu.Back) break;

            try
            {
                // Simulate a logged-in Manager ID
                int reqId = GetInt("Enter Manager ID (Requester)");

                switch (choice)
                {
                    case CrudMenu.Add:
                        BO.Courier newCourier = new BO.Courier
                        {
                            Id = GetInt("ID"),
                            FullName = GetString("Name"),
                            Email = GetString("Email"),
                            PhoneNumber = GetString("Phone"),
                            Password = GetString("Password"),
                            IsActive = true,
                            DistanceToDelivery = GetDouble("Max Distance"),
                            DeliveryType = GetEnum<BO.ShippingType>("Vehicle Type (0=Walk, 1=Bicycle, 2=Motorcycle, 3=Car)"),
                            EmploymentStartDate = DateTime.Now
                        };
                        s_bl.Courier.AddCourier(reqId, newCourier);
                        Console.WriteLine("Courier added successfully.");
                        break;

                    case CrudMenu.Show:
                        int idShow = GetInt("Courier ID");
                        // Implicitly calls ToString() via Console.WriteLine
                        Console.WriteLine(s_bl.Courier.SearchCourier(reqId, idShow));
                        break;

                    case CrudMenu.ShowAll:
                        // List all couriers (optional filters set to null)
                        var list = s_bl.Courier.ListOfCourier(reqId, null, null);
                        foreach (var item in list) Console.WriteLine(item);
                        break;

                    case CrudMenu.Update:
                        int idUpdate = GetInt("Courier ID to Update");
                        // Retrieve existing entity first to allow partial updates
                        BO.Courier existing = s_bl.Courier.SearchCourier(reqId, idUpdate);
                        Console.WriteLine($"Updating: {existing.FullName}");

                        // Update specific fields (Expandable logic)
                        existing.FullName = GetString("New Name (or re-enter old):");
                        existing.PhoneNumber = GetString("New Phone:");

                        s_bl.Courier.UpdateCourier(reqId, existing);
                        Console.WriteLine("Courier updated.");
                        break;

                    case CrudMenu.Delete:
                        int idDel = GetInt("Courier ID to Delete");
                        s_bl.Courier.DeleteCourier(reqId, idDel);
                        Console.WriteLine("Courier deleted.");
                        break;
                }
            }
            // Exception handling specific to BO layer
            catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
            catch (BO.BlInvalidDataException ex) { PrintException(ex); }
            catch (BO.BlAlreadyExistsException ex) { PrintException(ex); }
            catch (BO.BlDeletionImpossibleException ex) { PrintException(ex); }
            catch (Exception ex) { PrintException(ex); }

        } while (true);
    }
    #endregion

    #region Order Menu
    private static void OrderMenu()
    {
        OrderMenuOptions choice;
        do
        {
            Console.WriteLine("\n--- Order Menu ---");
            Console.WriteLine(
                "1. Add Order\n" +
                "2. Cancel Order\n" +
                "3. Select Order (Courier)\n" +
                "4. Close Order (Delivered)\n" +
                "5. Show Order Details\n" +
                "6. List All Orders\n" +
                "0. Back"
            );

            choice = (OrderMenuOptions)GetInt("Your choice");
            if (choice == OrderMenuOptions.Back) break;

            try
            {
                // Requester ID can be a Manager or a Courier depending on the action
                int reqId = GetInt("Enter Requester ID");

                switch (choice)
                {
                    case OrderMenuOptions.Add:
                        BO.Order newOrder = new BO.Order
                        {
                            // ID is generated by the DAL, passed as 0
                            OrderingName = GetString("Customer Name"),
                            PhoneNumber = GetString("Phone"),
                            FullAddress = GetString("Address"),
                            Latitude = GetDouble("Latitude"),
                            Longitude = GetDouble("Longitude"),
                            Description = GetString("Description"),
                            OrderType = GetEnum<BO.OrderType>("Order Type (0=Standard, 1=Express)")
                        };
                        s_bl.Order.AddOrder(reqId, newOrder);
                        Console.WriteLine("Order added.");
                        break;

                    case OrderMenuOptions.Cancel:
                        int cancelId = GetInt("Order ID to Cancel");
                        s_bl.Order.CancelOrder(reqId, cancelId);
                        Console.WriteLine("Order cancelled.");
                        break;

                    case OrderMenuOptions.SelectOrder:
                        int courierId = reqId; // Assuming the requester is the courier
                        int orderToPick = GetInt("Order ID to Select");

                        // Casting to specific implementation to access method if not yet in interface
                        // If it is in IOrder, use s_bl.Order directly
                        ((BlImplementation.OrderImplementation)s_bl.Order).OrderSelection(reqId, courierId, orderToPick);
                        Console.WriteLine("Order selected.");
                        break;

                    case OrderMenuOptions.CloseOrder:
                        int delCourierId = reqId; // Assuming the requester is the courier
                        int deliveryId = GetInt("Delivery ID (from Order details)");
                        s_bl.Order.CloseOrder(reqId, delCourierId, deliveryId);
                        Console.WriteLine("Order closed (Provided).");
                        break;

                    case OrderMenuOptions.Show:
                        int ordId = GetInt("Order ID");
                        Console.WriteLine(s_bl.Order.OrderDetails(reqId, ordId));
                        break;

                    case OrderMenuOptions.ShowAll:
                        var orders = s_bl.Order.ListOfOrder(reqId, null, null);
                        foreach (var item in orders) Console.WriteLine(item);
                        break;
                }
            }
            catch (Exception ex) { PrintException(ex); }

        } while (true);
    }
    #endregion

    #region Admin / Config Menu
    private static void ConfigMenu()
    {
        ConfigMenuOptions choice;
        do
        {
            Console.WriteLine("\n--- Admin Menu ---");
            Console.WriteLine(
                "1. Get System Clock\n" +
                "2. Forward Clock\n" +
                "3. Show Configuration\n" +
                "4. Update Configuration\n" +
                "5. Reset DB\n" +
                "6. Initialize DB\n" +
                "0. Back"
            );

            choice = (ConfigMenuOptions)GetInt("Your choice");
            if (choice == ConfigMenuOptions.Back) break;

            try
            {
                switch (choice)
                {
                    case ConfigMenuOptions.GetClock:
                        Console.WriteLine($"Current Clock: {s_bl.Admin.GetClock()}");
                        break;

                    case ConfigMenuOptions.ForwardClock:
                        var unit = GetEnum<BO.TimeUnit>("Unit (0=Second, 1=Minute, 2=Hour, 3=Day...)");
                        s_bl.Admin.ForwardClock(unit);
                        Console.WriteLine($"New Time: {s_bl.Admin.GetClock()}");
                        break;

                    case ConfigMenuOptions.ShowConfig:
                        BO.Config conf = s_bl.Admin.GetConfig();
                        Console.WriteLine($"Max Range: {conf.MaxRange}");
                        Console.WriteLine($"Address: {conf.CompanyAddress}");
                        Console.WriteLine($"Car Speed: {conf.AvgCarSpeed}");
                        break;

                    case ConfigMenuOptions.UpdateConfig:
                        BO.Config current = s_bl.Admin.GetConfig();
                        Console.WriteLine($"Current Max Range: {current.MaxRange}");
                        current.MaxRange = GetDouble("Enter new Max Range");
                        s_bl.Admin.SetConfig(current);
                        Console.WriteLine("Configuration updated.");
                        break;

                    case ConfigMenuOptions.ResetDB:
                        Console.Write("Are you sure? (y/n): ");
                        if (Console.ReadLine() == "y")
                        {
                            s_bl.Admin.ResetDB();
                            Console.WriteLine("DB Reset.");
                        }
                        break;

                    case ConfigMenuOptions.InitDB:
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine("DB Initialized.");
                        break;
                }
            }
            catch (Exception ex) { PrintException(ex); }

        } while (true);
    }
    #endregion

    #region Helpers (Input & Output)

    // Standardized Exception Printing
    private static void PrintException(Exception ex)
    {
        Console.WriteLine($"\nERROR ({ex.GetType().Name}): {ex.Message}");
        if (ex.InnerException != null)
            Console.WriteLine($"  Inner ({ex.InnerException.GetType().Name}): {ex.InnerException.Message}");
    }

    // --- Safe Input Methods (Using TryParse) ---

    private static int GetInt(string prompt)
    {
        int result;
        do
        {
            Console.Write($"{prompt}: ");
        } while (!int.TryParse(Console.ReadLine(), out result));
        return result;
    }

    private static double GetDouble(string prompt)
    {
        double result;
        do
        {
            Console.Write($"{prompt}: ");
        } while (!double.TryParse(Console.ReadLine(), out result));
        return result;
    }

    private static string GetString(string prompt)
    {
        Console.Write($"{prompt}: ");
        return Console.ReadLine() ?? "";
    }

    // Generic method to parse any Enum safely
    private static T GetEnum<T>(string prompt) where T : struct
    {
        T result;
        do
        {
            Console.Write($"{prompt}: ");
        } while (!Enum.TryParse(Console.ReadLine(), out result));
        return result;
    }

    #endregion
}