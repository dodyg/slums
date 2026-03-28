# Slums Plan

## Purpose

This is the single active planning document for the repository.

It should stay short and current. Historical expansion notes and old gap lists are archived elsewhere and should not be treated as the live roadmap.

## Read First

Before making major changes, read these files in order:

1. `REQS.md`
2. `PLAN.MD`
3. `AGENTS.md`

## Current State

The game is already past the initial vertical-slice stage. The current repo supports:

- a working multi-project .NET 10 solution with shared build settings
- SadConsole game flow with main menu, background selection, narrative, and gameplay screens
- core survival simulation for money, hunger, energy, health, stress, rent, food, and time
- mother-clinic care at clinic-capable locations with schedule-aware availability and variable pricing
- clinic travel menu accessible from Home for integrated travel+visit action
- entertainment activities across cafe, bar, and billiards-capable locations with cost, time, stress, and energy tradeoffs
- three playable backgrounds with distinct systemic advantages and liabilities
- travel across Imbaba, Dokki, and Ard al-Liwa
- honest work progression with reliability, variant unlocks, and temporary lockouts
- crime routes with contact-specific access, risk, aftermath, and police pressure behavior
- small-business investments with location-based opportunities, weekly passive income, and risk events
- recurring NPC interactions driven by trust, faction standing, and relationship memory
- conversation pool system with 100+ unique dialogues per NPC context to prevent repeats for 100 days
- district-specific random events with event-history-aware gating
- save/load built around `GameSession` snapshots plus `LoadedGameSession` handoff
- player-facing screen actions routed through application commands and menu/action queries
- `GameSession` kept as the runtime boundary while orchestration-heavy narrative, work/crime aftermath, and investment logic is delegated to focused core planners/evaluators/calculators
- fail-fast bootstrap for repository-owned JSON content instead of silent fallback to default registries
- multiple ending paths based on long-term behavior, pressure, and survival trajectory
- world-enrichment mechanics planned: day-of-week rhythms, seasonal calendar, weather, sleep quality, rumors, territory dynamics, NPC-to-NPC economy (see dedicated sections below)

## Architecture Guardrails

- `Slums.Core` owns simulation rules and the EntitiesDb-backed `GameSession` runtime boundary. It must not depend on SadConsole, Ink, or file IO.
- `Slums.Application` owns contracts and orchestration, including the `LoadedGameSession` persistence handoff plus player-facing action commands and menu/action queries. It must not depend on SadConsole.
- `Slums.Infrastructure` owns `GameSession` snapshot persistence, static content loading, and environment-specific adapters. Repo-owned JSON content should fail fast when it is missing or invalid.
- `Slums.Narrative.Ink` owns Ink loading, variable sync, and narrative outcome mapping. Missing or invalid story content should fail fast rather than falling back.
- `Slums.Game` owns screen flow, HUDs, menus, and input. It must not become the home of business rules.
- Shared narrative signal and scene-trigger rules that must be reused by both `GameSession` and application queries belong in `Slums.Core`, not in the UI layer or a duplicate application-only helper.

Dependency flow must remain inward:

```text
Slums.Game -> Slums.Application
Slums.Application -> Slums.Core
Slums.Infrastructure -> Slums.Application
Slums.Infrastructure -> Slums.Core
Slums.Narrative.Ink -> Slums.Application
Slums.Narrative.Ink -> Slums.Core
Slums.Core -> nothing
```

## Content Boundaries

Use Ink for authored scenes:

- intros
- NPC conversations
- crime aftermath scenes
- event scenes
- ending scenes

Use C# for simulation:

- economy and household pressure
- work and crime calculations
- relationship memory and faction state
- police pressure and event eligibility
- ending checks

Keep authored narrative in `content/ink/` and compile to `content/ink/main.json`.
Keep static world data in `content/data/` when it does not need hardcoded rules.

## Current Priorities

### 1. Active Skill Training

Status: planned. See dedicated section below.

### 2. World-Enrichment Mechanics

Implement in dependency order after skill training:
1. Day-of-Week Rhythms (quick win, no dependencies)
2. Sleep Quality (quick win, standalone)
3. Seasonal Calendar (depends on day-of-week)
4. Weather (depends on seasonal calendar)
5. Rumors (depends on relationships)
6. Territory Dynamics (depends on factions)
7. Debt and NPC Economy (depends on relationships)

### 3. UI Clarity

Make the deeper systems legible to the player.

- explain why work variants improve, worsen, or lock out
- explain why crime routes are available, blocked, or unusually risky
- surface debt, suspicion, relationship-memory, and district-pressure signals in menus or logs
- make investment risk, suspension, and extortion outcomes easier to read from the UI

### 4. Balance Pass

Tune the expanded simulation without flattening background identity.

- review pay, stress, pressure, reliability, and lockout values
- review entertainment costs and reductions against survival pressure
- review investment returns and disruption rates against the economy
- review district event frequency and recurrence
- add regression tests around balance-sensitive thresholds

### 5. More Authored Reactivity

Expand content that reflects the systems already in place.

- add more memory-reactive NPC variants
- add more spillover follow-up scenes
- add more district-event and background-specific aftermath content
- expand conversation pools beyond 100 days if needed

### 6. Content Breadth

Broaden the world through additional authored and systemic slices instead of more architecture churn.

- add more location-specific jobs, crime routes, and relationship hooks
- expand recurring-event variety so long runs feel less cyclical
- add more endings or epilogue variants only where they reflect existing systems cleanly
- prefer content and UX improvements over another repo-wide refactor unless a new hotspot clearly appears

### 7. Tracing and Diagnostics

Status: implemented.

Add structured diagnostic logging across all gameplay layers so agents can quickly detect logic bugs and understand gameflow.

**Phase 1 -- Diagnostic record types in `Slums.Core`:**

- Define lightweight `GameMutationRecord` (sealed record with `RunId`, `Timestamp`, `Category`, `Action`, `Before`/`After` snapshots as key-value dictionaries, `Reason` string) in `Slums.Core/Diagnostics/`.
- `GameSession` mutation methods (`WorkJob`, `CommitCrime`, `EndDay`, `MakeInvestment`, `Travel`, `FeedFamily`, `TakeMotherToClinic`, `Entertain`, etc.) append a `GameMutationRecord` to a `List<GameMutationRecord> Mutations` on `GameSession`.
- Guard failures (action blocked, rule rejection, ending triggered) also produce records with a `GuardRejected` or `EndingTriggered` category and include the rule/condition name in `Reason`.
- This keeps `Slums.Core` dependency-free -- it only produces records; it does not log them.

**Phase 2 -- Application-layer logging with `ILogger`:**

- Add `ILogger<T>` to all application commands (`WorkCommand`, `CrimeCommand`, `AdvanceTimeCommand`, `EntertainmentCommand`, `ShopCommand`, `TravelCommand`, `ClinicTravelCommand`, `GameActionCommand`) and use-case classes (`SaveGameUseCase`, `LoadGameUseCase`).
- After each command executes, emit structured `ILogger` events using the existing `LoggerMessage.Define` pattern, pulling data from the `GameMutationRecord` produced by Core.
- Each log event includes `RunId` from `GameSession.RunId` for session correlation.
- Add `ILogger<T>` to all application queries that perform filtering or eligibility evaluation so rejected actions log why they were unavailable.

**Phase 3 -- State snapshot logging at key boundaries:**

- At day-start (`EndDay` entry) and day-end (`EndDay` completion), emit a compact snapshot log event containing core survival stats: `Money`, `Hunger`, `Energy`, `Health`, `Stress`, `RentDaysUnpaid`, `PolicePressure`, `Day`, `Location`.
- At save and load boundaries in `SaveGameUseCase`/`LoadGameUseCase`, emit a snapshot log event so agents can correlate save state with subsequent behavior.
- Snapshot events use a dedicated `LogLevel.Information` with a distinct `EventId` range (200-299) to separate them from mutation events.

