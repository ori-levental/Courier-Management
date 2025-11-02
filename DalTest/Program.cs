using Dal;
using DalApi;
using DO;
using System.Reflection.Metadata;

namespace DalTest
{
    internal class Program
    {
        private static ICourier? s_dalCourier = new CourierImplementation(); //stage 1
        private static IDelivery? s_dalDelivery = new DeliveryImplementation(); //stage 1
        private static IOrder? s_dalOrder = new OrderImplementation(); //stage 1
        private static IConfig? s_dalConfig = new ConfigImplementation(); //stage 1
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

        static void Main(string[] args)
        {

            try
            {
                MainMenu choice;
                do
                {
                    Console.WriteLine("Main mune, Press: \n1 to choose Courier \n2 to Delivery \n3 to Order \n4 to init \n" +
                                        "5 to print all \n6 to Config \n7 to reset \n0 to exit");
                    choice = (MainMenu)GetInt("your choice (0-7)");
                    if (choice == 0) break;
                    switch (choice)
                    {
                        case MainMenu.Courier: CourierMenu(); break; // Courier (1)
                        case MainMenu.Delivery: DeliveryMenu(); break; // Delivery (2)
                        case MainMenu.Order: OrderMenu(); break; // Order (3)
                        case MainMenu.Init: Init(); break; // Init (4)
                        case MainMenu.PrintAll: PrintAll(); break; // PrintAll (5)
                        case MainMenu.Config: ConfigMenu(); break; // Config (6)
                        case MainMenu.Reset: Reset(); break; // Reset (7)
                    }

                } while (choice != MainMenu.Exit);
            }
            catch (Exception exp)
            { Console.WriteLine($"ERROR: {exp}"); }
        }

        internal static void CourierMenu()
        {
            CrudMenu choice;
            do
            {
                Console.WriteLine("Courier menu, press: \n0 - back to main menu \n1 - Create \n2 - Read\n3 - Read All\n" +
                    "4 - Update \n5 - Delete \n6 - Delete All");

                choice = (CrudMenu)(MainMenu)GetInt("your choice (0-6)");
                if (choice == CrudMenu.Back) break;
                switch (choice) 
                {
                    case CrudMenu.Add:
                        AddCourier();
                        break;
                    case CrudMenu.Show:
                        ShowCourier(s_dalCourier);
                        break;
                    case CrudMenu.ShowAll:
                        ShowAllCourier();
                        break;  
                    case CrudMenu.Update:
                        UpdateCourier();
                        break;  
                    case CrudMenu.Delete:
                        DeleteCourier();
                        break;
                    case CrudMenu.DeleteAll:
                        DeleteAllCourier();
                        break;  
                }
            }while(true);
        }
        internal static void OrderMenu()
        {
            CrudMenu choice;
            do
            {
                Console.WriteLine("Order menu, press: \n0 - back to main menu \n1 - Create \n2 - Read\n3 - Read All\n" +
                    "4 - Update \n5 - Delete \n6 - Delete All");

                choice = (CrudMenu)(MainMenu)GetInt("your choice (0-6)");
                if (choice == CrudMenu.Back) break;
                switch (choice)
                {
                    case CrudMenu.Add:
                        AddOrder();
                        break;
                    case CrudMenu.Show:
                        ShowOrder(s_dalCourier);
                        break;
                    case CrudMenu.ShowAll:
                        ShowAllOrder();
                        break;
                    case CrudMenu.Update:
                        UpdateOrder();
                        break;
                    case CrudMenu.Delete:
                        DeleteOrder();
                        break;
                    case CrudMenu.DeleteAll:
                        DeleteAllOrder();
                        break;
                }
            } while (true);
        }
        internal static void DeliveryMenu()
        {
            CrudMenu choice;
            do
            {
                Console.WriteLine("Delivery menu, press: \n0 - back to main menu \n1 - Create \n2 - Read\n3 - Read All\n" +
                    "4 - Update \n5 - Delete \n6 - Delete All");

                choice = (CrudMenu)(MainMenu)GetInt("your choice (0-6)");
                if (choice == CrudMenu.Back) break;
                switch (choice)
                {
                    case CrudMenu.Add:
                        AddDelivery();
                        break;
                    case CrudMenu.Show:
                        ShowDelivery(s_dalCourier);
                        break;
                    case CrudMenu.ShowAll:
                        ShowAllDelivery();
                        break;
                    case CrudMenu.Update:
                        UpdateDelivery();
                        break;
                    case CrudMenu.Delete:
                        DeleteDelivery();
                        break;
                    case CrudMenu.DeleteAll:
                        DeleteAllDelivery();
                        break;
                }
            } while (true);
        }

