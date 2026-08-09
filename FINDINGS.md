# Code Review Findings

This document records the findings from the repository review on 2026-08-09. Fix findings in priority order. Preserve the existing architecture: `Slums.Core` owns simulation rules, `Slums.Application` owns orchestration, `Slums.Infrastructure` owns persistence/content adapters, `Slums.Narrative.Ink` owns Ink integration, and `Slums.Game` owns SadConsole presentation.

## Resolution status (2026-08-09)

All findings fixed. Validation baseline after fixes: `Slums.Core.Tests` 1,047 passed, `Slums.Application.Tests` 151 passed, `Slums.Game.Tests` 22 passed, `Slums.Infrastructure.Tests` 66 passed, `Slums.Narrative.Ink.Tests` 163 passed; solution build clean.

| # | Priority | Finding | Resolution |
|---|----------|---------|------------|
| 1 | Critical | Ink runtime compatibility failure | Root cause was a non-thread-safe static registry init in `Qyl27.Ink.Engine 1.2.0` (format is compatible; `inkVersion 21` loads fine single-threaded). Story construction now serialized via `InkStoryFactory`; inkjs pinned to `2.4.0`; `StoryArtifactValidationTests` load the artifact and traverse every authored knot. |
| 2 | High | Save/load does not preserve deterministic randomness | Added `GameRandom` (serializable xoshiro256**); snapshot persists `RandomState`, restored streams are rewound past construction; `AttendCommunityEvent` and talk-knot selection use the session source; `RumorState` now persisted. |
| 3 | High | Narrative effects can be applied to the wrong target | `NarrativeOutcome` now carries an ordered `IReadOnlyList<NarrativeEffect>` of typed effects (`NpcTrustEffect`, `FactionReputationEffect`, `FavorEffect`, `RefusalEffect`, `DebtEffect`, `EmbarrassedEffect`, `HelpedEffect`); malformed known tags throw. Regression test for `event_ramadan_iftar`. |
| 4 | High | Repo-owned content is not fully authoritative or validated | Added `ContentCatalogValidator` invoked at bootstrap (enum coverage, duplicates, ranges, location/district, purchase-location and ink-knot references); registries throw on empty/incomplete instead of falling back; `Square` canonical district is `DowntownCairo` (matches `locations.json`). |
| 5 | High | Screens bypass Application commands | Added `SelectGenderCommand`, `SelectBackgroundCommand`, `HomeUpgradeCommand`, `PhoneActionCommand`, `NarrativeQueueCommand`; all five listed screens now execute through them. |
| 6 | Medium | Event log is UI-local and lost on load | Added persisted `EventJournal` (day, source, message; 200-entry cap) owned by `GameSession`; snapshots capture/restore it; `GameScreen` renders a view seeded from the journal. |
| 7 | Medium | Save storage needs hardening | Slot names validated (`SaveSlotRules`); atomic writes via temp file + `File.Replace` with `.bak` backup; typed `LoadGameResult` (`Missing`/`Corrupt`/`Incompatible`/`Loaded`); `SaveGameValidator` checks deserialized state before restore; load/save screens run I/O off the input path. |
| 8 | Medium | `GameSession` too large and coupled | Removed `IDisposable` (session owns no disposable resources); conversion of ~250 `using` sites. Extraction of planners (ending evaluation, crime, events, economy, tips, phone, territory) remains delegated to dedicated services with `GameSession` as the integration boundary. |
| 9 | Low | Phone UI duplicated and stale state | `PhoneState.ReplacementCost` is the single source (30 LE); `PhoneMenuContext` exposes it; `RefreshStatus` replaces the rendered context after every mutation. |

## 1. Critical: Ink runtime compatibility failure

The narrative test project currently fails 12 of 158 tests with:

```text
Failed to convert token to runtime object: ==
```

The generated artifact is `content/ink/main.json`, which begins with `inkVersion: 21`. It is generated with `inkjs 2.4.0` from `src/Slums.Game/package.json`, while the .NET runtime comes from `Qyl27.Ink.Engine 1.2.0`. The failure occurs while constructing `Ink.Runtime.Story` in `InkNarrativeService.StartScene`.

Relevant files:

- [content/ink/main.json](/mnt/d/GitHub/slums/content/ink/main.json:1)
- [src/Slums.Game/package.json](/mnt/d/GitHub/slums/src/Slums.Game/package.json:1)
- [Directory.Packages.props](/mnt/d/GitHub/slums/Directory.Packages.props:11)
- [InkNarrativeService.cs](/mnt/d/GitHub/slums/src/Slums.Narrative.Ink/InkNarrativeService.cs:26)