**Phase 4 -- Narrative and infrastructure tracing:**

- Extend `InkNarrativeService` logging to include `RunId` in all existing and new log events.
- Add logging to `JsonSaveGameStore` and `JsonContentRepository` for all paths including success cases (not just failures).
- Log scene transitions, random-event selections, and narrative follow-up triggers with the scene/event name and eligibility reasoning.

**Phase 5 -- Tests and validation:**

- Add `Slums.Application.Tests` tests that verify commands produce the correct `GameMutationRecord` entries after execution.
- Add tests that verify guard rejections produce records with the expected `Reason`.
- Verify all existing tests still pass after logging is introduced (logging is additive, not behavioral).

## Architecture Hardening Pass

Status: implemented.

Completed steps:

1. Moved player-facing gameplay mutations behind `Slums.Application` commands/queries instead of mutating `GameSession` directly from `Slums.Game`.
2. Centralized shared narrative signal rules in `Slums.Core.Narrative` so both runtime orchestration and application queries reuse the same logic.
3. Kept `GameSession` as the canonical runtime boundary while extracting orchestration-heavy work into focused core collaborators such as `NarrativeFollowUpPlanner`, `CrimeNarrativePlanner`, `WorkNarrativePlanner`, `InvestmentEligibilityEvaluator`, and `InvestmentResolutionCalculator`.
4. Reduced unnecessary application coupling by shifting `InvestmentMenuContext` and talk-scene creation toward narrower snapshots instead of carrying the full `GameSession`.
5. Validated the supported root build plus all five executable test projects after the refactor.

## Entertainment Activities

Status: implemented.

Completed steps:

1. Added entertainment venue flags (`HasCafe`, `HasBar`, `HasBilliards`) to location data.
2. Implemented 6 activity types: coffee, shisha, billiards, bar drinking, football watching, and social hangout.
3. Added entertainment availability, status queries, command flow, and the SadConsole entertainment screen.
4. Wired entertainment activities into gameplay with money, time, stress, and energy effects plus automated tests.

## Household Care Enhancement

Status: implemented.

Completed steps:

1. Added location-driven clinic access so only clinic-capable locations offer mother treatment.
2. Added variable clinic pricing plus relationship and medical-skill discounts.
3. Added clinic opening-day rules and surfaced the paid clinic option in the shop flow and status page.

## Clinic Travel From Home

Status: implemented.

Completed steps:

1. Added `GetClinicLocations()`, `GetClinicTravelOption()`, and `TravelAndTakeMotherToClinic()` to GameSession for integrated travel+clinic action.
2. Created `ClinicTravelOption` and `TravelAndClinicVisitResult` records in Slums.Core/State.
3. Created `ClinicTravelMenuQuery`, `ClinicTravelMenuContext`, `ClinicTravelMenuStatus`, and `ClinicTravelOptionContext` in Slums.Application/Activities.
4. Created `ClinicTravelScreen` in Slums.Game/Screens for clinic selection UI.
5. Wired "Take Mother to Clinic" action into GameScreen, visible when player is at Home.

## Walking as Travel Alternative

Status: implemented.

Completed steps:

1. Added `TryWalkTo` method in GameSession that costs no money, 3x energy, and 3x time compared to paid transport.
2. Updated TravelScreen to show walking option with cost preview.
3. Added background-specific flavor for Sudanese Refugee walking in Dokki.

## Active Skill Training

Status: planned.

Requirements: see REQS.md "Active Skill Training" section.

### Design

Players choose to spend evening time, energy, and sometimes money to deliberately train a specific skill. Training follows the same vertical-slice pattern as entertainment: Core data model + registry, Core logic on GameSession, Application query/command/context, Game screen, wired into the action menu.

### Phase 1 -- Core data model and registry

Create `src/Slums.Core/Training/` with three files following the Entertainment pattern:

1. **`TrainingActivityType.cs`** -- enum with one member per skill: `StudyMedical`, `PracticePersuasion`, `StreetDice`, `RooftopExercise`.

2. **`TrainingActivity.cs`** -- sealed record:
   ```
   TrainingActivity(
       TrainingActivityType Type,
       SkillId Skill,
       string Name,
       string Description,
       int MoneyCost,
       int TimeCostMinutes,
       int EnergyCost,
       NpcId? RequiredNpc,
       int RequiredTrust,
       bool RequiresHome)
   ```
   `RequiredNpc` is nullable; Physical training has no NPC gate.

3. **`TrainingRegistry.cs`** -- static class with `AllActivities` returning the four hardcoded activities. Each entry encodes the REQS.md table values:

   | Type | Skill | Money | Time | Energy | NPC | Trust | Home |
   |------|-------|-------|------|--------|-----|-------|------|
   | StudyMedical | Medical | 0 | 150 | 18 | NurseSalma | 10 | true |
   | PracticePersuasion | Persuasion | 10 | 120 | 15 | FixerUmmKarim | 5 | false |
   | StreetDice | StreetSmarts | 15 | 120 | 12 | RunnerYoussef | 5 | false |
   | RooftopExercise | Physical | 0 | 180 | 20 | null | 0 | true |

### Phase 2 -- Core logic on GameSession

Add to `src/Slums.Core/State/GameSession.cs`:

1. **Daily training tracker** -- add `private Dictionary<SkillId, bool> _trainedSkillsToday` initialized empty, reset in `EndDay()`.

2. **`GetAvailableTrainingActivities()`** -- filters `TrainingRegistry.AllActivities` by:
   - player is at Home if activity `RequiresHome` (or at correct general location otherwise)
   - NPC trust gate met via `Relationships.GetNpcRelationship(activity.RequiredNpc).Trust >= activity.RequiredTrust`
   - skill not at cap (`Player.Skills.GetLevel(activity.Skill) < SkillState.MaxLevel`)
   - not already trained today (`_trainedSkillsToday.ContainsKey(activity.Skill)` is false)
   Returns the filtered list.

3. **`TryPerformTraining(TrainingActivity activity)`** -- returns `bool`, guards:
   - activity is in the available list
   - `Player.Stats.Energy >= activity.EnergyCost`
   - `Player.Stats.Money >= activity.MoneyCost`
   - clock hour is in evening window (18:00-22:00) via `Clock.Hour >= 18 && Clock.Hour < 22`
   - skill not already at cap (double-check)
   
   On success:
   - deduct money if `activity.MoneyCost > 0`
   - call `AdvanceTime(activity.TimeCostMinutes)`
   - deduct energy: `Player.Stats.ModifyEnergy(-activity.EnergyCost)`
   - call existing `ApplySkillGain(activity.Skill)`
   - mark `_trainedSkillsToday[activity.Skill] = true`
   - apply background-specific modifiers:
     - MedicalSchoolDropout + StudyMedical: `Player.Stats.ModifyStress(-3)` (relief from studying)
     - ReleasedPoliticalPrisoner + StreetDice: energy cost reduced by 3
     - SudaneseRefugee + RooftopExercise: energy cost reduced by 3
   - raise event message with skill name and new level
   - append `GameMutationRecord` with category `Training`
   
   On failure: raise event with reason (too tired, not enough money, wrong time, already trained, skill maxed), return false.

4. **Reset in `EndDay()`** -- clear `_trainedSkillsToday`.

5. **Persistence** -- add `TrainedSkillsToday: Dictionary<string, bool>` to `GameSessionSnapshot` capture/restore so mid-day saves preserve training state.

### Phase 3 -- Application query and command

Create in `src/Slums.Application/Activities/`:

1. **`TrainingMenuContext.cs`** -- sealed record with `static Create(GameSession)` factory, carrying:
   - available activities list
   - current location name
   - player money and energy
   - current hour
   - trained-today set