        internal static void DeliveryMenu() { }
        internal static void OrderMenu() { }

        internal static void Init() { 
            Intialization.Do(s_dalCourier,s_dalDelivery,s_dalOrder,s_dalConfig);}
        internal static void PrintAll() { }
        internal static void ConfigMenu() { }
        internal static void Reset() { }

        // courier
        private static void AddCourier()
        {
            int id = GetInt("id");
            string fullName = GetString("full Name");
            string PhoneNumber = GetString("phone number");
            string Email = GetString("email");
            string Password = GetString("password");
            bool Active = GetBoolean("active");
            double? DistanceToDelivery = GetDouble("distance to delivery");
            Enums.ShippingType DeliveryType = GetShippingType("delivery Type");
            DateTime EmploymentStartDate = GetDateTime("employment start date");
            
            Courier newCourier = new Courier(id, fullName, PhoneNumber, Email, Password, Active, DistanceToDelivery, DeliveryType, EmploymentStartDate);
            s_dalCourier.Create(newCourier);
        }
        private static void ShowCourier(ICourier? s_dalCourier)
        {
            int id = GetInt("id");
            Console.WriteLine(s_dalCourier.Read(id));
        }
        private static void ShowAllCourier()
        {
            var couriers = s_dalCourier?.ReadAll();
            if (couriers != null && couriers.Count > 0)
            {
                foreach (var courier in couriers)
                    Console.WriteLine(courier);
            }
            else
            {
                Console.WriteLine("Couriers is empty");
            }
        }
        internal static void UpdateCourier()
        {
            int id = GetInt("Id to update");
            var existgCall = s_dalCourier?.Read(id);
            if (existgCall != null)
            {

                string fullName = GetString("full Name");
                string PhoneNumber = GetString("phone number");
                string Email = GetString("email");
                string Password = GetString("password");
                bool Active = GetBoolean("active");
                double? DistanceToDelivery = GetDouble("distance to delivery");
                Enums.ShippingType DeliveryType = GetShippingType("delivery Type");
                DateTime EmploymentStartDate = GetDateTime("employment start date");

                Courier UpdateCourier = new Courier(id, fullName, PhoneNumber, Email, Password, Active, DistanceToDelivery, DeliveryType, EmploymentStartDate);
                s_dalCourier.Update(UpdateCourier);
            }
        }
        internal static void DeleteCourier()
        {
            int id = GetInt("Id of the courier for deletion:");
            s_dalCourier.Delete(id);
        }
        internal static void DeleteAllCourier()
        {
            s_dalCourier.DeleteAll();
            Console.WriteLine("Deleted all\n");
        }


        // geters
        private static int GetInt(string prompt)
        {
            Console.Write($"Enter  {prompt}: ");
            return int.Parse(s: Console.ReadLine());
        }
        private static double GetDouble(string prompt)
        {
            Console.Write($"Enter {prompt}: ");
            return double.Parse(s: Console.ReadLine());
        }
        private static string GetString(string prompt)
        {
            Console.Write($"Enter {prompt}: ");
            return Console.ReadLine();
        }
        private static bool GetBoolean(string prompt)
        {
            Console.Write($"{prompt} (true/false): ");
            return bool.Parse(value: Console.ReadLine());
        }

        // helped by 'gemini' prmpt like the other methods
        private static Enums.ShippingType GetShippingType(string prompt)
        {
            Console.Write($"Enter  {prompt}: ");
            string input = Console.ReadLine();
            return (Enums.ShippingType)Enum.Parse(typeof(Enums.ShippingType), input, true);
        }
        private static DateTime GetDateTime(string prompt)
        {
            Console.Write($"Enter  {prompt}: ");
            return DateTime.Parse(s: Console.ReadLine());
        }

    }
}