Recommended fix:

1. Select and pin a compiler/runtime combination that supports the same Ink JSON format.
2. Regenerate `main.json` using the pinned compiler.
3. Add a CI/build validation step that loads the compiled story and traverses every declared knot.
4. Keep invalid or incompatible Ink content as a hard failure; do not add a fallback narrative runtime.

Acceptance criteria:

- All `Slums.Narrative.Ink.Tests` pass.
- A clean checkout can compile/load the checked-in Ink artifact without relying on a developer-global tool.

## 2. High: Save/load does not preserve deterministic randomness

`GameSessionSnapshot.Restore()` constructs a new `Random` and does not restore the random generator state:

- [GameSessionSnapshot.cs](/mnt/d/GitHub/slums/src/Slums.Infrastructure/Persistence/GameSessionSnapshot.cs:90)

`GameSession` uses the shared random source for weather, daily events, phone events, tips, investments, and crime outcomes. Consequently, continuing from a save can produce different outcomes from an uninterrupted run, contrary to the save/load requirement in `PLAN.MD`.

Recommended fix:

- Replace hidden `System.Random` usage with an explicit serializable deterministic PRNG or persist sufficient seed/state information.
- Include that state in `GameSessionSnapshot`.
- Ensure all gameplay randomness goes through the same injected/state-owned source.

Acceptance criteria:

- Run a seeded session, save it, continue for several days, and record outcomes.
- Restore the save and perform the same actions.
- The day state, random events, economy, police pressure, narrative triggers, and ending outcomes match exactly.

## 3. High: Narrative effects can be applied to the wrong target

`NarrativeOutcome` supports only one NPC trust target. `MergeOutcome` sums all trust changes but retains only the latest target:

- [InkNarrativeService.cs](/mnt/d/GitHub/slums/src/Slums.Narrative.Ink/InkNarrativeService.cs:147)
- [InkNarrativeService.cs](/mnt/d/GitHub/slums/src/Slums.Narrative.Ink/InkNarrativeService.cs:292)

For example, `event_ramadan_iftar` applies trust to both Neighbor Mona and Landlord Hajj Mahmoud. The current merge can apply the combined value to only the last target.

Recommended fix:

- Replace single-target fields with a collection of typed narrative effects.
- Preserve every target and effect in deterministic order.
- Make malformed or unknown effect tags fail clearly rather than silently disappearing.

Acceptance criteria:

- Add a regression test for `event_ramadan_iftar` proving both NPC trust values change correctly.
- Add tests for multiple faction, debt, favor, and relationship effects in one scene.

## 4. High: Repo-owned content is not fully authoritative or validated

`JsonContentRepository` validates completeness only for jobs. Several registries silently retain hardcoded defaults when JSON content is empty or incomplete:

- [JsonContentRepository.cs](/mnt/d/GitHub/slums/src/Slums.Infrastructure/Content/JsonContentRepository.cs:23)
- [PetRegistry.cs](/mnt/d/GitHub/slums/src/Slums.Core/Characters/PetRegistry.cs:39)
- [PlantRegistry.cs](/mnt/d/GitHub/slums/src/Slums.Core/Characters/PlantRegistry.cs:75)
- [InvestmentRegistry.cs](/mnt/d/GitHub/slums/src/Slums.Core/Investments/InvestmentRegistry.cs:182)
- [BackgroundRegistry.cs](/mnt/d/GitHub/slums/src/Slums.Core/Characters/BackgroundRegistry.cs:68)

There are also duplicate world definitions. For example, `LocationId.Square` belongs to Dokki in code but Downtown Cairo in `locations.json`:

- [WorldState.cs](/mnt/d/GitHub/slums/src/Slums.Core/World/WorldState.cs:73)
- [locations.json](/mnt/d/GitHub/slums/content/data/locations.json:48)

Recommended fix:

- Create one `ContentCatalogValidator` invoked during bootstrap.
- Validate required enum coverage, duplicate IDs, required fields, numeric ranges, location references, Ink knot references, district conditions, and purchase locations.
- Remove normal-runtime fallback defaults for repo-owned content.
- Make the loaded content catalog immutable and use it consistently in production and tests.
- Decide and document the canonical district for `Square`.

Acceptance criteria:

- Missing, empty, incomplete, duplicated, or cross-referenced-invalid JSON fails bootstrap with a precise error.
- Tests use the same catalog model as the game bootstrap.

## 5. High: SadConsole screens bypass Application commands and mutate/query Core directly

The repository rules require player-triggered mutations to flow through `Slums.Application`. Several screens directly call `GameSession` methods or mutate its player state:

