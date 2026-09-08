# REC — Codebase Review Recommendations

Date: 2026-09-08
Source: codebase audit of `src/`, `tests/`, `content/` against `AGENTS.md` and `REQS.md`.
Status of `OPP.md`, `PLAN.MD`, `MEMORY.MD`: none of these exist in the repo; this file stands alone.

What is healthy (preserve):
- Dependency direction holds: `Slums.Core` refs nothing local; `Slums.Application` refs `Core` + logging only; `Slums.Infrastructure` implements app contracts; `Slums.Narrative.Ink` adapts Ink; `Slums.Game` delegates mutations via commands (e.g. `src/Slums.Game/Screens/WorkScreen.cs:144`, `src/Slums.Game/Screens/NarrativeScreen.cs:218`).
- Tests are TUnit-only, `OutputType=Exe` pattern kept.
- `ScreenTransition` convention (`FadeTo` forward / `SwitchTo` back) is sound in `src/Slums.Game/Screens/ScreenTransition.cs:10-42`.

Process top-down. Each item lists: problem, evidence, fix, acceptance.

---

## 1. Eliminate mutable static registries (testability, parallel-test pollution)

Problem: process-global registries share state across sessions (26 `*Registry.cs` files in `Slums.Core`; ~13 confirmed mutable `Configure()`-style).
Evidence:
- `src/Slums.Core/World/WorldState.cs:9` — `static _locations` inside instance class (worst case).
- `src/Slums.Core/Jobs/JobRegistry.cs:6,24-34`, plus `BackgroundRegistry`, `RandomEventRegistry`, `DistrictConditionRegistry.cs:299`, `NewsRegistry.cs:9`, `PlantRegistry.cs:75`, `PetRegistry.cs:39`, `InvestmentRegistry.cs:204`, `RobotRegistry.cs:49`, `ItemRegistry.cs:9`, `NpcScheduleRegistry.cs:9` — 26 `*Registry.cs` files exist in total, so the list above is not exhaustive. `DigitalServiceRegistry.cs:6` and `TechnicalRepairRegistry.cs:6` are static classes with hardcoded inline definitions rather than `Configure()`; fold them into the same migration.
- Constructor side-effects in `src/Slums.Core/State/GameSession.cs:87-103`.

Fix:
- Move catalog data to instance members of `GameContentCatalog`; inject into `GameSession` ctor.
- Keep static `Configure()` shims only as `[Obsolete]` test compat during migration, then delete.
- Remove `WorldState` static/instance mix first (highest risk).

Acceptance: two `GameSession` instances in one process with different catalogs do not interfere; `dotnet run --project tests/Slums.Core.Tests` passes with test parallelization on.

## 2. Freeze and shrink `GameSession` facade

Problem: god-object metrics; every new domain adds 3-5 passthroughs.
Evidence: `src/Slums.Core/State/GameSession.cs:1006` lines; with partials (`Snapshot.cs:189`, `Resolvers.cs:58`, `Diagnostics.cs:56`, `Actions.cs:51`, `Queries.cs:19`) total ~1379 lines, ~290 members (~190 methods/constructors plus ~78 properties/events), 34 usings (`GameSession.cs:1-34`, `:64-216`).

Fix:
- No new passthrough methods. Route new domains via existing `*Service` + `INarrativeOutcomeTarget` extension.
- Optionally split partials by area (`GameSession.{Commerce,Care,Travel,Narrative}.cs`) with segregated interfaces.

Acceptance: line/method count does not grow on next feature; new domain ships without touching `GameSession.cs`.

## 3. Dedupe price-modifier pipeline

Problem: same modifier summation copied in 3+ places; drift risk between preview and commit.
Evidence:
- `src/Slums.Core/Expenses/FoodShopService.cs:19-41` vs `:43-61` (~12 lines verbatim); double `NewsImpactCalculator` call at `:33,36`.
- `src/Slums.Core/Characters/ClinicVisitService.cs:249-260`, `src/Slums.Core/World/TravelService.cs:209,232,241`.
- `TravelService.cs:19,84,136,143,150,157,164` — 7x `FirstOrDefault(candidate => candidate.Id == ...)` with no `GetLocationById`.

