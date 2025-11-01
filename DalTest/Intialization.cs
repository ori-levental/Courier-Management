namespace DalTest;
using DalApi;
using DO;
using System.Runtime.InteropServices;

public static class Intialization
{
    private static ICourier? s_dalCourier; //stage 1
    private static IDelivery? s_dalDelivery; //stage 1
    private static IOrder? s_dalOrder; //stage 1
    private static IConfig? s_dalConfig; //stage 1
    private static readonly Random s_rand = new();

    private static void createCoureirs()
    {
        string[] CourierNames =
       { "Dani Levy", "Eli Amar", "Yair Cohen", "Ariela Levin", "Dina Klein", "Shira Israelof" };
        foreach (var name in CourierNames)
        {
            int id;
            do
                id = s_rand.Next(200000000, 400000000);
            while (s_dalCourier!.Read(id) != null);
            string phone;
            do
                phone = "+1" + s_rand.Next(500000000, 599999999).ToString();
            while (s_dalCourier!.Read(id) != null);

            string password = "password" + s_rand.Next(1000, 9999).ToString();

            string email = name + "@gmail.com";
            bool active = s_rand.Next(0, 2) == 1;
            Enums.ShippingType? shipping = (Enums.ShippingType)s_rand.Next(0, 4);
            double? distanceToDelivery = s_rand.Next(5, 51); //in km

            DateTime? start = new DateTime(s_rand.Next(2015, 2024), s_rand.Next(1, 13), s_rand.Next(1, 29));

            s_dalCourier!.Create(new(id, name, phone, email,password,active, distanceToDelivery, shipping,start));
        }

    }
    private static void createDeliveries()
    {
        Courier[] couriers = s_dalCourier!.ReadAll().ToArray();

        for (int i = 0; i < 5; i++)
        {
            {
                int id;
                int did = i;
                do
                    id = s_rand.Next(200000000, 400000000);
                while (s_dalDelivery!.Read(id) != null);
                int cid = couriers[s_rand.Next(0, couriers.Length)].Id;
                Enums.ShippingType deliveryType = (Enums.ShippingType)s_rand.Next(0, 4);
                DateTime startOrderTime = new DateTime(s_rand.Next(2015, 2024), s_rand.Next(1, 13), s_rand.Next(1, 29));
                double? distance = s_rand.Next(5, 51); //in km
                Enums.ShipmentCompletionStatus? endType = (Enums.ShipmentCompletionStatus?)s_rand.Next(0, 5);
                DateTime? endOrderTime = null;
                if (endType != null)
                    endOrderTime = startOrderTime.AddHours(s_rand.Next(1, 6));



                s_dalDelivery!.Create(new(id, did, cid, deliveryType, startOrderTime, distance, endType, endOrderTime));
            }
        }

    }
    private static void createOrders()
    {
        string[] names =
        {
            "George Smith", "Anna Johnson", "Michael Brown", "Emily Davis"
            ,"Olivia Wilson","Britney Spears", "John Doe", "Jane Roe"
        };

        for (int i = 0; i < 8; i++)
        {
            {
                int id;
                do
                    id = s_rand.Next(200000000, 400000000);
                while (s_dalDelivery!.Read(id) != null);
                Enums.OrderType Type = (Enums.OrderType)s_rand.Next(0, 3);
                string? description = "I want: " + i + "apple pies";
                string addres = "D.Trump Blvd " + s_rand.Next(1, 100) + ", New-York, America";
                double latitude = s_rand.Next(-90, 91) + s_rand.NextDouble();
                double longitude = s_rand.Next(-180, 181) + s_rand.NextDouble();
                String orderingName = names[i];
                String phoneNumber = "+1" + s_rand.Next(500000000, 599999999).ToString();
                DateTime startOrderTime = new DateTime(s_rand.Next(2015, 2024), s_rand.Next(1, 13), s_rand.Next(1, 29));

                s_dalOrder!.Create(new(id,Type,description,addres,latitude,longitude,orderingName,phoneNumber,startOrderTime));
            }
        }

    }
    public static void Do(ICourier? dalCourier, IDelivery? dalDelivery,IOrder? dalOrder, IConfig? dalConfig)
    {
        s_dalCourier = dalCourier ?? throw new NullReferenceException("DAL can not be null!");
        s_dalDelivery = dalDelivery ?? throw new NullReferenceException("DAL can not be null!");
        s_dalOrder = dalOrder ?? throw new NullReferenceException("DAL can not be null!");
        s_dalConfig = dalConfig ?? throw new NullReferenceException("DAL can not be null!");
        Console.WriteLine("Reset Configuration values and List values...");
        s_dalConfig.Reset(); //stage 1
        s_dalCourier.DeleteAll(); //stage 1
        createCoureirs();
        s_dalDelivery.DeleteAll();
        createDeliveries();
        s_dalOrder.DeleteAll();
        createOrders();
    }

}
