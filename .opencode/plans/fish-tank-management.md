# Fish Tank Management — Implementation Plan

## Problem
After buying a fish tank at the fish market, the player has no way to manage it from the home "Pets & Plants" menu. Plants have a full `ManagePlant` → upgrade screen pipeline, but fish tanks only appear in the bulk `PayPetCare` action. There is no individual management entry, no upgrades, and no upgrade screen.

## Approach
Mirror the plant upgrade pattern exactly: add a `ManageFishTank` action type that opens a `FishTankUpgradeScreen` with 4 fish-tank-specific upgrades (2 permanent + 2 recurring).

---

## Step 1 — Core Domain: `FishTankUpgradeType` enum (NEW FILE)

**File**: `src/Slums.Core/Characters/FishTankUpgradeType.cs`

```csharp
namespace Slums.Core.Characters;

public enum FishTankUpgradeType
{
    BetterFilter,
    Heater,
    Decorations,
    WaterConditioner
}
```

## Step 2 — Core Domain: `FishTankUpgradeCatalog` (NEW FILE)

**File**: `src/Slums.Core/Characters/FishTankUpgradeCatalog.cs`

```csharp
namespace Slums.Core.Characters;

public static class FishTankUpgradeCatalog
{
    public static int GetCost(FishTankUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            FishTankUpgradeType.BetterFilter => 15,
            FishTankUpgradeType.Heater => 20,
            FishTankUpgradeType.Decorations => 12,
            FishTankUpgradeType.WaterConditioner => 18,
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }

    public static string GetName(FishTankUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            FishTankUpgradeType.BetterFilter => "Better Filter",
            FishTankUpgradeType.Heater => "Heater",
            FishTankUpgradeType.Decorations => "Decorations",
            FishTankUpgradeType.WaterConditioner => "Water Conditioner",
            _ => throw new ArgumentOutOfRangeException(nameof(upgradeType), upgradeType, null)
        };
    }

    public static int GetMotherHealthBonusPerUpgrade => 1;
}
```

## Step 3 — Core Domain: Modify `OwnedPet` 

**File**: `src/Slums.Core/Characters/OwnedPet.cs`

Add upgrade tracking fields and methods following the exact `OwnedPlant` pattern:

- Add properties: `HasBetterFilter`, `HasHeater`, `DecorationsPaidWeek`, `WaterConditionerPaidWeek`
- Add methods: `HasActiveUpgrade(FishTankUpgradeType, int currentWeek)`, `CanPurchaseUpgrade(FishTankUpgradeType, int currentWeek)`, `PurchaseUpgrade(FishTankUpgradeType, int currentWeek)`, `GetActiveUpgradeCount(int currentWeek)`
- Update `Restore()` factory to accept the 4 new upgrade fields
- The existing `Type`, `AcquiredOnDay`, `LastUpkeepPaidWeek` properties remain unchanged
- `Create()` factory stays unchanged (no upgrades at purchase time)

## Step 4 — Core Domain: Modify `HouseholdAssetsState`

**File**: `src/Slums.Core/Characters/HouseholdAssetsState.cs`

- Add `GetFishTank()` method — returns the first `OwnedPet` with `PetType.Fish`, or null
- Add `TryUpgradeFishTank(FishTankUpgradeType, int currentWeek)` method — mirrors `TryUpgradePlant`
- Modify `GetMotherDailyHealthBonus()` — after the pet loop's existing bonus calculation, add: `+ FishTankUpgradeCatalog.GetMotherHealthBonusPerUpgrade * fishTank.GetActiveUpgradeCount(currentWeek)` when a fish tank exists

## Step 5 — Core Domain: Add `GameSession.UpgradeFishTank()` 

**File**: `src/Slums.Core/State/GameSession.cs`

Add a new method following the exact pattern of `UpgradePlant()` at line 1912:

