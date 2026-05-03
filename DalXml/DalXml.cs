using DalApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dal
{
    internal sealed class DalXml : IDal
    {
        //'Lazy<IDal>': This is the wrapper that manages Lazy Initialization and Thread Safety.
        //'() => new DalXml()': This is the factory delegate. It's not executed yet. 
        //    It's stored to be run later, only when the value is requested.
        private static readonly Lazy<IDal> lazyInstance =
            new Lazy<IDal>(() => new DalXml());
        public static IDal Instance
        {
            get
            {
                // This is where Thread Safety happens!
                // On the first run: The Lazy object checks if created -> if not, it Locks -> creates instance -> unlocks.
                // On subsequent runs: It simply returns the already created instance.
                return lazyInstance.Value;
            }
        }
        private DalXml() { }
        public ICourier Courier { get; } = new CourierImplementation();

        public IOrder Order { get; } = new OrderImplementation();

        public IDelivery Delivery { get; } = new DeliveryImplementation();

        public IConfig Config { get; } = new ConfigImplementation();

        public void ResetDB()
        {
            Courier.DeleteAll();
            Order.DeleteAll();
            Delivery.DeleteAll();
            Config.Reset();
        }
    }
}