2. **`TrainingMenuStatus.cs`** -- sealed record:
   ```
   TrainingMenuStatus(
       TrainingActivity Activity,
       bool CanAfford,
       bool HasEnergy,
       bool RightTime,
       bool NpcTrustMet,
       bool NotTrainedToday,
       bool NotAtCap,
       bool CanTrain,
       string? UnavailabilityReason)
   ```

3. **`TrainingMenuQuery.cs`** -- builds `List<TrainingMenuStatus>` from context, computing each guard and composing a human-readable `UnavailabilityReason` from the first failed check.

4. **`TrainingCommand.cs`** -- thin delegation:
   ```
   bool Execute(GameSession gameSession, TrainingActivity activity)
       => gameSession.TryPerformTraining(activity);
   ```

### Phase 4 -- Game layer wiring

1. **`GameActionId.cs`** -- add `Train` member.

2. **`GameActionMenuContext.cs`** -- add `bool HasTrainingAvailable` to the context record. In `Create()`, compute from `gameSession.GetAvailableTrainingActivities().Count > 0`.

3. **`GameActionMenuQuery.cs`** -- add conditional:
   ```
   if (context.HasTrainingAvailable)
       actions.Add(new GameAction(GameActionId.Train, "Train Skills"));
   ```
   Insert after Entertainment in the action list.

4. **`GameScreen.cs`**:
   - Add `case GameActionId.Train: ShowTrainingMenu(); break;` in `ExecuteAction()`.
   - Add `ShowTrainingMenu()` method following the `ShowEntertainmentMenu()` pattern: build context, query statuses, construct screen, swap focus.

5. **`TrainingScreen.cs`** in `src/Slums.Game/Screens/` -- follows `EntertainmentScreen.cs` pattern:
   - Two-panel layout: left list of available training activities, right detail panel
   - Shows cost, time, energy, NPC requirement, and availability per activity
   - Keyboard (Up/Down/Enter/Escape) and mouse navigation
   - `PerformSelectedTraining()` calls `TrainingCommand.Execute()`, adds event log, returns to parent
   - Grayed-out entries with reason text for unavailable activities

### Phase 5 -- Ink narrative content

Add optional training flavor knots in `content/ink/`:

1. One knot per training type for the first-time experience (e.g., `training_medical_first`, `training_dice_first`).
2. Background-specific training variants (e.g., `training_medical_dropout`, `training_physical_refugee`).
3. NPCs may comment on training progress in conversation pools (e.g., Salma noticing improved medical knowledge).

These are optional for the initial implementation; the feature works without them using event log messages only.

### Phase 6 -- Tests

1. **`tests/Slums.Core.Tests/Training/TrainingTests.cs`**:
   - training succeeds when all guards pass and grants +1 skill
   - training fails when energy too low
   - training fails when money too low (for paid activities)
   - training fails outside evening hours
   - training fails when NPC trust below gate
   - training fails when skill at cap
   - training fails when already trained that skill today
   - training fails when not at Home for home-required activities
   - training resets daily tracker on EndDay
   - background-specific energy/stress modifiers apply correctly
   - daily tracker survives save/load round-trip

2. **`tests/Slums.Application.Tests/Activities/TrainingMenuQueryTests.cs`**:
   - query returns correct availability per activity given various stat/NPC configurations
   - blocking reasons are accurate and human-readable
   - context captures trained-today state correctly

3. **`tests/Slums.Infrastructure.Tests/`** -- extend existing save/load round-trip tests to verify `TrainedSkillsToday` persists and restores correctly.

4. **`tests/Slums.Game.Tests/`** -- add `GameActionMenuQuery` test verifying `Train` action appears when training is available and hidden when not.

### Phase 7 -- Diagnostic integration

Training follows the existing tracing pattern:

- `TryPerformTraining` appends a `GameMutationRecord` with category `Training`, including skill trained, costs paid, old/new skill level, and background modifier applied.
- Guard failures produce records with category `GuardRejected` and the specific rule name in `Reason`.
- The application `TrainingCommand` logs the mutation via `ILogger` if a logger is injected (follow existing command logging pattern).

### File Summary

New files:
- `src/Slums.Core/Training/TrainingActivityType.cs`
- `src/Slums.Core/Training/TrainingActivity.cs`
- `src/Slums.Core/Training/TrainingRegistry.cs`
- `src/Slums.Application/Activities/TrainingMenuContext.cs`
- `src/Slums.Application/Activities/TrainingMenuStatus.cs`
- `src/Slums.Application/Activities/TrainingMenuQuery.cs`
- `src/Slums.Application/Activities/TrainingCommand.cs`
- `src/Slums.Game/Screens/TrainingScreen.cs`
- `tests/Slums.Core.Tests/Training/TrainingTests.cs`
- `tests/Slums.Application.Tests/Activities/TrainingMenuQueryTests.cs`

Modified files:
- `src/Slums.Core/State/GameSession.cs` -- add training tracker, `GetAvailableTrainingActivities()`, `TryPerformTraining()`, reset in `EndDay()`
- `src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs` -- add `TrainedSkillsToday` capture/restore
- `src/Slums.Application/Activities/GameActionId.cs` -- add `Train`
- `src/Slums.Application/Activities/GameActionMenuContext.cs` -- add `HasTrainingAvailable`
- `src/Slums.Application/Activities/GameActionMenuQuery.cs` -- add Train action conditional
- `src/Slums.Game/Screens/GameScreen.cs` -- add dispatch and `ShowTrainingMenu()`

## Day-of-Week Rhythms

Status: planned.

Requirements: see REQS.md "Day-of-Week Rhythms" section.

### Design

Add a `DayOfWeek` enum derived from `GameClock.Day`, a `DayScheduleModifiers` record, and a `DayScheduleRegistry` with per-day modifiers. Work queries, food purchase, crime detection, and event eligibility consult the schedule. The HUD shows the current day name.

### Phase 1 -- Core types

Create `src/Slums.Core/Clock/` additions:

1. **`GameDayOfWeek.cs`** -- enum: `Saturday=0, Sunday=1, Monday=2, Tuesday=3, Wednesday=4, Thursday=5, Friday=6`.

2. **`DayScheduleModifiers.cs`** -- sealed record:
   ```
   DayScheduleModifiers(
       GameDayOfWeek Day,
       string DayName,
       bool MarketsClosed,
       int FoodCostModifier,
       int JobPayModifier,
       int CrimeDetectionModifier,
       bool PrayerGatheringAvailable,
       bool ClinicDiscount,
       int ClinicDiscountAmount,
       int InvestmentRevenueModifier,
       string[] BlockedJobTypes)
   ```

3. **`DayScheduleRegistry.cs`** -- static class returning `AllModifiers` as a dictionary keyed by `GameDayOfWeek`. Encode the REQS.md table:
   - Friday: markets partially closed, no bakery/call-center/cafe work, prayer gathering, crime detection -10%, foul cart revenue x2
   - Saturday: food costs -2 LE, laundry pay +2 LE
   - Monday: clinic discount -5 LE
   - Wednesday: investment returns distributed
   - Other days: zero modifiers

4. Add computed property `GameDayOfWeek GetDayOfWeek()` on `GameClock`:
   ```
   return (GameDayOfWeek)((Day - 1) % 7);
   ```

### Phase 2 -- Core integration on GameSession

1. Add `public DayScheduleModifiers GetCurrentSchedule()` on `GameSession` that looks up `Clock.GetDayOfWeek()` in `DayScheduleRegistry`.

2. Modify `WorkJob()` / job availability queries to check `schedule.BlockedJobTypes` and apply `schedule.JobPayModifier`.

3. Modify food purchase logic to apply `schedule.FoodCostModifier`.

4. Modify crime detection calculation to apply `schedule.CrimeDetectionModifier`.

5. Modify investment weekly resolution to apply `schedule.InvestmentRevenueModifier`.

6. Add Friday prayer gathering as a community event variant gated by `schedule.PrayerGatheringAvailable`.

