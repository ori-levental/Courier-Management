namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

internal class DeliveryImplementation : IDelivery
{
    public void Create(Delivery item)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
       Delivery temp = item with { Id = Config.NextDeliveryId };
        Deliveries.Add(temp);
        XMLTools.SaveListToXMLSerializer(Deliveries, Config.s_deliveries_xml);
    }

    public void Delete(int id)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        if (Deliveries.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Deliveries, Config.s_deliveries_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Delivery>(), Config.s_deliveries_xml);
    }

    public Delivery? Read(int id)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return Deliveries.FirstOrDefault(d => d.Id == id);
    }

    public Delivery? Read(Func<Delivery, bool> filter)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return Deliveries.FirstOrDefault(filter);
    }

    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);

        if (filter == null)
            return Deliveries;

        return Deliveries.Where(filter);
    }

    public void Update(Delivery item)
    {
        List<Delivery> Deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        if (Deliveries.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist");
        Deliveries.Add(item);
        XMLTools.SaveListToXMLSerializer(Deliveries, Config.s_deliveries_xml);
    }
}