namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

internal class CourierImplementation : ICourier
{
    static Courier getCourier(XElement s)
    {
        return new DO.Courier()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            FullName = (string?)s.Element("FullName") ?? "",
            PhoneNumber = (string?)s.Element("PhoneNumber") ?? "",
            Email = (string?)s.Element("Email") ?? "",
            password = (string?)s.Element("password") ?? "",
            Active = bool.TryParse((string?)s.Element("Active"), out var activeResult) ? activeResult : false,
            DistanceToDelivery = s.ToDoubleNullable("DistanceToDelivery"),
            DeliveryType = s.ToEnumNullable<Enums.ShippingType>("DeliveryType"),
            EmploymentStartDate = s.ToDateTimeNullable("EmploymentStartDate"),
        };
    }
    static XElement ToXElement(Courier courier)
    {
        return new XElement("Courier",
            new XElement("Id", courier.Id),
            new XElement("FullName", courier.FullName),
            new XElement("PhoneNumber", courier.PhoneNumber),
            new XElement("Email", courier.Email),
            new XElement("password", courier.password),
            new XElement("Active", courier.Active.ToString()),
            courier.DistanceToDelivery.HasValue ? new XElement("DistanceToDelivery", courier.DistanceToDelivery.Value) : null,
            courier.DeliveryType.HasValue ? new XElement("DeliveryType", courier.DeliveryType.Value) : null,
            courier.EmploymentStartDate.HasValue ? new XElement("EmploymentStartDate", courier.EmploymentStartDate.Value) : null
        );
    }

    public void Create(Courier item)
    {
        // load the list
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        if (root.Elements("Courier").Any(c => c.ToIntNullable("Id") == item.Id))
        {
            throw new DalAlreadyExistsException($"Courier with ID {item.Id} already exists.");
        }
        else
        {
            // Change the courier to XElement and push beack - and save.
            XElement newCourierElement = ToXElement(item);
            root.Add(newCourierElement);
            XMLTools.SaveListToXMLElement(root, Config.s_couriers_xml);
        }
    }

    public void Delete(int id)
    {
        // load the list
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        XElement? elementToDelete = root.Elements("Courier")
            .FirstOrDefault(c => c.ToIntNullable("Id") == id);

        if (elementToDelete == null)
        {
            throw new DalDoesNotExistException($"Courier with ID {id}  does Not exists.");
        }
        else
        {
            // delete and save
            elementToDelete.Remove();
            XMLTools.SaveListToXMLElement(root, Config.s_couriers_xml);
        }
    }

    public void DeleteAll()
    {
        // load the list
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        root.RemoveAll();
        // save
        XMLTools.SaveListToXMLElement(root, Config.s_couriers_xml);
    }

    public Courier? Read(int id)
    {
        XElement? courierElem =
    XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return courierElem is null ? null : getCourier(courierElem);
    }


    public Courier? Read(Func<Courier, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().Select(c => getCourier(c)).FirstOrDefault(filter);
    }


    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        // load the list
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        var couriers = root.Elements("Courier").Select(c=>getCourier(c));
        return filter == null ? couriers : couriers.Where(filter);
    }

    public void Update(Courier item)
    {
        // load the courier XML
        XElement couriersRootElem = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        XElement elementToRemove = couriersRootElem.Elements("Courier")
            .FirstOrDefault(c => (int?)c.Element("Id") == item.Id)
            ?? throw new DO.DalDoesNotExistException($"Courier with ID={item.Id} does Not exist");

        // update the courier
        elementToRemove.Remove();
        XElement newElement = ToXElement(item);
        couriersRootElem.Add(newElement);

        // save the list
        XMLTools.SaveListToXMLElement(couriersRootElem, Config.s_couriers_xml);
    }
}