Fix:
- Single `PriceModifierPipeline.GetFood/StreetFood/Medicine/Clinic/Travel` builder (district + schedule + season + weather + territory + news + infra + investments + skills).
- Add `WorldState.GetLocationById`, `RoboticsState.GetOperational(type)`; fix `AdjustFoodStockpile` O(n) loop at `GameSession.cs:655-667` to bulk op; split `HouseholdAssetsService` into `Pet/Plant/Robotics` services.

Acceptance: one calculation path for preview and commit; add unit test asserting preview == commit for food/clinic/travel.

## 4. Ink fail-fast: close the remaining silent paths

Problem: two genuine silent paths remain, but an earlier draft overstated this area. Required globals, orphan knots, and malformed/unknown effect tags already fail fast: `InkStoryFactory.cs:31-34` runs `InkStoryValidator.Validate`, `InkStoryCatalog.ValidateRequiredGlobals` (throws at `InkStoryCatalog.cs:82-86`), and `ValidateEntryKnots` (throws at `:113-119`) on every `Story` construction; unknown effect-tag keys throw at `InkStoryValidator.cs:125-128`; malformed effect values throw with 16 tested cases (`InkNarrativeTagParserTests.cs:59-88`).

Verified gaps:
- `src/Slums.Narrative.Ink/InkNarrativeService.cs:50-59` — `SelectChoice` logs a warning and silently returns on an out-of-range choice index; no test anywhere exercises an invalid index. (Precedent for failing fast exists: `NarrativeScreen.cs:216-218` throws on a completed scene missing its source knot.)
- Colon-less tags are dropped by design (`InkTagEffectParser.cs:71-89` → dropped at `InkNarrativeService.cs:112-119`, pinned by `InkNarrativeTagParserTests.cs:90-94`), and `InkStoryValidator.cs:118-121` likewise skips them — so a typo'd effect tag like `MONEY 10` (missing `:`) escapes both runtime parsing and bootstrap validation.

Fix:
- Throw on an invalid choice index (a UI/index desync is a programming bug; fail fast) and add the negative test.
- In the bootstrap validator, reject colon-less tags that look like effect keys (`LooksLikeEffectKey`, `InkStoryValidator.cs:271-274`) while still allowing ordinary prose tags.
- Do not add a throw to `InkVariableSynchronizer.cs:87-98` for "missing globals": that branch is unreachable for the synchronized set because `InkStoryFactory` validates required globals before sync runs. Leave the strict `InkTagCatalog.cs:6-12` allowlist as-is — it is already enforced.

Acceptance: a negative test for a bad choice index fails fast; a `MONEY 10`-style tag fails artifact validation; existing narrative tests stay green.

## 5. Validate saves on write; unify slot guard

Problem: corrupt saves discovered only on load.
Evidence:
- `src/Slums.Infrastructure/Persistence/JsonSaveGameStore.cs:41-49` captures + writes without `SaveGameValidator.Validate`; load validates at `:93`.
- `src/Slums.Application/Persistence/SaveGameUseCase.cs:16-22` uses `ThrowIfNullOrWhiteSpace` while store (`:34,56`) and `LoadGameUseCase.cs:18` use `SaveSlotRules.EnsureValidSlot`.
- `JsonSaveGameStore.cs:39` unguarded `ReadDocumentAsync` for `CreatedUtc` — a corrupt existing file currently blocks overwriting that slot. (Do not treat the `.bak` retention at `:197-206` as a defect: it is deliberate, per the "retaining the previous version as a backup" comment; `.tmp` is already cleaned on failure.)

Fix:
- Call `Validate(document.SessionSnapshot)` before `WriteAtomicAsync`.
- Call `EnsureValidSlot` in `SaveGameUseCase`.
- Wrap `CreatedUtc` preservation in try/catch with `now` fallback.

Acceptance: invalid snapshot fails at save time with test; traversal-guard test passes for save and load paths.

## 6. Harden Game startup / DI wiring

Problem: bootstrap can leave half-configured globals; null catalog tolerated.
Evidence:
- `src/Slums.Game/SadConsoleGame.cs:82-126` — content load + exactly 7 static `Configure()` calls run on the main thread before the SadConsole game loop starts.
- `src/Slums.Game/Program.cs:18-33` — `NewGameUseCase` not registered; `SadConsoleGame.cs:35,69,95-101` passes nullable catalog into `new NewGameUseCase(_randomSource, _contentCatalog)`.
- `src/Slums.Game/Screens/MainMenuScreen.cs:119` — `Environment.Exit(0)` bypasses `IHost` disposal.
- `src/Slums.Game/Screens/TalkScreen.cs:94,101` — threads `_gameState.SharedRandom` through UI.

