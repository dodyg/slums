namespace Slums.Core.Characters;

public sealed class Background
{
    public BackgroundType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string StoryIntro { get; init; } = string.Empty;
    public int StartingMoney { get; init; } = 100;
    public int StartingHealth { get; init; } = 100;
    public int StartingEnergy { get; init; } = 80;
    public int StartingHunger { get; init; } = 80;
    public int StartingStress { get; init; } = 20;
    public int MotherStartingHealth { get; init; } = 70;
    public int FoodStockpile { get; init; } = 3;
    public string InkIntroKnot { get; init; } = string.Empty;
}