7. Apply background-specific flavor modifiers on top of the schedule.

### Phase 3 -- Application and Game layer

1. Add `DayScheduleModifiers` to relevant application query contexts (work menu, shop, crime preview).

2. Update HUD to display day-of-week name alongside date and time.

3. Show day-specific notes in UI (e.g., "Friday -- markets closed" in the location panel).

### Phase 4 -- Tests

1. **`tests/Slums.Core.Tests/Clock/DayScheduleTests.cs`**:
   - `GetDayOfWeek()` returns correct values for days 1-14
   - Friday blocks bakery, call-center, and cafe work
   - Saturday reduces food costs by 2 LE
   - Monday applies clinic discount
   - Prayer gathering only available on Friday
   - Background modifiers apply on top of schedule modifiers

2. **`tests/Slums.Application.Tests/`** -- verify day-of-week is surfaced in menu contexts.

### File Summary

New files:
- `src/Slums.Core/Clock/GameDayOfWeek.cs`
- `src/Slums.Core/Clock/DayScheduleModifiers.cs`
- `src/Slums.Core/Clock/DayScheduleRegistry.cs`
- `tests/Slums.Core.Tests/Clock/DayScheduleTests.cs`

Modified files:
- `src/Slums.Core/Clock/GameClock.cs` -- add `GetDayOfWeek()`
- `src/Slums.Core/State/GameSession.cs` -- add `GetCurrentSchedule()`, integrate into work/food/crime/investment
- `src/Slums.Game/Screens/` -- HUD day-of-week display

## Seasonal Calendar

Status: planned.

Requirements: see REQS.md "Seasonal Calendar" section.

### Design

Map Day 1 to October 1. Derive current date, season, and active holidays. Season modifiers affect daily stat decay, food prices, energy, and event pools. Holidays create temporary world-state shifts with unique events and economic pressure.

### Phase 1 -- Core types

Create `src/Slums.Core/Calendar/`:

1. **`Season.cs`** -- enum: `Autumn, Winter, Spring, Summer`.

2. **`GameCalendar.cs`** -- sealed record:
   ```
   GameCalendar(
       int GameDay,
       DateOnly StartDate, // October 1
       DateOnly CurrentDate,
       Season CurrentSeason,
       string? ActiveHoliday,
       bool IsRamadan)
   ```
   Static factory `FromGameDay(int day)` computes `CurrentDate` as `StartDate.AddDays(day - 1)`, derives `Season` from month, checks holidays.

3. **`SeasonModifiers.cs`** -- sealed record:
   ```
   SeasonModifiers(
       Season Season,
       int FoodCostModifier,
       int EnergyDrainModifier,
       int StressModifier,
       double IllnessEventFrequencyMultiplier,
       double InvestmentReturnMultiplier,
       bool MealQualityDegradesFaster)
   ```

4. **`SeasonRegistry.cs`** -- static class with per-season modifiers from REQS.md.

5. **`HolidayRegistry.cs`** -- static class with holiday definitions for the ~180-day playable span. Each holiday: name, start date, duration, effects record.

### Phase 2 -- Ramadan mechanic

Create `src/Slums.Core/Calendar/RamadanState.cs`:

1. **`RamadanState.cs`** -- sealed record:
   ```
   RamadanState(
       bool IsActive,
       bool PlayerIsFasting,
       int DaysFasting,
       int DaysRemaining)
   ```

2. Logic for fasting consequences: daily energy/stress/trust modifications applied in `EndDay()` during Ramadan.

3. Iftar gathering as a community event variant available only during Ramadan.

4. Discovery roll for non-fasting player: 30% per religious NPC per day in same district.

### Phase 3 -- Core integration on GameSession

1. Add `GameCalendar Calendar` property to `GameSession`, initialized with `GameCalendar.FromGameDay(1)`.

2. Add `SeasonModifiers GetCurrentSeasonModifiers()` using `Calendar.CurrentSeason`.

