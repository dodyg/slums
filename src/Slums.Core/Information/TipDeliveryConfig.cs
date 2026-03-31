using Slums.Core.World;

namespace Slums.Core.Information;

public static class TipDeliveryConfig
{
    public static TipDeliveryMethod GetDeliveryMethod(Tip tip, DistrictId currentDistrict)
    {
        ArgumentNullException.ThrowIfNull(tip);

        if (tip.IsEmergency)
        {
            return TipDeliveryMethod.Emergency;
        }

        if (tip.RelevantDistrict == currentDistrict)
        {
            return TipDeliveryMethod.InPerson;
        }

        return TipDeliveryMethod.Phone;
    }
}

public enum TipDeliveryMethod
{
    InPerson,
    Phone,
    Emergency
}
