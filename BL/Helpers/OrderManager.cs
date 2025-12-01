using BO;
using DalApi;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get; //stage 4
    private static DO.Order BOToDOOrder(BO.Order BoOrder)
    {
        DO.Order doCourier = new DO.Order()
        {
            Id = BoOrder.Id,
            Description = BoOrder.Description,
            CustomerAddress = BoOrder.FullAddress,
            PhoneNumber = BoOrder.PhoneNumber,
            Latitude = BoOrder.Latitude,
            OrderingName = BoOrder.OrderingName,
            Type = (DO.Enums.OrderType)BoOrder.OrderType!,
            Longitude = BoOrder.Longitude,
            StartOrderTime = BoOrder.StartOrderTime,
        };
        return doCourier;
    }
    internal static void AddOrder(int requesterId, BO.Order boOrder)
    {
        DO.Order doOrder = BOToDOOrder(boOrder);
        try
        {
            s_dal.Order.Create(doOrder);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Order with ID {doOrder.Id} already exists", ex);
        }
    }
    internal static void AccessPermissionToManager(int requesterId)
    {
        if (requesterId != DalApi.Factory.Get.Config.ManagerId)
            throw new BO.BLAccessPermission("ERROR: No access permission");
    }

    internal static void CheckCorrectnessVariables(BO.Order boOrder)
    {
        // Execute all individual property validations
        CheckPhoneNumber(boOrder.PhoneNumber);
        CheckAdress(boOrder.FullAddress);
        CheckLatitude(boOrder.Latitude);
        CheckLongtitude(boOrder.Longitude);
        CheckOrderingName(boOrder.OrderingName);

    }

    internal static void CheckOrderingName(string orderingName)
    {
       if(string.IsNullOrWhiteSpace(orderingName))
            throw new BO.BLInvalidDataException("ERROR: ordering name cannot be empty");
    }

    internal static void CheckAdress(string fullAddress)
    {
       if(string.IsNullOrWhiteSpace(fullAddress))
            throw new BO.BLInvalidDataException("ERROR: address cannot be empty");
    }

    internal static void CheckLongtitude(double longitude)
    {
        if (longitude < -180 || longitude > 180)
            throw new BO.BLInvalidDataException("ERROR: lontitide must be between -180 to 180 degrees");
    }

    internal static void CheckLatitude(double latitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new BO.BLInvalidDataException("ERROR: latitude must be between -90 to 90 degrees");
    }

    internal static void CheckPhoneNumber(string phoneNumber)
    {
        // Validate format: 10 digits, starts with '05', contains only numbers
        if (string.IsNullOrWhiteSpace(phoneNumber) ||
            phoneNumber.Length != 10 ||
            phoneNumber[0] != '0' ||
            phoneNumber[1] != '5' ||
            !phoneNumber.All(char.IsDigit))
        {
            throw new BO.BLInvalidDataException("ERROR: Invalid phone number. Must start with '05' and contain 10 digits.");
        }
    }


}
