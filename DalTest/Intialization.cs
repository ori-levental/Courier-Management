namespace DalTest;
using DalApi;
using DO;


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
                phone = "+972" + s_rand.Next(500000000, 599999999).ToString();
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
}
