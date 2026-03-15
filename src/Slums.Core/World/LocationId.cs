namespace Slums.Core.World;

public readonly record struct LocationId(string Value)
{
    public static readonly LocationId Home = new("home");
    public static readonly LocationId Market = new("market");
    public static readonly LocationId Bakery = new("bakery");
    public static readonly LocationId CallCenter = new("call_center");
    public static readonly LocationId Square = new("square");
    public static readonly LocationId Clinic = new("clinic");
    public static readonly LocationId Workshop = new("workshop");
    public static readonly LocationId Cafe = new("cafe");
    public static readonly LocationId Pharmacy = new("pharmacy");
    public static readonly LocationId Depot = new("depot");
    public static readonly LocationId Laundry = new("laundry");
    public static readonly LocationId FishMarket = new("fish_market");
    public static readonly LocationId PlantShop = new("plant_shop");
}
