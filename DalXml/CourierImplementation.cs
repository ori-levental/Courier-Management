namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices; // Added for MethodImpl
using System.Xml.Linq;

/// <summary>
/// XML (XElement) implementation of the ICourier interface.
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// Helper: Converts an XElement to a Courier entity.
    /// </summary>
    static Courier getCourier(XElement s)
    {
        return new DO.Courier()
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("can't convert id"),
            FullName = (string?)s.Element("FullName") ?? "",
            PhoneNumber = (string?)s.Element("PhoneNumber") ?? "",
            Email = (string?)s.Element("Email") ?? "",
            Password = (string?)s.Element("password") ?? "",
            Active = bool.TryParse((string?)s.Element("Active"), out var activeResult) ? activeResult : false,
            DistanceToDelivery = s.ToDoubleNullable("DistanceToDelivery"),
            DeliveryType = s.ToEnumNullable<Enums.ShippingType>("DeliveryType"),
            EmploymentStartDate = s.ToDateTimeNullable("EmploymentStartDate"),
        };
    }

    /// <summary>
    /// Helper: Converts a Courier entity to an XElement.
    /// </summary>
    static XElement ToXElement(Courier courier)
    {
        return new XElement("Courier",
            new XElement("Id", courier.Id),
            new XElement("FullName", courier.FullName),
            new XElement("PhoneNumber", courier.PhoneNumber),
            new XElement("Email", courier.Email),
            new XElement("password", courier.Password),
            new XElement("Active", courier.Active.ToString()),
            courier.DistanceToDelivery.HasValue ? new XElement("DistanceToDelivery", courier.DistanceToDelivery.Value) : null,
            courier.DeliveryType.HasValue ? new XElement("DeliveryType", courier.DeliveryType.Value) : null,
            courier.EmploymentStartDate.HasValue ? new XElement("EmploymentStartDate", courier.EmploymentStartDate.Value) : null
        );
    }

    /// <summary>
    /// Adds a new Courier to the XML file. Throws if ID exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
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

    /// <summary>
    /// Deletes a Courier by ID. Throws if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
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

    /// <summary>
    /// Removes all Couriers from the XML file.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // load the list
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        root.RemoveAll();
        // save
        XMLTools.SaveListToXMLElement(root, Config.s_couriers_xml);
    }

    /// <summary>
    /// Retrieves a Courier by ID, or null if not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        XElement? courierElem =
    XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return courierElem is null ? null : getCourier(courierElem);
    }

    /// <summary>
    /// Retrieves the first Courier matching the filter condition.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements().Select(c => getCourier(c)).FirstOrDefault(filter);
    }

    /// <summary>
    /// Retrieves all Couriers, optionally filtered by a predicate.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        // load the list
        XElement root = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        var couriers = root.Elements("Courier").Select(c => getCourier(c));
        return filter == null ? couriers : couriers.Where(filter);
    }

    /// <summary>
    /// Updates an existing Courier. Throws if ID not found.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
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