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

### 1. UI Clarity

Make the deeper systems legible to the player.

- explain why work variants improve, worsen, or lock out
- explain why crime routes are available, blocked, or unusually risky
- surface debt, suspicion, relationship-memory, and district-pressure signals in menus or logs
- make investment risk, suspension, and extortion outcomes easier to read from the UI

### 2. Balance Pass

Tune the expanded simulation without flattening background identity.

- review pay, stress, pressure, reliability, and lockout values
- review entertainment costs and reductions against survival pressure
- review investment returns and disruption rates against the economy
- review district event frequency and recurrence
- add regression tests around balance-sensitive thresholds

### 3. More Authored Reactivity

Expand content that reflects the systems already in place.

- add more memory-reactive NPC variants
- add more spillover follow-up scenes
- add more district-event and background-specific aftermath content
- expand conversation pools beyond 100 days if needed

### 4. Content Breadth

Broaden the world through additional authored and systemic slices instead of more architecture churn.

- add more location-specific jobs, crime routes, and relationship hooks
- expand recurring-event variety so long runs feel less cyclical
- add more endings or epilogue variants only where they reflect existing systems cleanly
- prefer content and UX improvements over another repo-wide refactor unless a new hotspot clearly appears

### 5. Tracing and Diagnostics

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

## Suggested Next Ticket

After tracing is in place, return to UI clarity around deeper systems:

- explain why work variants improve, worsen, or lock out
- surface debt, suspicion, relationship-memory, and district-pressure signals
- explain investment disruption risk and recovery in a more player-readable way

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
