# Improvement Handoff

This document records the review findings and gives the next agent a completion path. The review found no source defects that prevent the current vertical slice from building or passing its tests, but it did find several integration and maintainability risks.

## Validated baseline

- `dotnet build Slums.slnx --no-restore -m:1 -p:UseSharedCompilation=false` succeeds with zero warnings and zero errors.
- All five executable TUnit suites pass: 1,504 tests total.
  - Core: 1,069
  - Application: 174
  - Game: 25
  - Infrastructure: 72
  - Narrative.Ink: 164
- The default parallel solution build can fail in the current ARM64 environment with `0 Warning(s), 0 Error(s)` before compilation. Use the serialized command above for validation and document the CI equivalent if this remains reproducible.
- The review did not modify source files.

## Priority 0: repair narrative state and effect boundaries

### 1. Make Ink state synchronization real

`InkNarrativeService` attempts to synchronize money, health, energy, hunger, stress, mother health, food, day, background, and gender into Ink, but `content/ink/main.ink` currently declares only `gender`. `TrySetGlobalVariable` silently returns when a variable is absent, so most of the synchronization is currently a no-op.

Relevant files:

- `content/ink/main.ink:3`
- `src/Slums.Narrative.Ink/InkNarrativeService.cs:271-301`

Completion criteria:

- Declare every required global with the correct Ink type and default value, or remove fields that are intentionally not narrative-facing.
- Add artifact validation that required globals exist.
- Add a test proving a scene visibly branches on at least one synchronized gameplay value.
- Verify all authored scenes still load and traverse successfully.

### 2. Apply narrative outcomes through one mutation boundary

`NarrativeScreen` currently applies the pending outcome directly to an `INarrativeOutcomeTarget`. The `GameSession.Adjust*` methods then mutate raw state without recording a structured mutation or calling failure-ending checks. A narrative scene can therefore cause lethal state changes without immediately producing the corresponding ending or mutation record.

Relevant files:

- `src/Slums.Game/Screens/NarrativeScreen.cs:167-186`
- `src/Slums.Application/Narrative/NarrativeSceneExtensions.cs:7-86`
- `src/Slums.Core/State/GameSession.cs:2420-2463`
- `src/Slums.Core/State/GameSession.cs:3062-3076`

Completion criteria:

- Add an application command/use case, or an equivalent `GameSession` boundary, for applying a complete narrative outcome.
- Capture before/after state and record the source knot/scene and effect reason.
- Run failure-ending checks after applying the outcome.
- Preserve NPC, flag, money, health, nutrition, stress, and household effects.
- Add tests for player-health failure, mother-death failure, mutation records, and normal multi-effect scenes.
- Keep the SadConsole screen responsible only for presentation and navigation.

### 3. Preserve all narrative flags

`MergeOutcome` stores only one `SetFlag` string and prefers the later value. Future scenes with multiple `FLAG` tags can silently lose earlier flags.

Relevant file: `src/Slums.Narrative.Ink/InkNarrativeService.cs:303-322`

Completion criteria:

- Change the outcome protocol to carry `IReadOnlyList<string>` or typed effects.
- Add a test with multiple flags emitted during one scene.
- Keep compatibility with existing authored tags while migrating.

## Priority 1: make the simulation easier to extend safely

### 4. Split `GameSession` by responsibility

`GameSession` is approximately 4,400 lines with roughly 171 public members. It remains the canonical state owner, but daily resolution, action calculations, narrative application, and several economy/household concerns should move into focused services or planners.

Suggested extraction order:

1. `DayEndResolver` for rent, nutrition, weather, household assets, economy, phone/tips/news, and end conditions.
2. `NarrativeOutcomeApplier` for typed narrative effects and mutation records.
3. Shared action calculators for work/crime previews and committed results.
4. Additional focused evaluators only where tests show repeated logic.

Do not create a second runtime state model. Services should receive immutable inputs and return results; `GameSession` should remain the state owner and integration surface.

### 5. Add seeded scenario acceptance tests

Current unit coverage is strong, but add a small scenario harness that drives application commands against deterministic sessions.

Required scenarios:

