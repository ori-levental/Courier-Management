using Dal;
using DalApi;
using DO;
using System.Reflection.Metadata;
using static DO.Enums;

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

                choice = (CrudMenu)GetInt("your choice (0-6)");
                if (choice == CrudMenu.Back) break;
                switch (choice)
                {
                    case CrudMenu.Add:
                        AddCourier();
                        break;
                    case CrudMenu.Show:
                        ShowCourier();
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
            } while (true);
        }
        internal static void OrderMenu()
        {
            CrudMenu choice;
            do
            {
                Console.WriteLine("Order menu, press: \n0 - back to main menu \n1 - Create \n2 - Read\n3 - Read All\n" +
                    "4 - Update \n5 - Delete \n6 - Delete All");

                choice = (CrudMenu)GetInt("your choice (0-6)");
                if (choice == CrudMenu.Back) break;
                switch (choice)
                {
                    case CrudMenu.Add:
                        AddOrder();
                        break;
                    case CrudMenu.Show:
                        ShowOrder();
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

                choice = (CrudMenu)GetInt("your choice (0-6)");
                if (choice == CrudMenu.Back) break;
                switch (choice)
                {
                    case CrudMenu.Add:
                        AddDelivery();
                        break;
                    case CrudMenu.Show:
                        ShowDelivery();
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
            Console.WriteLine("The courier was added\n");
        }
        private static void ShowCourier()
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
                Console.WriteLine("Couriers is empty\n");
            }
        }
        internal static void UpdateCourier()
        {
            int id = GetInt("Id to update");
            var existCourier = s_dalCourier?.Read(id);
            if (existCourier != null)
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
                Console.WriteLine("The courier was update\n");
            }
        }
        internal static void DeleteCourier()
        {
            int id = GetInt("Id of the courier for deletion:");
            s_dalCourier.Delete(id);
            Console.WriteLine("The courier was deketed\n");

        }
        internal static void DeleteAllCourier()
        {
            s_dalCourier.DeleteAll();
            Console.WriteLine("Deleted all\n");
        }

        // Order
        private static void AddOrder()
        {
            int id = GetInt("id");
            Enums.OrderType OrderType = GetOrderType("order type");
            string? Description = GetString("short description");
            string Addres = GetString("Addres");
            double Latitude = GetDouble("latitude");
            double Longitude = GetDouble("longitude");
            string OrderingName = GetString("ordering name");
            string phoneNumber = GetString("phone number");
            DateTime StartOrderTime = GetDateTime("start order time");

            Order newOrder = new Order(id, OrderType, Description, Addres, Latitude, Longitude, OrderingName, phoneNumber, StartOrderTime);
            s_dalOrder.Create(newOrder);
            Console.WriteLine("The order was added\n");
        }
        private static void ShowOrder()
        {
            int id = GetInt("id");
            Console.WriteLine(s_dalOrder.Read(id));
        }
        private static void ShowAllOrder()
        {
            var orders = s_dalOrder?.ReadAll();
            if (orders != null && orders.Count > 0)
            {
                foreach (var order in orders)
                    Console.WriteLine(orders);
            }
            else
            {
                Console.WriteLine("Orders is empty\n");
            }
        }
        private static void UpdateOrder()
        {
            int id = GetInt("Id to update");
            var existOrder = s_dalCourier?.Read(id);
            if (existOrder != null)
            {
                Enums.OrderType OrderType = GetOrderType("order type");
                string? Description = GetString("short description");
                string Addres = GetString("Addres");
                double Latitude = GetDouble("latitude");
                double Longitude = GetDouble("longitude");
                string OrderingName = GetString("ordering name");
                string phoneNumber = GetString("phone number");
                DateTime StartOrderTime = GetDateTime("start order time");

                Order newOrder = new Order(id, OrderType, Description, Addres, Latitude, Longitude, OrderingName, phoneNumber, StartOrderTime);
                s_dalOrder.Create(newOrder);
                Console.WriteLine("The order was update\n");
            }
        }
        private static void DeleteOrder()
        {
            int id = GetInt("Id of the courier for deletion:");
            s_dalOrder.Delete(id);
            Console.WriteLine("The order was deleted\n");

        }
        private static void DeleteAllOrder()
        {
            s_dalOrder.DeleteAll();
            Console.WriteLine("Deleted all\n");
        }

        // Delivery
        private static void AddDelivery()
        {
            int id = GetInt("id");
            int OrderId = GetInt("order id");
            int CourierId = GetInt("courier id");
            Enums.ShippingType DeliveryType = GetShippingType("delivery type");
            DateTime StartOrderTime = GetDateTime("start order time");
            double? Distance = GetDouble("distance");
            Enums.ShipmentCompletionStatus? EndType = GetShipmentCompletionStatus("end type");
            DateTime? EndOrderTime = GetDateTime("end order time");

            Delivery newDelivery = new Delivery(id, OrderId, CourierId, DeliveryType, StartOrderTime, Distance, EndType, EndOrderTime);
            s_dalDelivery.Create(newDelivery);
            Console.WriteLine("The delivery was added\n");
        }
        private static void ShowDelivery()
        {
            int id = GetInt("id");
            Console.WriteLine(s_dalDelivery.Read(id));
        }
        private static void ShowAllDelivery()
        {
            var deliverys = s_dalDelivery?.ReadAll();
            if (deliverys != null && deliverys.Count > 0)
            {
                foreach (var delivery in deliverys)
                    Console.WriteLine(deliverys);
            }
            else
            {
                Console.WriteLine("Delivery is empty\n");
            }
        }
        private static void UpdateDelivery()
        {
            int id = GetInt("Id to update");
            var existDelivery = s_dalDelivery?.Read(id);
            if (existDelivery != null)
            {
                int OrderId = GetInt("order id");
                int CourierId = GetInt("courier id");
                Enums.ShippingType DeliveryType = GetShippingType("delivery type");
                DateTime StartOrderTime = GetDateTime("start order time");
                double? Distance = GetDouble("distance");
                Enums.ShipmentCompletionStatus? EndType = GetShipmentCompletionStatus("end type");
                DateTime? EndOrderTime = GetDateTime("end order time");

                Delivery newDelivery = new Delivery(id, OrderId, CourierId, DeliveryType, StartOrderTime, Distance, EndType, EndOrderTime);
                s_dalDelivery.Create(newDelivery);
                Console.WriteLine("The delivery was update\n");
            }
        }
        private static void DeleteDelivery()
        {
            int id = GetInt("Id of the delivery for deletion:");
            s_dalDelivery.Delete(id);
            Console.WriteLine("The delivery was deleted\n");

        }
        private static void DeleteAllDelivery()
        {
            s_dalDelivery.DeleteAll();
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

        // helped by 'gemini' prmpt like the other methods with enum
        private static Enums.ShippingType GetShippingType(string prompt)
        {
            Console.Write($"Enter  {prompt}: ");
            string input = Console.ReadLine();
            return (Enums.ShippingType)Enum.Parse(typeof(Enums.ShippingType), input, true);
        }
        private static Enums.OrderType GetOrderType(string prompt)
        {
            Console.Write($"Enter  {prompt}: ");
            string input = Console.ReadLine();
            return (Enums.OrderType)Enum.Parse(typeof(Enums.OrderType), input, true);
        }
        private static Enums.ShipmentCompletionStatus GetShipmentCompletionStatus(string prompt)
        {
            Console.Write($"Enter  {prompt}: ");
            string input = Console.ReadLine();
            return (Enums.ShipmentCompletionStatus)Enum.Parse(typeof(Enums.ShipmentCompletionStatus), input, true);
        }

        private static DateTime GetDateTime(string prompt)
        {
            Console.Write($"Enter  {prompt}: ");
            return DateTime.Parse(s: Console.ReadLine());
        }

    }
}