Fix:
- Register `NewGameUseCase` in DI; assert `_contentCatalog != null` fail-fast.
- Extract `IContentBootstrapper` for load + `Configure()` so startup is unit-testable.
- Replace `Environment.Exit(0)` with a normal return from the game loop so `Program.cs`'s `using` host disposal runs. Note: `IHostApplicationLifetime` is not usable as-is — `Program.cs` builds the host but never starts it (no `host.Run()`), so lifetime callbacks would never fire without restructuring `Main` first.
- Resolve `IRandomSource` from `GameRuntime` instead of passing `SharedRandom`.

Acceptance: startup with missing/invalid JSON fails fast with clear error; no `new Random` / direct stat assignment in `Slums.Game` (grep clean).

## 7. Fix input gating and mouse/render drift

Problem: held keys double-fire across transitions; mouse hit-boxes diverge from render.
Evidence:
- Gated: `CrimeScreen.cs:41`, `PhoneScreen.cs:37`, `NewsScreen.cs:28`, `GameScreen.cs:29` (Enter/Escape consumed via gate at `:113,:136,:154`) + `ScreenActionKeyGate.cs:27-59`. Raw (no gate): `WorkScreen.cs:90,96`, `TalkScreen.cs:86,107`, `TravelScreen.cs:94,106`, `BackgroundSelectionScreen.cs:110,116`, `GenderSelectionScreen.cs:88,94`, `MainMenuScreen.cs:64`, `LoadGameScreen.cs:132`.
- `WorkScreen.cs:54` renders at `effectiveListY` but `:116` hit-tests at `ListY`; same `CrimeScreen.cs:76` vs `:144`. `TravelScreen.cs:158-159` measures `walkPrefix` string width for hit-test.
- `NumberKeyMapper.cs:7-27` wired only in `NarrativeScreen.cs:141` and `NewsScreen.cs:72`.

Fix:
- Extend `ScreenActionKeyGate` to all Enter/Escape transitions; suppress destinations in `GameScreenNavigator.cs:301-305`.
- Shared `rowY(i)` helper for render + mouse; layout constants for travel walk/transport hit areas.
- Add 1-9 selection to all list screens.

Acceptance: manual hold-Enter across `Game -> Work -> Return` fires once; mouse click selects rendered row at `100x28`.

## 8. Ink artifact freshness

Problem: stale `main.json` silently wins over edited sources.

Orphan-knot coverage was dropped from this item after verification: `InkStoryCatalog.ValidateEntryKnots` already throws on unclassified top-level knots at every `Story` construction (`InkStoryCatalog.cs:113-120`, invoked from `InkStoryFactory.cs:34`), and `NarrativeEntryKnotCatalog.GetUnclassified()` has a direct unit test (`tests/Slums.Core.Tests/Coverage/HighValueCoreCoverageTests.cs:112`).

Evidence:
- `src/Slums.Game/package.json:2-7` (`inkjs 2.4.0`); `StoryArtifactValidationTests.cs:19-47` checks `inkVersion==21` and globals but performs no source-vs-artifact freshness comparison (no mtime/hash check exists anywhere in the test suite).
- `InkStoryLoader` prefers the on-disk `content/ink/main.json` when present, so an artifact left stale by editing any of the 12 `.ink` files without `npm run compile-ink` silently wins.
- The 12 `.ink` sources live in `content/ink/` and tests already resolve those paths (`RecurringNpcSceneValidationTests.cs:36-51` reads source text), so a freshness check is feasible without new tooling.

Fix:
- Fail a test when the `.ink` sources and `main.json` diverge. Prefer a committed hash of the concatenated `.ink` files over mtime: git checkout rewrites timestamps, so mtime is flaky on fresh clones and CI. Do not recompile inside the test (that would require node + inkjs in every test run).

Acceptance: editing any `.ink` file without `npm run compile-ink` fails a test.

## 9. Close test-vs-bootstrap catalog divergence