3. Modify `EndDay()`:
   - Apply season energy/stress modifiers
   - Apply season food cost modifiers (stored for next day's purchases)
   - Check for active holidays and apply effects
   - During Ramadan: apply fasting/consequences
   - Update `Calendar` for new day

4. Eid gift-spending pressure: during Eid al-Fitr, if money < 20, stress +5/day and trust -3/day with neighbors.

5. Background-specific seasonal modifiers applied on top.

### Phase 4 -- Application and Game layer

1. Add season and holiday info to status queries and HUD display.

2. Add Ramadan fasting choice dialog when Ramadan starts (Ink scene).

3. Add iftar gathering to community events menu during Ramadan.

4. Holiday events surfaced through existing event system.

### Phase 5 -- Tests

1. **`tests/Slums.Core.Tests/Calendar/`**:
   - `GameCalendar.FromGameDay(1)` returns October 1, Autumn
   - `GameCalendar.FromGameDay(92)` returns January 1, Winter
   - `GameCalendar.FromGameDay(183)` returns April 1, Spring
   - Season modifiers match REQS.md table
   - Holiday dates resolve correctly
   - Ramadan fasting applies correct energy/stress/trust deltas
   - Non-fasting discovery roll works
   - Eid gift pressure triggers when broke
   - Background modifiers apply on top

2. **`tests/Slums.Infrastructure.Tests/`** -- save/load round-trip includes `GameCalendar` and `RamadanState`.

### File Summary

New files:
- `src/Slums.Core/Calendar/Season.cs`
- `src/Slums.Core/Calendar/GameCalendar.cs`
- `src/Slums.Core/Calendar/SeasonModifiers.cs`
- `src/Slums.Core/Calendar/SeasonRegistry.cs`
- `src/Slums.Core/Calendar/HolidayRegistry.cs`
- `src/Slums.Core/Calendar/RamadanState.cs`
- `tests/Slums.Core.Tests/Calendar/CalendarTests.cs`
- `tests/Slums.Core.Tests/Calendar/RamadanTests.cs`

Modified files:
- `src/Slums.Core/State/GameSession.cs` -- add `Calendar`, integrate season/holiday/Ramadan into `EndDay()`
- `src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs` -- add calendar and Ramadan state
- `src/Slums.Game/Screens/` -- HUD season/holiday display, Ramadan choice dialog

## Weather and Atmospheric Conditions

Status: planned.

Requirements: see REQS.md "Weather and Atmospheric Conditions" section.

### Design

Each day rolls a weather state based on season. Weather affects energy, health, costs, crime, travel, and events. Weather is stored on `GameSession` and displayed on the HUD.

### Phase 1 -- Core types

Create `src/Slums.Core/Weather/`:

1. **`WeatherType.cs`** -- enum: `Clear, Hot, Heatwave, Khamsin, CoolOvercast, Rain, Windy`.

2. **`WeatherState.cs`** -- sealed record:
   ```
   WeatherState(
       WeatherType Type,
       int EnergyDrainModifier,
       int StressModifier,
       int FoodCostModifier,
       int CrimeDetectionModifier,
       int TravelCostModifier,
       bool BlocksOutdoorJobs,
       bool BlocksCrime,
       bool BlocksTravelToArdAlLiwa,
       bool BlocksTravelToDokki)
   ```

3. **`WeatherModifiers.cs`** -- static registry mapping `WeatherType` to `WeatherState` with the REQS.md values.

4. **`WeatherProbabilityTable.cs`** -- static class with per-season probability distributions. Returns `Dictionary<WeatherType, int>` (weights summing to 100) per season.

5. **`WeatherRoller.cs`** -- takes season + seeded random, returns `WeatherType`.

### Phase 2 -- Core integration on GameSession

1. Add `WeatherState CurrentWeather` property to `GameSession`.

2. In `EndDay()`, after advancing clock: roll new weather using `WeatherRoller.Roll(Calendar.CurrentSeason, _random)`.

3. Apply weather modifiers during the day:
   - `EnergyDrainModifier` applied during daily decay
   - `FoodCostModifier` applied when buying food
   - `CrimeDetectionModifier` applied during crime resolution
   - `TravelCostModifier` applied during travel
   - `BlocksOutdoorJobs` blocks outdoor job variants
   - `BlocksCrime` blocks all crime actions
   - `BlocksTravelToArdAlLiwa` / `BlocksTravelToDokki` block travel to those districts

4. Khamsin triggers a unique Ink scene (`weather_khamsin`).

5. Rain triggers a home leak event if player lacks curtain upgrade (repair cost 10-20 LE).

6. Background-specific weather modifiers applied on top.

### Phase 3 -- Application and Game layer

1. Add weather to status queries and HUD display (icon + name in the header).

2. Block relevant actions in menu queries based on weather state.

3. Travel screen shows weather warnings for destination districts.

4. Weather-gated random events added to `RandomEventRegistry`.

### Phase 4 -- Tests

1. **`tests/Slums.Core.Tests/Weather/WeatherTests.cs`**:
   - Weather roll produces valid types per season
   - Season probabilities match REQS.md tables
   - Khamsin blocks outdoor jobs and crime
   - Rain blocks travel to Ard Al-Liwa and Dokki
   - Heatwave applies energy drain +10 and food cost +5
   - Weather modifiers stack with season and day-of-week
   - Home leak event triggers on rain without curtain
   - Background modifiers apply (refugee khamsin stress, prisoner rain memory, dropout heatwave immunity)

2. **`tests/Slums.Infrastructure.Tests/`** -- weather persists through save/load.

### File Summary

New files:
- `src/Slums.Core/Weather/WeatherType.cs`
- `src/Slums.Core/Weather/WeatherState.cs`
- `src/Slums.Core/Weather/WeatherModifiers.cs`
- `src/Slums.Core/Weather/WeatherProbabilityTable.cs`
- `src/Slums.Core/Weather/WeatherRoller.cs`
- `tests/Slums.Core.Tests/Weather/WeatherTests.cs`

Modified files:
- `src/Slums.Core/State/GameSession.cs` -- add `CurrentWeather`, weather roll in `EndDay()`, apply weather modifiers
- `src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs` -- persist weather state
- `src/Slums.Game/Screens/` -- HUD weather display, travel warnings

## Sleep Quality

Status: planned.

Requirements: see REQS.md "Sleep Quality" section.

### Design

Replace flat +30 energy recovery from `RestAtHome()` with a calculated value based on conditions. Add home comfort upgrades. Add implicit overnight recovery to `EndDay()`.

### Phase 1 -- Core types

Create `src/Slums.Core/Home/`:

1. **`HomeUpgrade.cs`** -- enum: `CleanBedding, Fan, WindowScreen, Curtain`.

2. **`HomeUpgradeState.cs`** -- sealed record tracking a set of purchased upgrades with query methods:
   ```
   HomeUpgradeState(
       IReadOnlySet<HomeUpgrade> PurchasedUpgrades)
   ```
   Methods: `HasUpgrade(HomeUpgrade)`, `Purchase(HomeUpgrade)`, `GetEnergyRecoveryBonus(Season)`, `GetStressBonus()`, `GetPrivacyBonus()`.

3. **`HomeUpgradeDefinitions.cs`** -- static registry mapping `HomeUpgrade` to cost and effects.

4. **`SleepQualityCalculator.cs`** -- static class:
   ```
   int CalculateRecovery(SurvivalStats stats, NutritionState nutrition,
       HouseholdCareState household, RentState rent, WeatherState weather,
       HomeUpgradeState upgrades, Season season)
   ```
   Base 30, applies modifiers from REQS.md table, minimum 10.

### Phase 2 -- Core integration on GameSession

1. Add `HomeUpgradeState HomeUpgrades` property to `GameSession`.

2. Modify `RestAtHome()`: replace flat +30 with `SleepQualityCalculator.CalculateRecovery(...)`. Display recovery breakdown as event log message.

3. Add `TryPurchaseHomeUpgrade(HomeUpgrade)` method on `GameSession`:
   - Guard: at home, enough money, not already purchased
   - On success: deduct money, add to `HomeUpgrades`, event log

4. Add implicit overnight recovery to `EndDay()`:
   - Base +15 energy
   - Modified by same sleep quality factors (stress, hunger, mother health, rent, weather, upgrades)
   - Applied after daily decay and nutrition effects
   - Minimum 5 energy overnight

5. `WindowScreen` blocks insect-related random events.
6. `Curtain` reduces negative rumor spread probability by 20%.

### Phase 3 -- Application and Game layer

1. Add `HomeUpgradeMenuContext` and `HomeUpgradeMenuQuery` in `Slums.Application/Activities/`.

2. Add `HomeUpgradeCommand` for purchase delegation.

3. Add `HomeImprovement` to `GameActionId`, wired into action menu when at home.

4. Create `HomeUpgradeScreen` in `Slums.Game/Screens/` following the shop/investment screen pattern.

5. Show recovery breakdown in rest result message.

### Phase 4 -- Tests

1. **`tests/Slums.Core.Tests/Home/SleepQualityTests.cs`**:
   - Base recovery is 30 with no modifiers
   - Stress > 60 reduces by 5, stress > 80 reduces by 10
   - Did not eat today reduces by 5
   - DaysUndereating > 2 reduces by 5
   - Mother health in crisis reduces by 5
   - Rent unpaid > 3 days reduces by 3
   - Heatwave reduces by 3
   - Rain without curtain reduces by 3
   - Cool/overcast weather adds 3
   - Clean bedding adds 2
   - Fan adds 3 in summer, 1 otherwise
   - Window screen adds 1
   - Curtain adds 1
   - Multiple upgrades stack additively
   - Minimum recovery is 10
   - Overnight recovery base is 15 with minimum 5

2. **`tests/Slums.Core.Tests/Home/HomeUpgradeTests.cs`**:
   - Purchase succeeds with enough money at home
   - Purchase fails when not at home
   - Purchase fails with insufficient money
   - Purchase fails when already owned
   - Upgrades persist through save/load

3. **`tests/Slums.Application.Tests/`** -- upgrade menu query and command tests.

### File Summary

New files:
- `src/Slums.Core/Home/HomeUpgrade.cs`
- `src/Slums.Core/Home/HomeUpgradeState.cs`
- `src/Slums.Core/Home/HomeUpgradeDefinitions.cs`
- `src/Slums.Core/Home/SleepQualityCalculator.cs`
- `src/Slums.Application/Activities/HomeUpgradeMenuContext.cs`
- `src/Slums.Application/Activities/HomeUpgradeMenuQuery.cs`
- `src/Slums.Application/Activities/HomeUpgradeCommand.cs`
- `src/Slums.Game/Screens/HomeUpgradeScreen.cs`
- `tests/Slums.Core.Tests/Home/SleepQualityTests.cs`
- `tests/Slums.Core.Tests/Home/HomeUpgradeTests.cs`

Modified files:
- `src/Slums.Core/State/GameSession.cs` -- add `HomeUpgrades`, modify `RestAtHome()`, add overnight recovery to `EndDay()`, add `TryPurchaseHomeUpgrade()`
- `src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs` -- persist home upgrades
- `src/Slums.Application/Activities/GameActionId.cs` -- add `HomeImprovement`
- `src/Slums.Application/Activities/GameActionMenuContext.cs` -- add `HasHomeUpgradesAvailable`
- `src/Slums.Application/Activities/GameActionMenuQuery.cs` -- add HomeImprovement conditional
- `src/Slums.Game/Screens/GameScreen.cs` -- add dispatch for home upgrade screen

## Rumors and Word of Mouth

Status: planned.

Requirements: see REQS.md "Rumors and Word of Mouth" section.

### Design

Player actions generate rumors that spread through the community over 1-3 days, affecting NPC trust. Rumors are tracked on `GameSession`, propagated in `EndDay()`, and surfaced through NPC conversations and the event log.

### Phase 1 -- Core types

Create `src/Slums.Core/Rumors/`:

1. **`RumorId.cs`** -- enum or string-based identifier.

2. **`Rumor.cs`** -- sealed record:
   ```
   Rumor(
       string Id,
       string SourceAction,
       DistrictId District,
       int DayCreated,
       int Intensity,
       bool IsPositive,
       IReadOnlySet<NpcId> AffectedNpcs,
       int TrustModifier,
       IReadOnlySet<NpcId> NpcsWhoHeard)
   ```

3. **`RumorState.cs`** -- tracks active rumors:
   ```
   RumorState(IReadOnlyList<Rumor> ActiveRumors)
   ```
   Methods: `AddRumor(Rumor)`, `DecayAll()`, `RemoveExpired()`, `GetRumorsAffectingNpc(NpcId)`.

4. **`RumorGenerator.cs`** -- static class mapping gameplay events to rumor creation:
   - `OnCrimeSuccess(DistrictId, CrimeType)` -> rumor intensity 4-6
   - `OnCrimeDetected(DistrictId)` -> rumor intensity 7-9
   - `OnRentUnpaid(int daysUnpaid)` -> rumor intensity 3
   - `OnExpensivePurchase(int cost, DistrictId)` -> rumor intensity 5-7
   - `OnSeenWithCriminal(DistrictId)` -> rumor intensity 6
   - `OnClinicVisit(int totalVisits)` -> positive rumor intensity 3
   - `OnSkipCommunityEvents(int consecutiveSkips)` -> rumor intensity 4
   - `OnHighDistrictHeat(DistrictId, int heat)` -> rumor intensity 5
   - `OnCrimeEarningsThreshold(int totalEarnings)` -> rumor intensity 6

5. **`RumorPropagator.cs`** -- static class handling daily spread:
   - Day 1: affect co-located NPCs
   - Day 2: spread to socially connected NPCs (trust > 20 with day-1 NPC)
   - Day 3+: intensity drops to 1
   - Positive rumors decay 1 day faster
   - NPC disposition modifiers (trust > 30: -50% negative; trust < -10: +50% negative)

### Phase 2 -- Core integration on GameSession

1. Add `RumorState Rumors` property to `GameSession`.

2. In crime success/failure methods: call `RumorGenerator.OnCrimeSuccess/Detected()` and add to `Rumors`.

3. In `EndDay()`:
   - Check rent unpaid > 2 days -> generate rumor
   - Check crime earnings threshold -> generate rumor
   - Run `RumorPropagator.Propagate(Rumors, Relationships, Clock.Day)` to spread and decay
   - Apply trust modifications from newly heard rumors via `RelationshipService.ModifyTrust()`
   - Remove expired rumors
   - Generate event log entries for rumor effects

4. In expensive purchase logic: generate rumor if cost > 50 LE.

5. In community event skip tracking: generate rumor after 3+ consecutive skips.

6. Add `TryCounterRumor(Rumor rumor)` method:
   - Guard: in correct district, has 2 hours, not already countered
   - Uses Persuasion skill to reduce intensity
   - Background-specific bonus for Medical School Dropout

### Phase 3 -- Application and Game layer

1. Add rumor status to NPC conversation context so high-trust NPCs can warn about active rumors.

2. Add "Clear the Air" action to game action menu when active rumors exist and player is in the correct district.

3. Show rumor effects in event log with source and trust change.

4. Add conversation pool variants for NPC rumor warnings (e.g., "People are talking about you...").

### Phase 4 -- Tests

1. **`tests/Slums.Core.Tests/Rumors/RumorTests.cs`**:
   - Crime success generates rumor with correct intensity and district
   - Crime detection generates higher-intensity rumor
   - Rent unpaid generates rumor in home district
   - Rumor spreads to co-located NPCs on day 1
   - Rumor spreads to socially connected NPCs on day 2
   - Rumor intensity decays daily
   - Positive rumors decay faster
   - High-trust NPCs discount negative rumors by 50%
   - Low-trust NPCs amplify negative rumors by 50%
   - Rumors expire after intensity reaches 0
   - Clear the Air reduces intensity based on Persuasion skill
   - Background modifiers apply (refugee faster spread, prisoner higher intensity)

2. **`tests/Slums.Infrastructure.Tests/`** -- rumor state persists through save/load.

### File Summary

New files:
- `src/Slums.Core/Rumors/RumorId.cs`
- `src/Slums.Core/Rumors/Rumor.cs`
- `src/Slums.Core/Rumors/RumorState.cs`
- `src/Slums.Core/Rumors/RumorGenerator.cs`
- `src/Slums.Core/Rumors/RumorPropagator.cs`
- `src/Slums.Application/Activities/RumorMenuContext.cs`
- `src/Slums.Application/Activities/RumorCommand.cs`
- `tests/Slums.Core.Tests/Rumors/RumorTests.cs`

Modified files:
- `src/Slums.Core/State/GameSession.cs` -- add `Rumors`, integrate rumor generation into crime/work/purchase paths, rumor propagation in `EndDay()`
- `src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs` -- persist rumor state
- `src/Slums.Application/Activities/GameActionId.cs` -- add `ClearTheAir`
- `src/Slums.Game/Screens/GameScreen.cs` -- add rumor counter action dispatch

## Territory Dynamics

Status: planned.

Requirements: see REQS.md "Territory Dynamics" section.

### Design

Factions contest districts over time. Territory control affects NPC availability, crime access, prices, safety, and random events. The system builds on the existing `FactionId` and `RelationshipState` faction reputation model.

### Phase 1 -- Core types

Create `src/Slums.Core/Territory/`:

1. **`TerritoryControl.cs`** -- sealed record:
   ```
   TerritoryControl(
       DistrictId District,
       IReadOnlyDictionary<FactionId, int> FactionInfluence,
       int Tension,
       FactionId? ControllingFaction,
       int LastConflictDay)
   ```
   Computed: `ControllingFaction` is the faction with highest influence >= 50, or null.

2. **`TerritoryState.cs`** -- tracks territory for all districts:
   ```
   TerritoryState(IReadOnlyDictionary<DistrictId, TerritoryControl> Districts)
   ```
   Methods: `GetControl(DistrictId)`, `SetInfluence(DistrictId, FactionId, int)`, `ModifyInfluence(DistrictId, FactionId, int)`, `ModifyTension(DistrictId, int)`, `GetTensionLevel(DistrictId)`.

3. **`TensionLevel.cs`** -- enum: `Normal(0-30), Elevated(31-50), High(51-70), Dangerous(71-100)`.

4. **`TerritoryEventRegistry.cs`** -- defines territory-triggered events (street argument, protection demand, alliance shift, police crackdown, territory flip, refugee solidarity) with their conditions and effects.

5. **`TerritoryDynamicsCalculator.cs`** -- static class computing daily territory changes:
   - Natural tension decay: -2/day
   - Natural influence drift: +/-1 toward equilibrium
   - Crime impact: faction approval -> +3 influence; without approval -> +5 tension, -2 influence
   - Honest work: -1 tension per day worked
   - Police crackdown: tension -30, all faction influence -10

### Phase 2 -- Starting state and initialization

Initialize `TerritoryState` with the REQS.md starting values:
- Imbaba: ImbabaCrew 60, DokkiThugs 10, ExPrisonerNetwork 15, Tension 20
- Dokki: ImbabaCrew 5, DokkiThugs 50, ExPrisonerNetwork 10, Tension 30
- Ard Al-Liwa: ImbabaCrew 20, DokkiThugs 20, ExPrisonerNetwork 40, Tension 45
- Bulaq: ImbabaCrew 15, DokkiThugs 15, ExPrisonerNetwork 30, Tension 15
- Shubra: ImbabaCrew 10, DokkiThugs 10, ExPrisonerNetwork 20, Tension 10

Background modifiers:
- Released Political Prisoner: ExPrisonerNetwork +10 in all districts

### Phase 3 -- Core integration on GameSession

1. Add `TerritoryState Territory` property to `GameSession`.

2. In `CommitCrime()`: update territory influence/tension based on faction approval and crime location.

3. In `WorkJob()`: reduce tension by 1 in the work district.

4. In `EndDay()`:
   - Run `TerritoryDynamicsCalculator` for daily decay/drift
   - Check territory event triggers (tension thresholds, influence thresholds)
   - Apply territory event effects (market blocked, protection demand, etc.)
   - High tension districts: increase district heat +3/day
   - Territory flip: trigger narrative scene, update NPC availability

5. Modify crime availability queries to check territory control (non-controlling faction crime routes blocked in high-tension districts).

6. Modify NPC availability based on territory tension level.

7. Modify food prices based on territory tension (+3 LE at 51+, +5 LE at 71+).

8. Travel through dangerous districts (71+) may trigger crossfire event.

### Phase 4 -- Application and Game layer

1. Add territory status to map/status screen queries.

2. Show territory control and tension in location display.

3. Surface territory events through event log with clear explanation.

4. Block crime actions in menu when territory tension blocks them.

5. Add territory-related conversation variants for faction NPCs.

### Phase 5 -- Tests

1. **`tests/Slums.Core.Tests/Territory/TerritoryTests.cs`**:
   - Starting territory state matches REQS.md table
   - Crime with approval increases faction influence by 3
   - Crime without approval increases tension by 5, decreases influence by 2
   - Honest work decreases tension by 1
   - Tension decays by 2 per day naturally
   - Controlling faction computed correctly (highest influence >= 50)
   - Territory flip triggers when controlling faction changes
   - Elevated tension (31-50) triggers conflict events
   - High tension (51-70) increases food prices and blocks non-controlling crime
   - Dangerous tension (71-100) blocks all crime and increases heat
   - Police crackdown reduces tension and influence
   - Protection demand triggers when faction influence > 50 in home district
   - Background modifier: Released Political Prisoner starts with ExPrisonerNetwork +10

2. **`tests/Slums.Infrastructure.Tests/`** -- territory state persists through save/load.

### File Summary

New files:
- `src/Slums.Core/Territory/TerritoryControl.cs`
- `src/Slums.Core/Territory/TerritoryState.cs`
- `src/Slums.Core/Territory/TensionLevel.cs`
- `src/Slums.Core/Territory/TerritoryEventRegistry.cs`
- `src/Slums.Core/Territory/TerritoryDynamicsCalculator.cs`
- `tests/Slums.Core.Tests/Territory/TerritoryTests.cs`

Modified files:
- `src/Slums.Core/State/GameSession.cs` -- add `Territory`, integrate into crime/work/EndDay
- `src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs` -- persist territory state
- `src/Slums.Application/Activities/` -- territory-aware crime and NPC queries
- `src/Slums.Game/Screens/` -- territory display on map/status

## Debt and NPC Economy

Status: planned.

Requirements: see REQS.md "Debt and NPC Economy" section.

### Design

NPCs have individual financial states that shift weekly. They borrow from each other and from the player, and the player can borrow from NPCs and loan sharks. All debts are tracked through a unified model. Loan shark debts carry interest and escalation mechanics. Economic ripple effects connect NPC wealth to rent, prices, and stress.

### Phase 1 -- Core types

Create `src/Slums.Core/Economy/`:

1. **`NpcWealthLevel.cs`** -- enum: `Struggling, Poor, Stable, Comfortable`.

2. **`NpcEconomy.cs`** -- sealed record:
   ```
   NpcEconomy(
       NpcId Npc,
       NpcWealthLevel WealthLevel,
       int Generosity,
       IReadOnlyDictionary<DebtorId, int> MoneyOwedTo,
       IReadOnlyDictionary<DebtorId, int> MoneyOwedBy,
       int LastHardshipDay,
       int LastWindfallDay)
   ```
   `DebtorId` is either an `NpcId` or a `PlayerDebtor` sentinel, enabling the same model to track NPC-to-NPC and NPC-to-player debts.

3. **`NpcEconomyState.cs`** -- tracks economy for all NPCs:
   ```
   NpcEconomyState(IReadOnlyDictionary<NpcId, NpcEconomy> Economies)
   ```
   Methods: `GetEconomy(NpcId)`, `SetWealthLevel(NpcId, NpcWealthLevel)`, `AddDebt(DebtorId from, DebtorId to, int amount)`, `ResolveDebt(DebtorId from, DebtorId to)`, `GetStrugglingNpcs()`, `GetComfortableNpcs()`.

4. **`NpcEconomyDefinitions.cs`** -- static registry with starting wealth and generosity per NPC from REQS.md table.

5. **`NpcEconomyResolver.cs`** -- handles weekly economy resolution:
   - Roll wealth event per NPC (Hardship 20-35%, Windfall 10-20%, Stable 50-65%)
   - On Hardship: decrease wealth level, attempt NPC-to-NPC borrowing
   - On Windfall: increase wealth level, set generous flag for 3-5 days
   - NPC lending: find wealthiest trusted NPC (trust > 10), lend if generosity >= 4
   - Unpaid NPC-to-NPC debt after 14 days: trust -5 between them

6. **`PlayerDebt.cs`** -- sealed record tracking a single player debt:
   ```
   PlayerDebt(
       DebtSource Source,
       int AmountOwed,
       int InterestWeeklyBasisPoints,
       int DueDay,
       DebtCollectionState CollectionState)
   ```
   `DebtSource`: `NeighborLoan`, `LandlordAdvance`, `LoanShark`, `CommunityMutualAid`.

7. **`PlayerDebtState.cs`** -- tracks all active player debts:
   ```
   PlayerDebtState(IReadOnlyList<PlayerDebt> Debts)
   ```
   Methods: `AddDebt(PlayerDebt)`, `RepayPartial(DebtSource, int amount)`, `GetOverdueDebts(int currentDay)`, `ProcessInterest(int currentDay)`.

8. **`LoanSharkEscalation.cs`** -- handles loan shark debt escalation:
   - Days 1-7 overdue: stress +5/day
   - Days 8-14 overdue: health -5/day, stress +8/day
   - After 14 days overdue: trigger **DebtViolence** bad ending

### Phase 2 -- Core integration on GameSession

1. Add `NpcEconomyState NpcEconomies` and `PlayerDebtState PlayerDebts` properties to `GameSession`, initialized from `NpcEconomyDefinitions`.

2. In `EndDay()`, on Mondays (during existing weekly resolution):
   - Run `NpcEconomyResolver.ResolveWeek(NpcEconomies, Relationships, Clock.Day)`
   - Process NPC-to-NPC lending web
   - If NPC cannot borrow from any NPC, queue a loan request to the player
   - Apply ripple effects:
     - Hajj Mahmoud hardship: rent warning threshold drops from day 3 to day 2
     - Mona hardship: player stress +3
     - Hanan calling in debts: increased pressure on player criminal debt
     - Umm Karim comfortable: Imbaba food prices -1 LE for 3-5 days

3. In `EndDay()`, every day:
   - Process overdue debt checks via `PlayerDebtState.GetOverdueDebts(Clock.Day)`
   - Run loan shark escalation via `LoanSharkEscalation`
   - Apply interest on loan shark debts weekly
   - Emit event log entries for overdue debt warnings

4. Add `TryBorrowFromNpc(NpcId, int amount)` method on `GameSession`:
   - Guard: NPC trust meets threshold, NPC has sufficient wealth, within amount range
   - Neighbor loans (Mona, Umm Karim): 20-50 LE, no interest, 7-14 days, trust >= 10
   - Landlord advance (Hajj Mahmoud): 50-100 LE, adds to rent debt, trust >= 5
   - Creates entry in NPC's `MoneyOwedBy` and `PlayerDebtState`

5. Add `TryBorrowFromLoanShark(int amount)` method:
   - Guard: criminal contact access, amount 100-300 LE
   - Interest 20-30% per week, due in 7 days
   - Increases police pressure slightly

6. Add `TryLendToNpc(NpcId, int amount)` method on `GameSession`:
   - Guard: enough money, NPC has asked for loan
   - On success: deduct money, set `WasHelped`, increase trust +3 to +5
   - Track player-to-NPC loan for repayment

7. Add `RefuseNpcLoan(NpcId)` method:
   - Trust -2 to -5 based on NPC desperation

8. Add `RepayDebt(DebtSource, int amount)` method:
   - Partial repayment allowed; minimum payment prevents escalation

9. NPC economy state affects conversation variants (struggling NPCs reflect stress, comfortable NPCs share deals).

10. Background modifiers:
    - Medical School Dropout: Persuasion skill reduces loan shark interest rates
    - Released Political Prisoner: loan shark cap at 200 LE; Hajj Mahmoud trust +2 on refusing his loan
    - Sudanese Refugee: community mutual-aid loan (30-60 LE, no interest, 14 days) via refugee contacts

### Phase 3 -- Application and Game layer

1. Add NPC loan request to phone/message system or conversation variants.

2. Add "Lend Money" action when an NPC has requested a loan.

3. Add "Borrow Money" action showing available debt sources with terms.

4. Add "Repay Debt" action showing active debts with amounts and due dates.

5. Add NPC financial gossip to conversation pools for high-trust NPCs (trust >= 15).

6. Show NPC financial state hints in talk context (e.g., "Mona looks worried about money").

7. NPC loan repayment events surface through event log.

8. Debt status page shows all active debts, due dates, and escalation state.

### Phase 4 -- Tests

1. **`tests/Slums.Core.Tests/Economy/NpcEconomyTests.cs`**:
   - Starting wealth matches REQS.md table
   - Weekly hardship roll decreases wealth level
   - Weekly windfall roll increases wealth level
   - NPC borrows from wealthiest trusted contact
   - Lending requires generosity >= 4
   - Unpaid debt after 14 days reduces trust by 5
   - Player lending increases trust by 3-5
   - Player refusing decreases trust by 2-5
   - Hajj Mahmoud hardship makes rent collection aggressive
   - Mona hardship increases player stress
   - Umm Karim comfortable reduces Imbaba food prices
   - Background modifiers apply (refugee community sharing, prisoner financial independence, dropout medical discounts)

2. **`tests/Slums.Core.Tests/Economy/PlayerDebtTests.cs`**:
   - Borrow from neighbor creates debt with correct terms
   - Borrow from landlord adds to rent debt
   - Borrow from loan shark creates debt with interest and escalation
   - Partial repayment reduces amount owed
   - Overdue debt triggers escalation events
   - Loan shark debt triggers DebtViolence ending after 14 days overdue
   - Loan shark borrowing increases police pressure
   - Background modifiers (refugee mutual-aid, prisoner cap, dropout negotiation)

3. **`tests/Slums.Infrastructure.Tests/`** -- NPC economy state and player debt state persist through save/load.

### File Summary

New files:
- `src/Slums.Core/Economy/NpcWealthLevel.cs`
- `src/Slums.Core/Economy/NpcEconomy.cs`
- `src/Slums.Core/Economy/NpcEconomyState.cs`
- `src/Slums.Core/Economy/NpcEconomyDefinitions.cs`
- `src/Slums.Core/Economy/NpcEconomyResolver.cs`
- `src/Slums.Core/Economy/PlayerDebt.cs`
- `src/Slums.Core/Economy/PlayerDebtState.cs`
- `src/Slums.Core/Economy/LoanSharkEscalation.cs`
- `src/Slums.Application/Activities/NpcLoanCommand.cs`
- `src/Slums.Application/Activities/BorrowCommand.cs`
- `src/Slums.Application/Activities/RepayDebtCommand.cs`
- `tests/Slums.Core.Tests/Economy/NpcEconomyTests.cs`
- `tests/Slums.Core.Tests/Economy/PlayerDebtTests.cs`

Modified files:
- `src/Slums.Core/State/GameSession.cs` -- add `NpcEconomies`, `PlayerDebts`, weekly resolution, lending methods, borrowing methods, ripple effects, loan shark escalation
- `src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs` -- persist NPC economy state and player debt state
- `src/Slums.Game/Screens/` -- NPC loan dialog, borrow dialog, debt status page, financial gossip in conversations

## Suggested Next Ticket

After Active Skill Training, implement the world-enrichment mechanics in dependency order:

1. **Day-of-Week Rhythms** (quick win, no dependencies)
2. **Sleep Quality** (quick win, standalone)
3. **Seasonal Calendar** (depends on day-of-week pattern)
4. **Weather** (depends on seasonal calendar)
5. **Rumors** (depends on relationship system)
6. **Territory Dynamics** (depends on factions, district heat)
7. **Debt and NPC Economy** (depends on relationship system)

## Narrative Enhancement Pass

Status: implemented.

Completed steps:

1. Expanded recurring NPC reactivity for Hajj Mahmoud, Umm Karim, Mona, and Nurse Salma using trust, debt, police heat, and double-life state.
2. Added queued spillover narrative scenes for public-facing work under heat, criminal money reaching home, and neighborhood protection when Mona trusts you.
3. Expanded ending epilogues so stability and leaving-crime endings react to background, and network-shelter endings react to the strongest support contact.

## Small Business Investments

Status: implemented.

Completed steps:

1. Added `InvestmentType`, `InvestmentDefinition`, `Investment`, risk, eligibility, and resolution models in `Slums.Core`.
2. Added active-investment state plus weekly resolution in `GameSession`, including suspension, extortion, police-pressure, betrayal, and income handling.
3. Added location/contact-gated investment opportunities so investments are offered through reachable NPCs in the right districts.
4. Persisted active investments and total investment earnings through `GameSessionSnapshot` and JSON save/load.
5. Added application-layer investment menu query/command flow.
6. Added `InvestmentMenuScreen` and wired it into the main gameplay action list.
7. Added an investments status page so the player can review active ventures and lifetime earnings.
8. Added core, application, and infrastructure tests covering eligibility, weekly resolution, menu exposure, and save/load round-tripping.

### Risk Balance Targets

| Business | Failure | Extortion | Police | Betrayal |
|----------|---------|-----------|--------|----------|
| Foul cart | 1% | 0% | 0% | 2% |
| Micro-laundry | 2% | 0% | 0% | 3% |
| Scrap collection | 4% | 3% | 2% | 5% |
| Kiosk share | 3% | 4% | 3% | 4% |
| Market stall | 5% | 6% | 5% | 5% |
| Hashish courier | 8% | 10% | 12% | 8% |

## Validation

When relevant to the change:

1. Run `npm run compile-ink` from `src/Slums.Game` after editing Ink files.
2. Run `dotnet build Slums.slnx` from the repo root.
3. Run `dotnet run --project tests/Slums.Core.Tests` from the repo root.
4. Run `dotnet run --project tests/Slums.Application.Tests` from the repo root.
5. Run `dotnet run --project tests/Slums.Game.Tests` from the repo root.
6. Run `dotnet run --project tests/Slums.Infrastructure.Tests` from the repo root.
7. Run `dotnet run --project tests/Slums.Narrative.Ink.Tests` from the repo root.

Every rules change requires tests.

## Update Rules

- Keep this file focused on current state, guardrails, and near-term priorities.
- Do not add back long historical phase breakdowns or obsolete gap lists.
- Update `AGENTS.md` when architecture or workflow expectations change.
- Update `README.md` if the description of planning documents changes.
