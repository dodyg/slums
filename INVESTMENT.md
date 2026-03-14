# Investment System Implementation

## Status: IN PROGRESS (Core layer mostly complete, needs compile fixes)

## Goal

Implement the #1 priority from PLAN.MD: **Small Business Investments** - Allow players to invest in small local businesses for weekly passive income with risk mechanics.

## Current Build Status

**DOES NOT COMPILE** - Fix these errors before proceeding:

1. **Line 1647 in GameSession.cs**: Change `!CheckInvestmentEligibility(definition)` to `!CheckInvestmentEligibility(definition).IsEligible`
2. **CA5394 warnings (lines 1800, 1812, 1814, 1841, 1853, 1865)**: Add `#pragma warning disable CA5394` / `restore` around the `ResolveSingleInvestment` method (or use the existing `_sharedRandom` pattern)
3. **CA1062 (line 1881)**: Add null check at start of `RestoreInvestmentState`: `ArgumentNullException.ThrowIfNull(investments);`

## Completed Work

### Core Layer (Slums.Core/Investments/)

| File | Purpose |
|------|---------|
| `InvestmentType.cs` | Enum with 6 types: FoulCartPartnership, MicroLaundryService, ScrapCollectionCrew, KioskShare, InformalMarketStall, HashishCourierStake |
| `InvestmentRiskProfile.cs` | Record with risk parameters (WeeklyFailureChance, ExtortionChance, etc.) and static presets (Low, Medium, MediumHigh, High) |
| `InvestmentDefinition.cs` | Record for static investment definitions loaded from registry |
| `InvestmentRegistry.cs` | Static registry containing all 6 investment definitions with costs, returns, and unlock requirements |
| `Investment.cs` | Record for active investments with state management (WeeksActive, IsSuspended, etc.) |
| `InvestmentEligibility.cs` | Record for eligibility check results (IsEligible, FailureReasons) |
| `MakeInvestmentResult.cs` | Record for investment action results |
| `InvestmentResolution.cs` | Record for single investment resolution outcome |
| `InvestmentResolutionSummary.cs` | Class to aggregate weekly resolution results |
| `InvestmentSnapshot.cs` | Record for persistence serialization |

### State Layer (Slums.Core/State/)

| File | Purpose |
|------|---------|
| `GameInvestmentState.cs` | Internal class holding ActiveInvestments list, TotalInvestmentEarnings, WeeksSinceLastResolution |

### GameSession Integration

The following methods were added to `GameSession.cs`:

- `IReadOnlyList<Investment> ActiveInvestments` - property
- `int TotalInvestmentEarnings` - property
- `IReadOnlyList<InvestmentDefinition> GetAvailableInvestments()` - lists unlocked investments
- `InvestmentEligibility CheckInvestmentEligibility(InvestmentDefinition)` - checks money, ownership, relationships, requirements
- `MakeInvestmentResult MakeInvestment(InvestmentType)` - executes purchase
- `InvestmentResolutionSummary ResolveWeeklyInvestments(Random?)` - weekly tick
- `InvestmentResolution ResolveSingleInvestment(Investment, Random)` - handles risk events
- `void RestoreInvestmentState(IEnumerable<InvestmentSnapshot>, int)` - save game restore

## Remaining Tasks

### 1. Fix Compile Errors (URGENT)

Fix the 3 issues listed in "Current Build Status" above.

### 2. Wire Weekly Resolution into EndDay

The `ResolveWeeklyInvestments()` method exists but isn't called. Add to `EndDay()` method:
```csharp
if (Date.DayOfWeek == DayOfWeek.Monday && _investmentState.ActiveInvestments.Count > 0)
{
    ResolveWeeklyInvestments();
}
```

### 3. Update Persistence (GameSessionSnapshot)

Add to `GameSessionSnapshot`:
```csharp
public IReadOnlyList<InvestmentSnapshot> Investments { get; init; }
public int TotalInvestmentEarnings { get; init; }
```

Update `CreateSnapshot()` and restore logic in GameSession.

### 4. Application Layer (Slums.Application/)

Create:
- `Investments/InvestmentQueries.cs` - query for available investments and eligibility
- `Investments/MakeInvestmentCommand.cs` - command to execute investment

### 5. UI Layer (Slums.Game/)

Create:
- `Screens/InvestmentMenuScreen.cs` - display available investments, costs, risks
- Wire into contact interactions (e.g., talk to Hajj Mahmoud to unlock Foul Cart)
- Add investment status to HUD or status screen

### 6. Tests

Create `tests/Slums.Core.Tests/Investments/`:
- `InvestmentEligibilityTests.cs` - test money requirements, relationship unlocks, background requirements
- `InvestmentResolutionTests.cs` - test risk events, income calculation, loss conditions
- `InvestmentRegistryTests.cs` - verify all definitions are valid

## Business Types Reference (from PLAN.MD)

| Business | Cost | Risk | Weekly Return | Unlock |
|----------|------|------|---------------|--------|
| Foul cart partnership | 150 LE | Low | 8-12 LE | Trust >= 30 with Hajj Mahmoud |
| Micro-laundry service | 200 LE | Low | 10-15 LE | Any background |
| Scrap collection crew | 180 LE | Medium | 15-22 LE | Street smarts (L2+) or ex-prisoner |
| Kiosk share (koshk) | 250 LE | Medium | 18-25 LE | Trust >= 40 with Umm Karim |
| Informal market stall | 220 LE | Medium-High | 20-30 LE | Trust >= 25 with Dokki contact |
| Hashish courier stake | 300 LE | High | 35-50 LE | Criminal contact + trust + crime path |

## Risk Types

- **Business failure**: Investment lost, weeklyFailureChance
- **Gang extortion**: Pay or suspend, extortionChance
- **Police heat**: Pressure increase, policeHeatChance
- **Partner betrayal**: Investment lost, betrayalChance

## Architecture Notes

- All investment definitions are in `InvestmentRegistry` (static), not loaded from JSON
- `GameInvestmentState` is internal to Slums.Core
- Investment resolution uses `Random` - the existing `_sharedRandom` field in GameSession
- `InvestmentSnapshot` is used for save/load serialization
- Dependency flow: Core has no external dependencies

## Validation Commands

After fixes, run:
```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
```