```csharp
public bool UpgradeFishTank(FishTankUpgradeType upgradeType)
{
    var before = CaptureStats();
    if (World.CurrentLocationId != LocationId.Home)
    {
        RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), "Not at home");
        RaiseEvent("You need to be home to work on the fish tank.");
        return false;
    }

    var fishTank = Player.HouseholdAssets.GetFishTank();
    if (fishTank is null)
    {
        RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), "No fish tank");
        RaiseEvent("You don't have a fish tank to upgrade.");
        return false;
    }

    var cost = FishTankUpgradeCatalog.GetCost(upgradeType);
    if (Player.Stats.Money < cost)
    {
        RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), $"Not enough money (need {cost} LE, have {Player.Stats.Money} LE)");
        RaiseEvent($"Not enough money. {FishTankUpgradeCatalog.GetName(upgradeType)} costs {cost} LE.");
        return false;
    }

    if (!Player.HouseholdAssets.TryUpgradeFishTank(upgradeType, CurrentWeek))
    {
        RecordMutation(MutationCategories.GuardRejected, "UpgradeFishTank", before, CaptureStats(), $"Already active: {upgradeType}");
        RaiseEvent($"{FishTankUpgradeCatalog.GetName(upgradeType)} is already active.");
        return false;
    }

    Player.Stats.ModifyMoney(-cost);
    RaiseEvent($"You set up {FishTankUpgradeCatalog.GetName(upgradeType)} for the fish tank ({cost} LE).");
    RecordMutation(MutationCategories.HouseholdAsset, "UpgradeFishTank", before, CaptureStats(), $"Bought {FishTankUpgradeCatalog.GetName(upgradeType)} for {cost} LE");
    return true;
}
```

## Step 6 — Application: Add `ManageFishTank` to enum

**File**: `src/Slums.Application/HouseholdAssets/HouseholdAssetActionType.cs`

Add `ManageFishTank` to the enum.

## Step 7 — Application: New Fish Tank Upgrade types (4 NEW FILES)

**File**: `src/Slums.Application/HouseholdAssets/FishTankUpgradeMenuContext.cs`
- Record: `(OwnedPet FishTank, PetDefinition Definition, int CurrentWeek, int Money)`
- Static `Create(GameSession)` factory

**File**: `src/Slums.Application/HouseholdAssets/FishTankUpgradeMenuStatus.cs`
- Record: `(FishTankUpgradeType UpgradeType, string Name, int Cost, bool CanExecute, string Note)`

**File**: `src/Slums.Application/HouseholdAssets/FishTankUpgradeMenuQuery.cs`
- Lists all 4 upgrades with cost, can-execute, and descriptive notes (permanent vs recurring)

**File**: `src/Slums.Application/HouseholdAssets/FishTankUpgradeCommand.cs`
- Routes to `GameSession.UpgradeFishTank(upgradeType)`

## Step 8 — Application: Modify `HouseholdAssetsMenuQuery.AddHomeStatuses()`

**File**: `src/Slums.Application/HouseholdAssets/HouseholdAssetsMenuQuery.cs`

After the plant management loop (line ~105), add:

```csharp
var fishTank = context.Assets.GetFishTank();
if (fishTank is not null)
{
    var fishDefinition = PetRegistry.GetByType(PetType.Fish);
    var careState = fishTank.IsUpkeepPaidForWeek(context.CurrentWeek) ? "care covered" : "care due";
    var upgradeCount = fishTank.GetActiveUpgradeCount(context.CurrentWeek);
    statuses.Add(new HouseholdAssetsMenuStatus(
        HouseholdAssetActionType.ManageFishTank,
        "Manage Fish Tank",
        $"{careState} | upgrades active {upgradeCount}",
        true,
        BuildFishTankManageNote(fishDefinition, fishTank, context.CurrentWeek),
        PetType.Fish));
}
```

Add `BuildFishTankManageNote()` helper method.

## Step 9 — Application: Modify `HouseholdAssetsCommand`

**File**: `src/Slums.Application/HouseholdAssets/HouseholdAssetsCommand.cs`

Add case: `HouseholdAssetActionType.ManageFishTank => true` (like ManagePlant, just a navigation action).

## Step 10 — Game UI: `FishTankUpgradeScreen` (NEW FILE)

