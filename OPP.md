# OPP — Improvement Opportunities

This file records engineering-quality work identified during the September 2026 codebase review. It complements `PLAN.MD`: `PLAN.MD` owns product and roadmap priorities; this file owns accumulated technical debt. Read `AGENTS.md`, `PLAN.MD`, and `MEMORY.MD` before acting on any item. Work top-down, one item per change set, with tests. Strike an item when it is fixed and append new findings here instead of leaving them in review notes.

## 1. Remove shadow default catalogs in Core registries

`JobRegistry` (`Default*` shift bodies plus `GetJobByType(...) ?? DefaultX` accessors), `WorldState.DefaultLocations`, `BackgroundRegistry` (`Default*` backgrounds), and `RandomEventRegistry.DefaultEvents` hardcode full copies of the `content/data/*.json` catalogs (~450 lines total). `GameSession` falls back to them through `GameContentCatalog.FromLegacyRegistries()` when no catalog is passed.

Consequence: editing `content/data/` changes the game, while Core tests keep validating the stale code copies. This contradicts the repo's fail-fast content policy ("no silent fallback").

Fix direction:

- make each registry empty until `Configure(...)` runs and throw a clear error when queried unconfigured
- keep `GameContentCatalog` as the only content path; tests should configure content explicitly or share one fixture that does
- note that hundreds of Core tests construct `new GameSession()` and implicitly rely on defaults, so this is a deliberate, staged refactor

Acceptance: no `Default*` catalog bodies remain in Core; an unconfigured session fails fast; all five suites pass.

## 2. Restore global registry state in tests

`NewsRegistry.Configure(...)` and `ItemRegistry.Configure(...)` mutate process-global statics without restoring them in:

- `tests/Slums.Application.Tests/WorldEnrichmentApplicationTests.cs`
- `tests/Slums.Infrastructure.Tests/WorldEnrichmentSnapshotTests.cs`
- `tests/Slums.Core.Tests/World/WorldEnrichmentTests.cs`

TUnit runs test classes in parallel; a concurrent test reading these registries observes whatever the last `Configure` left behind. Follow the capture/restore-in-`finally` pattern already used in `tests/Slums.Infrastructure.Tests/JsonContentRepositoryTests.cs`, or extract a shared registry-scope helper.

## 3. Consolidate drifted UI thresholds into Core constants

Gameplay thresholds are re-hardcoded in screens and have already drifted:

- stress "red" is `> 80` in `GameScreenHudRenderer.GetStressColor` but `> 70` in `CommunityEventScreen` and `EntertainmentScreen`
- `GameScreen` hardcodes `UnpaidRentDays >= 5` instead of `RentState.FinalWarningDay`
- `PolicePressure >= 60` appears in `CrimeSessionService`, `RandomEventRegistry` (legacy default event), and `JsonContentRepository` (`high_police_pressure` condition) with no shared constant
- `CrimeScreen` and `GameScreen` re-hardcode the 80/100 arrest thresholds owned by `CrimeService` and `EndingService`

Fix direction: name each threshold once in `Slums.Core` (alongside `SurvivalStats.IsOverstressed`, `RentState`, `NarrativeSignalRules`) and reference it from services and screens. Display-only bands without a Core counterpart should move to Core too, mirroring `JobVariantThresholds`.

Acceptance: threshold changes cannot drift between execution and UI guidance.

## 4. De-duplicate save validation

`SaveGameValidator.Validate` (public, `Infrastructure/Persistence/SaveGameValidator.cs`) re-implements clock/player checks that the internal `SaveGameSnapshotValidator` performs via `ValidateClock`/`ValidatePlayer`/`ValidatePercentage`. Message strings have already diverged ("are negative" vs "is negative"), and the snapshot validator additionally checks `MedicineStock` and skills that `SaveGameValidator` omits. Both types live in the same assembly, so delegate from one to the other and keep a single message catalog; pin the message contract with tests.

## 5. Repo hygiene

- delete `md` (0-byte file at repo root) and `SKILL.md` (unfilled scaffold template)
- delete `bugs/` (manual bug screenshots); keep such evidence out of the repo
- add `TestResults/` and `src/Slums.Game/TestResults/` to `.gitignore` and remove the checked-in HTML reports under `src/Slums.Game/TestResults/`

## 6. Close high-value test gaps

Classes with no direct tests, by layer:

- Core: `SkillService`, `DebtService`, `NewsImpactCalculator`, `WeatherActivityRules`, `HeatBleedOverTable`, `CommunityActionRegistry`, `RobotRegistry`, `NarrativeEntryKnotCatalog`
- Application: `CommunityActionCommand`, `CommunityActionMenuQuery`, `AttendCommunityEventCommand`, `ClinicTravelMenuQuery`, `GameMutationLogger`, `SaveSlotRules`, `TechnicalRepairCommand`, `DigitalServiceCommand`, `PlantUpgradeCommand`, `FishTankUpgradeCommand`
- Infrastructure: `SaveValueParser`, `SaveGameSnapshotValidator`, `SeededRandomSource`
- Narrative.Ink: `NarrativeOutcomeMerger`, `InkChoiceAuditor`
- Game: only pure layout/input helpers are tested (31 tests). Screens stay untested by design; when changing a screen, extract testable pure logic into a layout/helper class as done with `EventLogViewerLayout`.

## 7. Smaller cleanups

- split `GameSessionSnapshot.Restore()` (~166 lines) into per-area steps mirroring the capture side
- split the 21 multi-type files that violate the one-type-per-file rule (worst: `src/Slums.Application/Narrative/INarrativeService.cs` with 6 types)
- adopt a project-level CA1822 policy for Application commands/queries to remove ~60 per-member `#pragma warning disable` pairs
- deduplicate UI helpers into `Slums.Game/Rendering`: `TrimToFit` (10 screens), `FormatDuration` (3), `WrapText` (3)
- rename `GameSession.TryDequeueNarrativeScene` / `TryTakePendingEndingKnot` (mutating `Try` methods) or document the mutation contract on them
- make `PlayerCharacter.Name`/`Gender` setters inaccessible (`init` or private) so identity changes must flow through commands

## Update rules

- keep this file short and actionable; every item needs concrete file references
- do not move product/roadmap work here and do not move engineering debt into `PLAN.MD`
- when an item is done, delete it rather than marking it complete
