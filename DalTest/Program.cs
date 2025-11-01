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
        private static IConfig? s_dalConfig = new ConfigImplementation(); //stage
        public enum Menu
        {
            EXIT,
            COURIER,
            DELIVERY,
            ORDER,
            INIT,
            PRINT_ALL,
            CONFIG,
            RESET
        }
        internal static void CourierMenu()
        {
            int choice;
            do
            {
                choice = Console.Read();
                Console.WriteLine("press:\n 0 - back to main menu, 1 - Create, 2 - Read, 3 - Read All, " +
                    "4 - Update, 5 - Delete, 6 - Delete All");
                if (choice == 0) break;
                switch (choice) 
                {
                    case 1: Console.WriteLine("Enter Details: "); break;

                }
            }while(true);
        }
        internal static void DeliveryMenu() { }
        internal static void OrderMenu() { }

        internal static void Init() { 
            Intialization.Do(s_dalCourier,s_dalDelivery,s_dalOrder,s_dalConfig);}
        internal static void PrintAll() { }
        internal static void ConfigMenu() { }
        internal static void Reset() { }
        static void Main(string[] args)
        {
       
            try{
                int choice;
                do
                {
                    Console.WriteLine("Press 1 to choose Courier, 2 to Delivery, 3 to Order," +
                        "4 to init, 5 to print all, 6 to Config, 7 to reset, 0 to exit");
                    choice = Console.Read();
                    if (choice == 0) break;
                    switch (choice)
                    {
                        case 1: CourierMenu(); break;
                        case 2: DeliveryMenu(); break;
                        case 3: OrderMenu(); break;
                        case 4: Init();break;
                        case 5: PrintAll(); break;
                        case 6: ConfigMenu(); break;
                        case 7: Reset(); break;
                    }

                } while (true);
            
            
            
            
            
            
            
            
            
            
            }
            catch {}
        }
    }
}