**File**: `src/Slums.Game/Screens/FishTankUpgradeScreen.cs`

Mirror `PlantUpgradeScreen.cs` exactly, but:
- Uses `FishTankUpgradeMenuContext`, `FishTankUpgradeMenuStatus`, `FishTankUpgradeMenuQuery`, `FishTankUpgradeCommand`
- Title: "=== Fish Tank Upgrades ==="
- Shows care status and upgrade count
- Detail panel shows upgrade name, cost, note, fish tank name

## Step 11 — Game UI: Modify `HouseholdAssetsScreen.ExecuteSelection()`

**File**: `src/Slums.Game/Screens/HouseholdAssetsScreen.cs`

In `ExecuteSelection()`, add handling for `ManageFishTank`:

```csharp
if (status.ActionType == HouseholdAssetActionType.ManageFishTank)
{
    IsFocused = false;
    GameHost.Instance.Screen = new FishTankUpgradeScreen(
        GameRuntime.ScreenWidth, GameRuntime.ScreenHeight,
        _gameState, FishTankUpgradeMenuContext.Create(_gameState),
        this, _parentScreen);
    return;
}
```

Also update `GetLocationTip()` home case to mention fish tank management.

## Step 12 — Infrastructure: Modify `OwnedPetSnapshot`

**File**: `src/Slums.Infrastructure/Persistence/OwnedPetSnapshot.cs`

Add 4 upgrade fields to the snapshot record:
- `bool HasBetterFilter`
- `bool HasHeater`
- `int DecorationsPaidWeek`
- `int WaterConditionerPaidWeek`

Update `Capture()` and `Restore()` to handle these fields.

Update JSON serialization context if needed (`SaveGameJsonContext.cs`).

## Step 13 — Tests

### New: `tests/Slums.Core.Tests/Characters/OwnedPetFishTankUpgradeTests.cs`
- `HasActiveUpgrade_BetterFilterPermanent` — returns true after purchase
- `HasActiveUpgrade_HeaterPermanent` — returns true after purchase
- `HasActiveUpgrade_DecorationsRecurring` — active in paid week, expired next week
- `HasActiveUpgrade_WaterConditionerRecurring` — active in paid week, expired next week
- `CanPurchaseUpgrade_ReturnsFalseWhenAlreadyActive`
- `GetActiveUpgradeCount_AllActive_Returns4`
- `GetActiveUpgradeCount_NoneActive_Returns0`
- `Restore_RoundTripsAllUpgradeFields`

### Modify: `tests/Slums.Core.Tests/Characters/HouseholdAssetsStateTests.cs`
- Add tests for `GetFishTank()` returns tank when present, null when absent
- Add tests for `TryUpgradeFishTank()` success/failure cases
- Add test for `GetMotherDailyHealthBonus()` includes fish tank upgrade bonus

### New: `tests/Slums.Application.Tests/HouseholdAssets/FishTankUpgradeMenuQueryTests.cs`
- Verify 4 upgrade items returned
- Verify can-execute depends on money and active state

### Modify: `tests/Slums.Application.Tests/HouseholdAssets/HouseholdAssetsMenuQueryTests.cs`
- Verify "Manage Fish Tank" appears at home when fish tank is owned
- Verify "Manage Fish Tank" does not appear when no fish tank is owned

### Modify: `tests/Slums.Infrastructure.Tests/JsonSaveGameStoreTests.cs`
- Verify fish tank with upgrades round-trips through save/load

## Step 14 — Build & Test

```
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

---

## Fish Tank Upgrades Summary

| Upgrade | Type | Cost | Effect |
|---------|------|------|--------|
| Better Filter | Permanent | 15 LE | +1 mother health bonus (permanent) |
| Heater | Permanent | 20 LE | +1 mother health bonus (permanent) |
| Decorations | Recurring weekly | 12 LE | +1 mother health bonus (active week only) |
| Water Conditioner | Recurring weekly | 18 LE | +1 mother health bonus (active week only) |

This mirrors the plant upgrade structure exactly (2 permanent + 2 recurring) with appropriate costs for the fish tank context.