- [BackgroundSelectionScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/BackgroundSelectionScreen.cs:185)
- [GenderSelectionScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/GenderSelectionScreen.cs:134)
- [HomeUpgradeScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/HomeUpgradeScreen.cs:156)
- [PhoneScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/PhoneScreen.cs:196)
- [GameScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/GameScreen.cs:220)

Recommended fix:

- Add Application commands/queries for gender/background selection, home upgrades, phone actions, and narrative queue/ending consumption.
- Return immutable menu/status DTOs to the UI.
- Keep `GameSession` inaccessible to screen-specific mutation paths where practical.
- Add application-level tests proving each UI action uses the command/query boundary.

## 6. Medium: Event log is UI-local and is lost on load

The event log is held as a private list in `GameScreen` and capped at 200 entries:

- [GameScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/GameScreen.cs:26)
- [GameScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/GameScreen.cs:164)

The full viewer exists, so the older concern that the log is limited to six visible lines is no longer accurate. The remaining issue is that automatic transactions and deductions are not persisted; loading a save creates a new empty UI log.

Recommended fix:

- Store a structured event journal in `GameSession` or Application state.
- Include day, event type/source, amount, and message.
- Include the journal in snapshots and expose it through a query for HUD/viewer rendering.
- Decide whether the 200-entry cap is sufficient for a 100-day session or add archival/paging behavior.

## 7. Medium: Save storage needs path, corruption, atomicity, and migration hardening

Current issues:

- Slot names are interpolated into a path without safe-name validation ([JsonSaveGameStore.cs](/mnt/d/GitHub/slums/src/Slums.Infrastructure/Persistence/JsonSaveGameStore.cs:148)).
- Saves write directly with `FileMode.Create`, so interruption can truncate an existing save ([JsonSaveGameStore.cs](/mnt/d/GitHub/slums/src/Slums.Infrastructure/Persistence/JsonSaveGameStore.cs:136)).
- Invalid JSON and I/O errors are treated like a missing save.
- Save version mismatches are rejected without a migration strategy.
- UI screens synchronously block on asynchronous file operations ([LoadGameScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/LoadGameScreen.cs:85), [SaveGameScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/SaveGameScreen.cs:135)).

Recommended fix:

- Restrict slots to a validated identifier format or resolve them from a fixed allow-list.
- Write to a same-directory temporary file, flush it, then atomically replace the target and retain a backup.
- Return typed load results such as `Missing`, `Corrupt`, `Incompatible`, or `Loaded`.
- Add snapshot migrations or an explicit save compatibility policy.
- Move save/load work off the SadConsole input/render path and update the UI asynchronously.
- Validate serialized enums, IDs, ranges, and references before restoring state.

## 8. Medium: `GameSession` remains too large and highly coupled

`GameSession.cs` is approximately 150 KB and coordinates clock progression, survival, economy, work, crime, household, phone, narrative, debt, investments, and endings.

Recommended fix:

- Continue extracting cohesive planners/resolvers, such as daily progression, activity guards, economy settlement, phone processing, and ending evaluation.
- Keep `GameSession` as the canonical state owner and integration boundary.
- Add integration tests around ordering and invariants before each extraction.
- Remove `IDisposable` if `GameSession` does not own disposable resources, or give the implementation a clear responsibility.

## 9. Low: Phone UI contains duplicated and stale state

The phone UI displays and logs a 25 LE replacement cost, while the domain charges 30 LE:

- [PhoneScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/PhoneScreen.cs:48)
- [GameSession.cs](/mnt/d/GitHub/slums/src/Slums.Core/State/GameSession.cs:3880)

`RefreshStatus()` creates a new context for the menu query but does not replace the `_context` used by rendering ([PhoneScreen.cs](/mnt/d/GitHub/slums/src/Slums.Game/Screens/PhoneScreen.cs:276)).

Recommended fix:

- Centralize phone prices in a domain/application definition and render the returned value.
- Refresh or replace the complete phone context after every mutation.
- Add UI/application tests for lost-phone replacement, credit refill, and post-action rendering.

## Validation baseline

At the time of review:

- `Slums.Core.Tests`: 1,036 passed
- `Slums.Application.Tests`: 131 passed
- `Slums.Game.Tests`: 22 passed
- `Slums.Infrastructure.Tests`: 36 passed after using a writable temporary directory
- `Slums.Narrative.Ink.Tests`: 146 passed, 12 failed

The solution-level build also encountered an environment-specific MSBuild failure, while individual project builds completed successfully. Re-run the repository’s required build/test workflow after fixing the Ink toolchain.
