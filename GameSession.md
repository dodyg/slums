# GameSession Refactor Plan

Working plan to shrink `src/Slums.Core/State/GameSession.cs` (currently ~4,300 lines, ~100 public
methods, ~15 gameplay domains) into a thin state owner and integration surface, per the guardrails
already in `PLAN.MD` ("continue extracting cohesive daily-resolution and action-policy rules from
`GameSession`") and `AGENTS.md` ("Keep `GameSession` as the canonical runtime boundary").

Any agent picking up work from this document must first read `AGENTS.md`, `PLAN.MD`, and
`MEMORY.MD`. This plan does not change architecture; it applies the extraction pattern the codebase
already uses.

---

## 1. Problem

`GameSession` owns state (correct) but also contains the *rules* for food, medicine, clinic visits,
travel, home upgrades, pets/plants/robots, entertainment, training, community events, debt and
loans, investments, phone, tips, crime, work glue, calendar, district conditions, and endings.
Rules inline in the session are hard to test in isolation, produce merge conflicts between agents,
and blur the Core/Application boundary.

## 2. Non-Negotiable Constraints

These come from `AGENTS.md` and `PLAN.MD`. An extraction that violates any of them is rejected.

1. **`GameSession` stays the canonical runtime state owner and integration boundary.** We are not
   dissolving it; we are moving rules out.
2. **No parallel state models.** Extracted collaborators receive the existing state objects
   (`PlayerCharacter`, `GameCrimeState`, `RelationshipState`, ...). Never clone or mirror state.
3. **No public API churn.** Every `GameSession` public method keeps its exact signature and
   semantics; the body becomes a delegation. `Slums.Application` and `Slums.Game` must not need
   edits for behavior reasons (only compile-safe moves are allowed to touch nothing at all).
4. **Observable behavior is frozen.** With the same seed, a run must produce the identical:
   - sequence of `SharedRandom` draws (RNG state is persisted — draw order inside a method and
     between pipeline steps must not change),
   - `EventJournal` messages and ordering,
   - `RecordMutation` entries,
   - final stats and ending outcomes.
5. **Persistence stays centered on snapshots.** `RestoreFromSnapshot` remains the single public
   hydration boundary. `Restore*` mutators stay `internal`. Snapshot JSON shape does not change.
6. **Queries stay side-effect free; commands keep their own guards** even if the UI also filters.
7. **No SadConsole, Ink, or file IO in `Slums.Core`.** Collaborators live in `Slums.Core` feature
   folders and stay reachable only through the session (or as pure services).
8. **Coding standards:** one type per file, file-scoped namespaces, explicit access modifiers,
   `_camelCase` private fields, braces everywhere, nullable enabled, warnings as errors, XML docs
   on public APIs that matter.

## 3. The Two Approved Extraction Patterns

Use exactly these two shapes. They are already established in the codebase.

### Pattern A — Stateless domain service (pure rules)

For rules that need no event journaling or mutation recording. Receives state as parameters,
returns a result record.

Precedent: `Slums.Core/Jobs/JobService.cs` (takes `PlayerCharacter`, `RelationshipState`,
`JobProgressState`; returns `JobPreview`/`JobResult`), `InvestmentResolutionCalculator`.

```csharp
public sealed class FoodShopService
{
    public int GetFoodCost(Location currentLocation, PlayerCharacter player) { ... }
    public FoodPurchaseResult BuyFood(PlayerCharacter player, Location currentLocation) { ... }
}
```

The session holds one `readonly` instance and delegates:

```csharp
public bool BuyFood()
{
    var before = CaptureStats();
    var result = _foodShop.BuyFood(Player, World.GetCurrentLocation());
    if (!result.Success) { RecordMutation(...); RaiseEvent(result.FailureMessage); return false; }
    RaiseEvent(result.SuccessMessage);
    RecordMutation(MutationCategories.Food, "BuyFood", before, CaptureStats(), result.Summary);
    return true;
}
```

The session keeps `CaptureStats` / `RaiseEvent` / `RecordMutation` calls (they are the journaling
boundary). The service returns messages/results; the session journals them. Do not pass the
session into a Pattern A service.

### Pattern B — Internal static collaborator (session-integrated rules)

For rules that need journaling, event emission, queued scenes, or multi-subsystem mutation.
Lives next to its feature folder, takes the `GameSession` explicitly, accesses `internal` helpers
(`RaiseEvent`, `RecordMutation`, internal setters).

Precedent: `EndOfDayPipeline`, `State/DailyResolution/*`, `Characters/MotherCareResolution`,
`Characters/HouseholdAssetsWeeklyResolution`, `Narrative/CrimeNarrativePlanner`.

```csharp
internal static class PhoneDailyResolution
{
    internal static void ProcessDailyPhone(GameSession session, Random random) { ... }
}
```

### Diagnostics conventions (both patterns)

- Every state-mutating entry point keeps a `CaptureStats()` before/after pair and a
  `RecordMutation(category, action, before, after, reason)` call with the existing category and
  action strings — verbatim. The action string is behavior (diagnostics and possibly tests read it).
- Every player-visible outcome keeps its exact `RaiseEvent` message text. Message text is
  observable behavior; do not "improve" wording while refactoring.

## 4. Verification Protocol (run after every extraction)

From the repo root:

```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

Additional gates per extraction:

- **Golden run digest** (see Wave 0): the pinned 35-day seeded digest must be byte-identical
  before and after. If it changes, the extraction changed behavior — fix or revert.
- **Snapshot round trip**: `RestoreBoundaryTests`, `JsonSaveGameStoreTests`, and
  `GameSessionSnapshot` tests must pass unmodified.
- **Line-count delta**: `wc -l src/Slums.Core/State/GameSession.cs` must drop by roughly the size
  of the moved logic. Record the number in the status table.

## 5. Waves

Work strictly wave by wave. Within a wave, domains are independent and different agents can take
different domains, but **one domain per commit** and one domain per agent session. Do not start a
later wave while an earlier wave is incomplete.

### Wave 0 — Safety net and mechanical split (do first, once)

**W0.1 Golden-run digest test.** Before moving anything, add
`tests/Slums.Core.Tests/State/GameSessionGoldenRunTests.cs`:

- Construct `new GameSession(new GameRandom(fixedSeed))`, run a scripted 35-day playthrough that
  exercises every domain (work several shifts, commit crimes, borrow, repay, buy food/medicine,
  clinic, travel, adopt cat, buy plant/robot, phone, tips, community event, training,
  entertainment, investment, end each day via `EndDay`).
- Capture a digest: concatenated event-journal messages + mutation count + final
  stats/inventories + `RandomState`. Assert against a checked-in expected value.
- If a digest legitimately needs to change later (it should not), that requires a written
  justification in the PR/commit — this test is the behavior freeze.

**W0.2 Public API surface pin.** Add a test that reflects over `GameSession` public members and
compares them to a checked-in approved list. Prevents accidental signature drift during waves.

**W0.3 Partial-class split (mechanical, zero behavior change).** Split the single file into
partial class files by concern so later extractions have clean seams and fewer merge conflicts:

- `State/GameSession.cs` — fields, constructor, properties, snapshot boundary
- `State/GameSession.Snapshot.cs` — all `Restore*` / `Set*` internal mutators
- `State/GameSession.Actions.cs` — player-action methods
- `State/GameSession.Resolvers.cs` — internal resolution/roll methods
- `State/GameSession.Queries.cs` — pure queries
- `State/GameSession.Diagnostics.cs` — `CaptureStats`, `RecordMutation`, `RaiseEvent*`

Rules: move code verbatim, no renames, no visibility changes. Build + all suites must pass and
the golden digest must be untouched.

### Wave 1 — Self-contained commerce and actions (lowest risk)

Each domain below is independent. Suggested targets (adjust folder if a better feature folder
exists; keep one type per file):

| Domain | Move these members from `GameSession` | Target (Pattern) |
|---|---|---|
| Food/medicine shop | `BuyFood`, `BuyMedicine`, `GetFoodCost`, `GetStreetFoodCost`, `GetMedicineCost`, `GetUmmKarimFoodDiscount` | `Expenses/FoodShopService.cs` (A) |
| Entertainment | `GetAvailableEntertainmentActivities`, `TryPerformEntertainment`, `GetEntertainmentFlavorMessage` | `Entertainment/EntertainmentService.cs` (A) |
| Training | `GetAvailableTrainingActivities`, `TryPerformTraining`, `GetTrainingFlavorMessage`, `ClearDailyTraining`, `_trainedSkillsToday` + `RestoreTrainedSkillsToday` state accessors | `Training/TrainingService.cs` (A) |
| Household assets | `CanUseHouseholdAssets`, `AdoptStreetCat`, `BuyFishTank`, `BuyPlant`, `BuyRobot`, `BuyRobotParts`, `RepairRobot`, `PayPetCare`, `PayPlantCare`, `UpgradePlant`, `UpgradeFishTank`, `ResolveWeeklyHouseholdAssets`, `TryRollStreetCatEncounter`, `RestoreHouseholdAssetsState` | extend `Characters/HouseholdAssetsWeeklyResolution` into `Characters/HouseholdAssetsService.cs` (A/B hybrid) |
| Home upgrades / rest | `RestAtHome`, `GetAvailableHomeUpgrades`, `TryPurchaseHomeUpgrade`, `RestoreHomeUpgrades` | `Home/HomeUpgradeService.cs` (A) |

Notes:

- `BuyFood`/`BuyMedicine` currently encode background bonuses (e.g. SudaneseRefugee staples) and
  skill gains inline. Move them verbatim; the service takes `PlayerCharacter` and the location so
  the rules stay pure.
- Training owns per-day trained-skill tracking; keep the dictionary in its existing state object,
  not inside the service.

### Wave 2 — Movement and care

| Domain | Move these members | Target (Pattern) |
|---|---|---|
| Travel | `TryTravelTo`, `TryWalkTo`, `CanAffordTravel`, both `GetTravelCost`, both `GetTravelTimeMinutes`, `GetWalkTimeMinutes`, `GetWalkEnergyCost`, `GetTravelEnergyCost`, `GetTravelConditionSummary`, `ApplyCargoMuleWear` | `World/TravelService.cs` (A) |
| Clinic & mother care | `CheckOnMother`, `GiveMotherMedicine`, `TakeMotherToClinic`, `TravelAndTakeMotherToClinic`, `GetCurrentLocationClinicStatus`, `GetClinicLocations`, `GetClinicTravelOption`, `GetClinicVisitCost`, `GetMotherStatusMessage`, `FormatOpenDays` | `Characters/ClinicVisitService.cs` (A); compose with existing `MotherCareResolution` |
| Community events | `GetAvailableCommunityEvents`, `AttendCommunityEvent`, `ApplyCommunityEventTrust`, `ApplyBackgroundEventBonus`, `RequestEmergencySupport` | `Community/CommunityEventService.cs` (A/B hybrid — attendance rolls trust and queues scenes) |

Notes:

- Travel pricing must keep using `LocationPricingService`; do not duplicate its tables.
- `TravelAndTakeMotherToClinic` composes travel + clinic; build it from the two extracted pieces,
  preserving exact event message order and RNG draw order.

### Wave 3 — Money flows

| Domain | Move these members | Target (Pattern) |
|---|---|---|
| Debt & loans | `ApplyDebtPayment`, `ExtendDebtDueDate`, `TryBorrowFromNpc`, `TryBorrowFromLandlord`, `TryBorrowFromLoanShark`, `TryLendToNpc`, `RefuseNpcLoan`, `RepayDebt`, `ApplyRentPayment`, `GrantRentGraceDays`, `RestoreEconomyState` | `Economy/DebtAndLoanService.cs` (A/B hybrid) |
| Investments | `GetCurrentInvestmentOpportunities`, `GetAvailableInvestments`, `CheckInvestmentEligibility`, `MakeInvestment`, `ResolveWeeklyInvestments`, `CreateInvestmentEligibilityContext`, `RestoreInvestmentState` | consolidate around existing `InvestmentEligibilityEvaluator` + `InvestmentResolutionCalculator`; add `Investments/InvestmentPurchaseService.cs` (A) |
| Phone | `ProcessDailyPhone`, `RefillPhoneCredit`, `RespondToMessage`, `ApplyMessageResponseEffects`, `IgnoreMessage`, `ReplacePhone`, `RestorePhoneState`, `RestorePhoneMessages` | `Phone/PhoneService.cs` (B for daily processing, A for commands) |
| Tips | `ProcessDailyTips`, `ApplyTipIgnoreErosion`, `AcknowledgeTip`, `IgnoreTipAction`, `RestoreTips` | `Information/TipService.cs` (A/B hybrid); reuse `TipGenerator` |

Notes:

- Loans mutate `RelationshipState`, `PlayerDebtState`, `NpcEconomyState`, and journal events —
  Pattern B with the session passed in is acceptable; keep the tuple return shapes
  `(bool, int, string)` unless you introduce result records *and* keep session signatures
  unchanged.
- Phone message response effects queue narrative scenes and trust changes; preserve queue order.

### Wave 4 — Work, crime, pressure (highest risk — only after Waves 0-3 are merged)

| Domain | Move these members | Target (Pattern) |
|---|---|---|
| Work session glue | `WorkJob`, `GetAvailableJobs`, `PreviewJob`, `ModifyEmployerTrust`, `ApplyWorkCrimeSpillover`, `ApplyBackgroundWorkFlavor`, `ApplyDistrictConditionToJobPreview`, `ApplyDistrictConditionToJob`, `ApplyDayScheduleToJob`, `CloneJobShift`, `BuildWorkDistrictModifierText`, `GetSkillForJob`, `RestoreJobTrack` | `Jobs/WorkSessionService.cs` (B); core math already lives in `JobService` — do not duplicate it |
| Crime | `GetAvailableCrimes`, `GetCrimeBlockReason`, `CommitCrime`, `PreviewCrime`, `GetFactionForCurrentCrimeRoute`, `EvaluateCrimeModifiers`, `ApplyCrimeModifierSideEffects`, `ApplyCrimeContactAftermath`, `ReduceCrimeHeat`, `ApplyCrimeFailureMitigation`, `SetCrimeCounters`, `RestoreCrimeState`, `AdjustPolicePressure`, `SetPolicePressure` | `Crimes/CrimeSessionService.cs` (B); compose with existing `CrimeService`, `CrimeRegistry`, `CrimeNarrativePlanner` |
| Random events | `ApplyRandomEvent`, `GetEffectiveRandomEventWeight` | extend `Events/RandomEventService.cs` (B) |

Notes:

- This wave has the most RNG surface (crime success rolls, event rolls) and the most narrative
  scene queueing. The golden-run digest and `EndOfDayDeterminismTests` are the arbiters.
- District-condition modifiers for jobs/crimes live in the work/crime collaborators after this
  wave; the condition *definitions* stay in `DistrictConditionRegistry`.

### Wave 5 — Calendar, conditions, endings glue

| Domain | Move these members | Target (Pattern) |
|---|---|---|
| Calendar/season/schedule | `CurrentWeek`, `GetCurrentDayOfWeek`, `GetCurrentSchedule`, `GetCurrentSeason`, `GetCurrentSeasonModifiers`, `GetActiveHolidayState`, `SetRamadanFasting`, `RestoreRamadanState`, `RestoreWeather` | `Calendar/CalendarService.cs` (A — queries are pure) |
| District condition rolls | `RollDistrictConditionsForCurrentDay`, `SetBaselineDistrictConditions`, `SelectWeightedDistrictCondition`, `GetDailyDistrictConditions`, `GetActiveDistrictConditionDefinition` | `World/DistrictConditionRoller.cs` (B); registry keeps definitions |
| Territory | `RollTerritoryEvents` | `Territory/TerritoryEventRoller.cs` (B) |
| Endings & game over | `GetAvailableEndingChoices`, `TryChooseEnding`, `CommitEnding`, `TryTakePendingEndingKnot`, `CheckGameOverConditions` | `Endings/EndingCommitmentService.cs` (B); `EndingService` stays authoritative for thresholds |

Notes:

- Two-stage endings are sensitive: pending commitment, final-knot application, and automatic
  failure endings must keep their exact sequencing (see `MEMORY.MD` and `PLAN.MD`).
- After Wave 5, re-run the full acceptance in `PLAN.MD` §1 (seeded playthroughs) if available.

## 6. What Stays in `GameSession` Permanently

Do **not** extract these; they are the integration surface itself:

- State properties (`Clock`, `Player`, `World`, `Relationships`, all subsystem states)
- `RestoreFromSnapshot` + `ValidateRestoredState` (single hydration boundary)
- `CaptureStats`, `RecordMutation`, `RaiseEvent`, `RestoreEventJournal` (journaling boundary used
  internally by Pattern B collaborators)
- Thin stat passthroughs (`AdjustHealth`, `AdjustEnergy`, `AdjustHunger`, `AdjustStress`,
  `AdjustMotherHealth`, `AdjustFoodStockpile`, `AdjustMoney`, `AddEventMessage`)
- `ApplyNarrativeOutcome` (canonical narrative mutation boundary per `AGENTS.md`)
- `AdvanceTime`, `EndDay` (delegating to `EndOfDayPipeline`), `IsGameOver`/`EndingId` accessors
- One-line delegations to every extracted service (that is the end state)

## 7. Per-Extraction SOP (follow every time)

1. **Claim** the domain: set the row to `In progress` in §9 and commit that doc edit first.
2. **Read** the current implementation, every test that touches it
   (`grep -rn "<MethodName>" tests/ src/Slums.Application src/Slums.Game`), and the snapshot
   fields it persists.
3. **Choose the pattern** (A or B) per §3. If the method calls `RaiseEvent`/`RecordMutation` more
   than incidentally or mutates multiple subsystems, use B.
4. **Create** the new file in the feature folder. Move logic verbatim: same math, same guard
   order, same RNG draw order, same message strings, same mutation categories/action strings.
5. **Delegate**: replace the session body with a one-liner (Pattern A) or a static call (B).
   Public signature must not change.
6. **Test**: add TUnit tests for the extracted type in the matching `tests/Slums.*.Tests`
   project (FluentAssertions; NSubstitute where collaborators exist). Existing tests must pass
   *unmodified*.
7. **Verify**: full protocol from §4, including the golden digest. Warnings are errors.
8. **Record**: update the status row (with new `GameSession.cs` line count) and, if you
   introduced a new named collaborator, add one line to `MEMORY.MD`'s collaborator list.
9. **Commit**: one domain per commit; message style `Extract <domain> from GameSession into <Type>`.

If an extraction turns out to entangle two domains irreversibly, stop, leave the code working,
and record the finding in §9 instead of forcing a bad seam.

## 8. Done Criteria (whole plan)

- `GameSession.cs` is at or under ~1,200 lines: state, properties, delegations, journaling
  helpers, snapshot boundary. No domain rule bodies remain inline.
- Public API surface pin unchanged since Wave 0.
- Golden-run digest unchanged since Wave 0.
- All five test suites green; new unit tests exist for every extracted type.
- `PLAN.MD` "Architecture hardening" item on extracting from `GameSession` is updated;
  `MEMORY.MD` lists the new collaborators.
- No behavior changes reported by any test, and no edits were required in `Slums.Game`.

## 9. Status Table

Agents: keep this table current. Update the `GameSession.cs` line count after each merge.

| ID | Wave | Domain | Status | Notes |
|---|---|---|---|---|
| W0.1 | 0 | Golden-run digest test | Done | SHA-256 digest pinned in Core safety-net tests |
| W0.2 | 0 | Public API surface pin | Done | Reflection pin covers declared public fields, properties, methods, and events |
| W0.3 | 0 | Partial-class file split | In progress | Diagnostics boundary moved verbatim; remaining seams continue in extraction commits |
| W1.1 | 1 | Food/medicine shop | Done | `FoodShopService` owns cost modifiers and food/medicine purchases; GameSession.cs = 3,935 lines |
| W1.2 | 1 | Entertainment | Done | `EntertainmentService` owns availability, guards, effects, messages, and journal mutations; GameSession.cs = 4,176 lines |
| W1.3 | 1 | Training | Done | `TrainingService` owns availability, guards, effects, and flavor; daily tracker remains session-owned; GameSession.cs = 3,999 lines |
| W1.4 | 1 | Household assets (pets/plants/robots) | Done | `HouseholdAssetsService` owns household asset actions, restoration, street-cat encounters, and weekly neglect resolution; GameSession.cs = 3,576 lines |
| W1.5 | 1 | Home upgrades / rest | Done | `HomeUpgradeService` owns purchase, restore, availability, and rest policy; GameSession.cs = 4,123 lines |
| W2.1 | 2 | Travel | Done | `TravelService` owns paid/walking travel, condition summaries, pricing composition, and Cargo Mule wear; GameSession.cs = 3,366 lines |
| W2.2 | 2 | Clinic & mother care | Done | `ClinicVisitService` owns mother care, clinic pricing/status, and travel-plus-clinic composition; GameSession.cs = 3,113 lines |
| W2.3 | 2 | Community events & emergency support | Done | `CommunityEventService` owns availability, attendance rolls, background trust effects, and emergency support; GameSession.cs = 2,929 lines |
| W3.1 | 3 | Debt & loans | Done | `DebtAndLoanService` owns debt payments, borrowing/lending, rent support, and economy restoration; GameSession.cs = 2,767 lines |
| W3.2 | 3 | Investments | In progress | Claim: extract investment queries, purchase, weekly resolution, and restoration into `InvestmentPurchaseService`; reuse existing evaluators/calculators and preserve risk rolls. |
| W3.3 | 3 | Phone | Not started | |
| W3.4 | 3 | Tips | Not started | |
| W4.1 | 4 | Work session glue | Not started | Highest RNG surface |
| W4.2 | 4 | Crime | Not started | Compose with `CrimeService`/planners |
| W4.3 | 4 | Random events | Not started | |
| W5.1 | 5 | Calendar/season/schedule | Not started | |
| W5.2 | 5 | District condition rolls | Not started | |
| W5.3 | 5 | Territory events | Not started | |
| W5.4 | 5 | Endings & game over glue | Not started | Two-stage endings sensitive |

Baseline: `GameSession.cs` = 4,292 lines at time of writing (2026-09-04).