Problem: green tests, red startup.
Evidence:
- `tests/Slums.TestSupport/ContentCatalogTestFixture.cs:13-24` loads 6 of 11 JSONs (omits pets/plants/robots/news/items).
- `ContentCatalogValidator.cs:29-33,53-62` conditionally validates `robots/newsFlashes/items/npcSchedules`; `ContentCatalogValidatorTests.cs:470-489` omits them with 4 synthetic knots.
- No dedicated Core `Inventory/` / `News/` / `Infrastructure/` test dirs (a `Phone/` dir already exists with 5 test files — do not duplicate it). `AcquireItemCommand` and `InventoryMenuQuery` have one incidental test each but no dedicated coverage. `JsonContentRepository.cs:155-186` hardcoded `conditionId` switch has no exhaustive test: only 5 of ~24 arms are covered, and arms like `in_ard_al_liwa`, `in_bulaq_al_dakrour`, `in_shubra`, `in_downtown_cairo`, `sudanese_refugee_home` are untested and unused by shipped content.

Fix:
- Full-catalog fixture + full `Validate()` path; replace synthetic knots with real `InkStoryCatalog.GetKnotNames()`.
- Add `InventoryState/ItemRegistry`, `NewsState` lifecycle, `InfrastructureState` tests; add dedicated Application inventory tests; parametrized `conditionId` coverage over `random_events.json` (and delete or exercise dead switch arms).

Acceptance: fixture loads all 11 JSONs; every `conditionId` in `random_events.json` maps; new catalog cross-ref bug fails in unit tests, not just bootstrap.

## 10. Analyzer / logging / RNG hygiene

Evidence:
- 44x `#pragma warning disable CA5394` (`PhoneMessageGenerator.cs:38,54,128,159,208`, `TipGenerator.cs:63-277` with 13 sites, `NpcEconomyResolver.cs:30,42,63`, `RandomEventService.cs:129,147`); `CA1822` in `CrimeService.cs:9,29`, `JobService.cs:10,86,127`, `LocationPricingService.cs:9-123`; `CA1024` in `GameSession.cs:486-507`.
- `GameSession.cs:106` `Random.Shared.NextInt64()` non-seeded fallback; `SeededRandomSource.cs:12-20` seeds from `Environment.TickCount` (int, ms-resolution — collision risk); `GameSessionSnapshot.cs:127-132` has a documented, deliberately stable fallback seed for pre-RNG saves (not silent, but worth a warning when the legacy path is hit).
- `GameMutationLogger.cs:34-62` disables `CA1848`, unbounded snapshot join (`:71`); `EventId 1-6` reused across `JsonSaveGameStore`, `JsonContentRepository`, `InkNarrativeService`, and `SadConsoleGame`; `SaveCompleted` (`JsonSaveGameStore.cs:280`) logs at Debug while sibling save-path messages use Warning/Information; ~100 commands/queries have zero intent logs.
- `NewsState.cs:43-47` `!` + null-check; `ClinicVisitService.cs:95`, `WorkSessionService.cs:296,314` per-call `ToHashSet()` (once per player action, not per frame — cleanup, not a hot-path fix).

Fix:
- Centralize gameplay RNG in `GameRandom` with one suppression; make `CA1822` classes `static`; explicit seed factory with `RunId` logging; `Guid`/`Random.Shared.Next()` seed source; warn on legacy fallback.
- Convert `GameMutationLogger` to `LoggerMessage.Define`; centralize `EventId` registry; document mutations-only vs intent-log policy.
- Change flag params to `IReadOnlySet<string>`; cache set in `GameNarrativeState`; rewrite `TryGetActive` without `!`; `ToHashSet` removal.

Acceptance: `grep -rn "pragma warning disable CA5394" src/` drops to 1 site; analyzers pass with `TreatWarningsAsErrors`; no `!` on content-lookup paths.

---

## Suggested order

1. Items 1 + 9 together (registry extraction needs full fixture to avoid regressions).
2. Items 4 + 8 (fail-fast narrative; cheap, high signal).
3. Item 5 (save validation).
4. Items 3 + 10 (dedupe + hygiene).
5. Items 6 + 7 (startup + UI polish).

## Validation per change

```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

If `.ink` sources change: from `src/Slums.Game`, run `npm run compile-ink` and commit regenerated `content/ink/main.json`.
