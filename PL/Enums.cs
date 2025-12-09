using System;
using System.Collections;
using System.Collections.Generic;


namespace PL
{
    public class CourierInListEnumCollection : IEnumerable
    {
        static readonly IEnumerable<BO.CourierInListEnum> s_enums =
    (Enum.GetValues(typeof(BO.CourierInListEnum)) as IEnumerable<BO.CourierInListEnum>)!;

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }

    public class ShippingType : IEnumerable
    {
        static readonly IEnumerable<BO.ShippingType> s_enums =
    (Enum.GetValues(typeof(BO.ShippingType)) as IEnumerable<BO.ShippingType>)!;

        public IEnumerator GetEnumerator() => s_enums.GetEnumerator();
    }
}