- all backgrounds, including the Sudanese refugee route;
- honest work, crime, debt, travel, and clean-life routes;
- every documented ending;
- day-35 survival and ending checks;
- preview values equal to the values used by the committed action;
- save/load followed by the same next action produces the same result.

Each scenario should report the seed, day, location, action, and ending to make balance regressions reproducible.

### 6. Centralize authored content references

Some crime opportunities and condition identifiers remain hardcoded in Core while most world data is JSON-authored. This creates drift risk.

Relevant areas:

- `src/Slums.Core/State/GameSession.cs:1441-1468`
- `src/Slums.Infrastructure/Content/JsonContentRepository.cs:154-184`

Move crime opportunities and their display/gating data into validated JSON, or establish one central registry and validate every reference during bootstrap. Add tests for unknown identifiers and missing content.

## Priority 2: narrative and cultural editorial pass

### 7. Remove stale or unreachable Ink knots

The ending file contains knots that do not appear to be selected by the active ending catalog, including:

- `ending_collapse`
- `ending_stability_location_dokki`
- `ending_stability_location_shubra`
- `ending_debt_violence`
- `ending_buried_by_heat`

Relevant files:

- `content/ink/endings.ink:13`
- `content/ink/endings.ink:35`
- `content/ink/endings.ink:111`
- `src/Slums.Core/Endings/EndingKnotCatalog.cs`
- `src/Slums.Core/Endings/EndingService.cs`

For each knot, either connect it to a documented ending path, mark it as intentionally archived, or remove it. `ending_debt_violence` especially needs a decision because the current plan describes debt as contributing to existing failure endings rather than defining a separate ending.

Add a validation rule for orphan knots, with an explicit annotation mechanism for intentionally unreachable test or draft content.

### 8. Vary NPC voice and agency

The NPC library has strong atmosphere and local detail, but many scenes reuse similar rhetorical patterns and hardship beats. An editorial pass should give each recurring NPC a distinct:

- diction and sentence rhythm;
- want, fear, contradiction, and practical competence;
- relationship to repair culture, faith, work, and technology;
- form of agency beyond offering the protagonist help or danger.

The existing duplicate-scene test checks normalized repetition, but it does not detect formulaic variation. Add editorial review criteria or lightweight metadata rather than relying only on structural uniqueness.

### 9. Clarify identity and cultural continuity

The Sudanese background says the protagonist fled Khartoum and is treated as a refugee, while also saying “You are Egyptian by law.” This may be intentional, but the legal status, citizenship, and family history need to be explicit so the player is not given contradictory identity information.

Relevant file: `content/data/backgrounds.json:31-40`

Review Nubian/Sudanese terminology and imagery with an appropriate sensitivity reader, including the “cadences of Nubi” phrasing in `content/ink/seasons.ink`. Avoid treating displacement, accent, food, or faith as a single undifferentiated hardship marker.

### 10. Broaden the technology texture

The technology layer is effective when it shows access, surveillance, labor, and repair. Keep that tension, but include more ordinary useful and locally maintained technology so every advanced system does not function primarily as oppression or metaphor.

## Performance note

The performance scan found no urgent anti-pattern. Source-generated JSON serialization and the absence of repeated regex construction are positive. There are many LINQ call sites, so profile day-end resolution and action-menu queries before replacing them with manual loops; optimize only where measurements show a real hot path.

## Suggested implementation order

1. Fix required Ink globals and add validation/tests.
2. Introduce the narrative outcome mutation boundary and failure checks.
3. Add seeded scenario tests for endings and save/load continuity.
4. Remove or connect stale ending knots.
5. Extract day-end resolution and shared action calculators from `GameSession`.
6. Centralize crime/content identifiers.
7. Complete the narrative voice and cultural editorial pass.

## Definition of done

- Ink globals are validated and gameplay state changes narrative branching.
- Narrative effects are typed, recorded, failure-aware, and applied outside the UI layer.
- All documented endings are reachable through deterministic scenarios, or explicitly documented as unavailable.
- No unexplained orphan Ink knots remain.
- The solution and all test projects pass using the repository’s prescribed workflow.
- `AGENTS.md`, `PLAN.MD`, or this file is updated whenever the architecture or content workflow changes.
