# REC — Codebase Review Recommendations

Date: 2026-09-08 (updated after remediation pass on the same date)
Source: codebase audit of `src/`, `tests/`, `content/` against `AGENTS.md` and `REQS.md`.

Status: items 1, 3–10 from the original audit have been implemented and were removed from
this file. The "healthy" findings (dependency direction, TUnit-only tests, `ScreenTransition`
convention) remain true and preserved. What follows is the remaining work, most of it
optional polish.

---

## Completed in this pass (for the record; no action required)

1. **Mutable static registries eliminated.** All `Configure()`-style registries, the
   `WorldState` static/instance mix, and `GameContentCatalog.FromConfiguredRegistries()` are
   gone. Sessions receive an immutable catalog: JSON-loaded at bootstrap, `TestContent.Catalog`
   in tests (loaded once, shared read-only, parallel-safe). The migration also surfaced a real
   divergence bug — five per-district `_steady_day` baseline conditions existed only in code
   defaults and were missing from `district_conditions.json`; they have been added to the JSON
   and the 35-day golden digest was re-pinned.
2. **`GameSession` facade frozen** (was item 2). Enforcement already existed as
   `GameSessionSafetyNetTests.PublicApi_ShouldMatchApprovedMemberNames`; the acceptance
   criterion (API does not grow) is met by policy. The optional area-partials split was not
   done — see remaining work below.
3. **Price-modifier pipeline deduped** (was item 3). `PriceModifierPipeline` is the single
   calculation path for food/street food/medicine/clinic/travel prices;
   `RoboticsState.GetOperational` added; `AdjustFoodStockpile` is a bulk operation;
   `HouseholdAssetsService` split into `PetAssetsService` / `PlantAssetsService` /
   `RoboticsAssetsService`; preview==commit unit tests added for food, clinic, and travel.
4. **Ink fail-fast** (was item 4), **save-on-write validation** (was item 5), **Ink artifact
   freshness** (was item 8): verified complete at review time.
5. **Startup/DI hardening** (was item 6). `IContentBootstrapper` now owns load + validation +
   publish; `NewGameUseCase` is DI-registered; `Environment.Exit` is gone; UI no longer
   threads `SharedRandom` (commands fall back to `session.SharedRandom`).
6. **Input gating / hit-test drift** (was item 7). All screens use `ScreenActionKeyGate`,
   list rows share render/hit-test geometry, `NumberKeyMapper` is wired on every list screen,
   `GameScreenNavigator.NavigateTo` suppresses parent keys on the way into a sub-screen, and
   TravelScreen's walk/transport hit areas come from shared layout helpers.
7. **Test-vs-bootstrap divergence** (was item 9). Fixture loads all 11 JSON files;
   `ContentCatalogValidator` takes the full catalog (no optional skips) and also validates
   bulletin text, gameplay summary, and null effect/effect lists per district condition;
   shipped `random_events.json` condition-id coverage has a dedicated test; the five dead
   `conditionId` switch arms were deleted; dedicated Application inventory tests exist.
8. **Analyzer/RNG/logging hygiene** (was item 10). Gameplay RNG is centralized:
   `GameRandom.FromEntropy()` is the single unseeded source and `Slums.Core` carries exactly
   one CA5394 suppression (assembly-level, documented); `SeededRandomSource` seeds from a
   `Guid`; `GameMutationLogger` uses `LoggerMessage.Define` with a bounded snapshot payload;
   EventIds are centralized in `Slums.Core.Diagnostics.LogEvents`; `SaveCompleted` logs at
   Information; legacy pre-RNG saves log a warning on load; `NewsState.TryGetActive` no longer
   uses `!`; `StoryFlags` is exposed as `IReadOnlySet<string>` so call sites stop rebuilding
   sets; static-castrated CA1822 members were made static where the service truly had no
   instance state.

Validation after the pass: `dotnet build Slums.slnx` clean; all five test projects green
(Core 1276, Application 216, Game 33, Infrastructure 97, Narrative.Ink 227).

---

## Remaining work

### 1. Load investment, digital-service, and technical-repair definitions from JSON

`InvestmentDefinitions`, `DigitalServiceDefinitions`, and `TechnicalRepairDefinitions` are
still code-owned defaults inside `Slums.Core` (wire format goes through
`GameContentCatalog`'s optional constructor parameters). AGENTS.md prefers repo-owned JSON.
Move them to `content/data/*.json`, add `JsonContentRepository` loaders plus
`ContentCatalogValidator` rules, and drop the code defaults.

Acceptance: no game-content arrays in `Slums.Core`; bootstrap validates them like the other
eleven collections.

### 2. Optionally split `GameSession` partials by area

`GameSession.{Commerce,Care,Travel,Narrative}.cs` region-style partials remain a
maintainability nicety. The API freeze is already enforced; only do the split if/when a
feature forces the issue.

### 3. Intent logging policy for application commands/queries

Roughly a hundred commands/queries emit no logs by design (mutations are journaled through
`GameSession.RecordMutation` and surfaced by `GameMutationLogger`). If intent-level logging
is ever wanted for support diagnostics, add it per command via `LogEvents` — do not log
inside `Slums.Core` services.

### 4. Manual QA for input gating

Hold-Enter across `Game -> Work -> Return` and mouse-click-on-rendered-row at 100x28 were
addressed structurally (gate on every screen, shared row geometry, navigator suppression).
They still deserve one manual pass on Windows since the harness cannot run the SadConsole
game loop.

## Validation per change

```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

If `.ink` sources change: from `src/Slums.Game`, run `npm run compile-ink`, then regenerate
`content/ink/source-manifest.sha256` (SHA-256 of each `.ink` file, `hash  filename` lines),
and commit the regenerated `content/ink/main.json`. The freshness test
(`InkSources_HaveFreshnessManifest`) fails when sources and manifest diverge.
