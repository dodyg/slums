namespace Slums.Core.World;

public readonly record struct LocationId(string Value)
{
    public static readonly LocationId Home = new("home");
    public static readonly LocationId Market = new("market");
    public static readonly LocationId Bakery = new("bakery");
    public static readonly LocationId CallCenter = new("call_center");
    public static readonly LocationId Square = new("square");
}
