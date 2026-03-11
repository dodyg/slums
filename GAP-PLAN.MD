# GAP-PLAN.MD

Identified during playtest on 2026-03-11.  
Read `REQS.md`, `PLAN.MD`, and `AGENTS.md` before implementing any gap.  
Work through gaps in the order listed. Each gap is self-contained and independently testable.

---

## Current state summary

Phases 0–4 of `PLAN.MD` are substantially complete:

- Solution scaffold, build tooling, and test projects are in place.
- `GameState`, `GameClock`, `SurvivalStats`, `HouseholdState`, `WorldState` are implemented and tested.
- Three preset backgrounds (Medical School Dropout, Political Prisoner, Sudanese Refugee) work end-to-end.
- Five Cairo locations across two districts are defined.
- Three honest work jobs (bakery, house cleaning, call center) are implemented.
- `InkNarrativeService` is wired up; three background intro knots exist in `content/ink/main.ink`.
- SadConsole screens: main menu, background selection, game screen, work screen, shop screen, travel screen.
- 81 unit tests pass.

The following gaps remain before Phase 5–8 scope is reached.

---

## GAP-1 — Narrative screen: wire Ink intros into the game

**Priority:** High — the Ink runtime and service are implemented but never called from the UI.

### What is missing

After background selection, `BackgroundSelectionScreen` transitions directly to `GameScreen` without triggering the background intro scene (`intro_medical`, `intro_prisoner`, `intro_sudanese`). `InkNarrativeService` is never injected or called from any screen.

### What to build

1. **`NarrativeScreen`** in `Slums.Game/Screens/` — a SadConsole screen that:
   - Receives an `INarrativeService` and a `GameState`.
   - Displays `CurrentText` as word-wrapped paragraphs in a scrollable panel.
   - Renders numbered choices at the bottom (`1. … / 2. …`).
   - Handles digit-key and Enter input to call `SelectChoice(index)`.
   - When `IsSceneActive` becomes false, applies any `GetPendingOutcome()` via `NarrativeSceneExtensions.ApplyOutcome()` and transitions to the caller-supplied next screen.
   - Shows `[Press Enter to continue]` when there is text but no choices and the story can no longer continue.

2. **Wire DI** in `SadConsoleGame` / `Program.cs`:
   - Register `InkNarrativeService` as `INarrativeService` (singleton).
   - Pass it through to `BackgroundSelectionScreen` → `NarrativeScreen` → `GameScreen`.

3. **Trigger in `BackgroundSelectionScreen.ConfirmSelection()`**:
   - Call `narrativeService.StartScene(background.InkIntroKnot, gameState)`.
   - Navigate to `NarrativeScreen`; pass `GameScreen` as the next screen.

4. **DI plumbing**: propagate `INarrativeService` and `GameState` through existing constructors that need them. Keep construction of `GameState` at composition root (not inside screen constructors) so state is shared.

### Tests to add

- `NarrativeScreen` is UI so skip rendering tests; test `ApplyOutcome` integration already covered.
- Add a test in `Slums.Application.Tests` that confirms `StartScene` followed by choices followed by `GetPendingOutcome()` produces expected stat deltas from the intro knots.

### Definition of done

- Starting a new game shows the Ink intro for the selected background.
- Choosing a dialogue option advances text.
- When the intro ends, the player is on the `GameScreen` with outcome applied.

---

## GAP-2 — Save / Load

**Priority:** High — `Load Game` menu option is a stub; no persistence exists.

### What is missing

- `Slums.Infrastructure` project is empty (no source files).
- No `ISaveGameStore` implementation exists.
- `LoadGame` on `MainMenuScreen` is a no-op.

### What to build

#### `Slums.Infrastructure`

1. **`SaveEnvelope`** record — matches the JSON shape from `PLAN.MD`:
   ```csharp
   record SaveEnvelope(int SaveVersion, Guid RunId, DateTimeOffset CreatedUtc,
       DateTimeOffset LastPlayedUtc, string CheckpointName,
       GameStateDto GameState, NarrativeStateDto NarrativeState);
   ```

2. **`GameStateDto`** — a serializable projection of `GameState` (money, hunger, energy, health, stress, mother health, food stockpile, clock day/hour/minute, background type, current location id).

3. **`NarrativeStateDto`** — currently just `{ string? LastKnot }`. Extend later.

4. **`JsonSaveGameStore`** — implements `ISaveGameStore`:
   - `SaveAsync(GameState, INarrativeService, string slot)` — serialises to `%APPDATA%\Slums\saves\{slot}.json`.
   - `LoadAsync(string slot)` → returns `SaveEnvelope?`.
   - `ListSlotsAsync()` → returns metadata for all save files.
   - Use `System.Text.Json` with source-generated context for AOT safety.
   - Validate `SaveVersion` on load; reject or migrate mismatched versions.

5. **`ISaveGameStore`** interface in `Slums.Application` (it already has a placeholder comment for it).

6. **`SaveGame` / `LoadGame` use cases** in `Slums.Application`:
   - `SaveGameUseCase.ExecuteAsync(GameState, INarrativeService, string slot)`.
   - `LoadGameUseCase.ExecuteAsync(string slot)` → returns a hydrated `GameState`.

#### `Slums.Game`

7. **Save/Load UI**: add a "Save Game" option to `GameScreen` actions or a pause menu (`P` key). On `MainMenuScreen`, `Load Game` opens a slot-selection screen listing existing saves with their `CheckpointName` and `LastPlayedUtc`.

### Tests to add

- `JsonSaveGameStore` round-trip: save a `GameState`, reload it, assert all fields match.
- Version mismatch rejection test.
- `LoadGameUseCase` with a pre-existing valid JSON fixture.

### Definition of done

- The player can save mid-run from the game screen.
- On next launch, `Load Game` lists the save and restores the exact game state.

---

## GAP-3 — Crime path (stealing, robbery, hashish trade)

**Priority:** High — the primary moral fork in `REQS.md` is honest work vs. crime.

### What is missing

`WorldState` flags `HasCrimeOpportunities = true` on Market and Square but no crime actions, mechanics, or police pressure exist.

### What to build

#### `Slums.Core`

1. **`CrimeType`** enum: `Petty Theft`, `Robbery`, `HashishTrade`.

2. **`CrimeAttempt`** record:
   ```csharp
   record CrimeAttempt(CrimeType Type, int BaseReward, int DetectionRisk,
       int PolicePressureIncrease, int StreetRepRequired, int EnergyCost);
   ```

3. **`CrimeRegistry`** — static definitions for the three crime types, parameterised by district context.

4. **`PolicePressure`** — an `int` (0–100) on `GameState`. Goes up when crimes are committed, goes down slowly each day. When ≥ 80 a `RandomArrest` event can fire.

5. **`CrimeService.AttemptCrime(CrimeAttempt, PlayerCharacter, int policePressure, Random)`**:
   - Returns `CrimeResult` with: success flag, money gained, energy cost, stress cost, police pressure delta, message.
   - Uses `DetectionRisk` and current `PolicePressure` together to calculate detection probability.
   - Detection → increase police pressure; optional "arrested" flag for later expansion.

6. **`GameState.CommitCrime(CrimeAttempt)`** — calls `CrimeService`, applies result, raises `GameEvent`.

7. **End-of-day**: `PolicePressure` decays by 5 per clean day. High pressure events fire via `GameEvent`.

#### `Slums.Game`

8. **`CrimeScreen`** — mirrors `WorkScreen`:
   - Shows available crimes at the current location.
   - Displays reward, risk level (Low/Medium/High), and current police pressure.
   - Enter to attempt; Escape to cancel.

9. **`GameScreen`** — add "Crime" to action list (shown only when `location.HasCrimeOpportunities`).

### Content

10. **`content/ink/crime.ink`** — short reactive scenes for first crime success and first arrest warning. Triggered once per run using a flag check.

### Tests to add

- `CrimeService`: low pressure + low risk → high success rate.
- `CrimeService`: high pressure + high risk → frequent detection.
- `GameState.CommitCrime` applies money, energy, and pressure changes.
- Police pressure decays on `EndDay`.
- `GameOverConditions` does not trigger from crime pressure alone (arrest is a warning, not instant death).

### Definition of done

- At Market or Square the player can choose "Crime" and see available actions.
- Committing a crime yields money but raises police pressure.
- High pressure causes warning events in the event log.
- The Ink reactive scene fires on the first successful crime.

---

## GAP-4 — Recurring NPCs, relationships, and factions

**Priority:** Medium — required by `REQS.md` for a rich, reactive world.

### What is missing

No NPC, relationship, or faction state exists in `GameState`.

### What to build

#### `Slums.Core`

1. **`NpcId`** enum — initial roster:
   - `LandlordHajjMahmoud`, `FixerUmm Karim`, `OfficerKhalid`.

2. **`NpcRelationship`** record: `NpcId`, `Trust` (−100 to 100), `LastSeenDay`.

3. **`FactionId`** enum: `ImbabaCrew`, `DokkiThugs`, `ExPrisonerNetwork`.

4. **`FactionStanding`** record: `FactionId`, `Reputation` (−100 to 100).

5. **`RelationshipState`** on `GameState` — dictionary of NpcId → NpcRelationship and FactionId → FactionStanding.

6. **`RelationshipService`** — `ModifyTrust(NpcId, int delta)`, `ModifyReputation(FactionId, int delta)`. Returns events when thresholds are crossed (e.g. "Hajj Mahmoud distrust: he may raise rent").

#### `Slums.Ink` / `content/ink/npcs.ink`

7. **At least one NPC interaction per recurring character**:
   - `landlord_rent_negotiation` — polite vs. defiant outcomes; trust delta applied via Ink tags.
   - `fixer_first_contact` — introduces crime access; faction reputation gate.
   - `officer_checkpoint` — appears at high police pressure; trust vs. bribe outcome.

#### `Slums.Game`

8. **"Talk" action on `GameScreen`** — shows which NPCs are reachable at the current location. Selecting one triggers the relevant Ink knot via `NarrativeScreen`.

### Tests to add

- `RelationshipService.ModifyTrust` clamps correctly at −100/100.
- Trust threshold crossing raises `GameEvent`.
- Faction reputation change is independent of NPC trust.

### Definition of done

- Landlord, fixer, and officer each have one interactive scene.
- Running the same scene twice shows different text when trust changed.

---

## GAP-5 — Random event framework

**Priority:** Medium — REQS.md requires this; player experience is currently fully deterministic.

### What is missing

No random events fire during time advancement or end-of-day.

### What to build

#### `Slums.Core`

1. **`RandomEvent`** record:
   ```csharp
   record RandomEvent(string Id, string Description, RandomEventEffect Effect,
       int MinDay, int Weight, Predicate<GameState>? Condition);
   ```

2. **`RandomEventEffect`** — same delta fields as `NarrativeOutcome` plus an optional `InkKnot` trigger.

3. **`RandomEventRegistry`** — static list of events (JSON-loadable later). Initial set:
   - `MotherHealthScare` (mother health −10, triggers if mother health < 50, min day 3).
   - `NeighborhoodRumor` (stress +10, triggers at high police pressure).
   - `UnexpectedWork` (+20 money, available at home, low frequency).
   - `PowerCut` (energy −10, available in Imbaba).
   - `GoodNeighbour` (food +1, available at market, random chance).

4. **`RandomEventService.RollDailyEvents(GameState, Random)`** — weighted random draw filtered by `Condition`. Returns up to 2 events per day. `GameState.EndDay()` calls this.

5. **`IRandomSource`** contract in `Slums.Application` — wrap `Random` so tests can inject deterministic sequences. Implement in `Slums.Infrastructure` as `SeededRandomSource`.

#### `Slums.Game`

6. Random events emit `GameEvent` messages which appear in the existing event log — no new screen needed.

7. If an event has `InkKnot != null`, push `GameState` to a queue; after current action resolves, `GameScreen` checks the queue and navigates to `NarrativeScreen`.

### Tests to add

- `RandomEventService` with a seeded `Random` returns deterministic events.
- Condition predicate filters events correctly (e.g. `MotherHealthScare` does not fire when mother is healthy).
- `EndDay` applies event deltas.

### Definition of done

- After several days, random events appear in the event log.
- At least one event can trigger an Ink scene.

---

## GAP-6 — Skills and progression

**Priority:** Medium — REQS.md lists this as a required system.

### What is missing

No skill state, progression, or unlock mechanism exists.

### What to build

#### `Slums.Core`

1. **`SkillId`** enum: `StreetSmarts`, `Physical`, `Persuasion`, `Medical`.

2. **`SkillState`** — `Dictionary<SkillId, int>` (0–10 per skill).

3. Skills on `PlayerCharacter`. Background bonuses applied on `ApplyBackground`:
   - Medical Dropout: +3 Medical.
   - Political Prisoner: +2 Persuasion, +1 StreetSmarts.
   - Sudanese Refugee: +2 StreetSmarts, +1 Physical.

4. **Skill effect hooks**:
   - `Medical ≥ 3` → `BuyMedicine` costs 40 LE instead of 50 (cheaper dosing).
   - `Physical ≥ 3` → Bakery and cleaning jobs reduce energy cost by 5.
   - `StreetSmarts ≥ 3` → Crime detection risk reduced by 10%.
   - `Persuasion ≥ 3` → NPC trust modifiers +5.

5. **`SkillService.ApplySkillGain(SkillId, GameState)`** — small XP-style increment per relevant action. Cap at 10. Fire `GameEvent` when a new level is reached.

#### `Slums.Game`

6. Add a skills row to the HUD (right-side panel) showing current levels.

### Tests to add

- Background application sets correct skill bonuses.
- Skill hook: `Medical ≥ 3` reduces medicine cost in `GameState.BuyMedicine`.
- Skill capping at 10.
- `GameEvent` fires on level-up.

### Definition of done

- Each background starts with correct skill bonuses.
- Performing actions gradually improves relevant skills.
- Skills visibly affect costs and risks.

---

## GAP-7 — Multiple endings

**Priority:** Medium — required by `REQS.md`.

### What is missing

`CheckGameOverConditions` only checks health=0, mother death, and triple-zero destitution. No positive endings or route-based outcomes exist.

### What to build

#### `Slums.Core`

1. **`EndingId`** enum:
   - `Destitution` (existing failure)
   - `MotherDied` (existing failure)
   - `CollapseFromExhaustion` (existing failure)
   - `StabilityHonestWork` — survived 30 days on honest work only, money > 200, mother alive, police pressure < 20.
   - `CrimeKingpin` — total crime earnings > 1,000 LE, faction rep with ImbabaCrew > 50, but high police pressure risk.
   - `QuitTheLuxorDream` — survived 30 days, money > 500, crime committed ≤ 3 times, mother health > 60. Triggers Luxor ending scene.
   - `Arrested` — police pressure reached 100.

2. **`EndingService.CheckEndings(GameState)`** — called from `GameState.CheckGameOverConditions`. Returns the first matching `EndingId?`. Set `GameOverReason` to a Cairo-flavored ending message string.

3. **`GameState`** — add `TotalCrimeEarnings`, `DaysSurvived`, `CrimesCommitted` counters.

#### `content/ink/endings.ink`

4. Ink scene per ending — at minimum `ending_stability`, `ending_luxor`, `ending_arrested`. Failure endings can be plain text.

#### `Slums.Game`

5. On game over, if an ending has an Ink knot, show `NarrativeScreen` first; then return to main menu.

### Tests to add

- `EndingService.CheckEndings` returns `StabilityHonestWork` when criteria met.
- `EndingService.CheckEndings` returns `null` when no criteria met.
- `Arrested` fires when police pressure == 100.
- Counters (`TotalCrimeEarnings`, `CrimesCommitted`) increment correctly.

### Definition of done

- The player can reach and read at least three distinct endings.
- The Luxor ending scene plays through Ink before returning to menu.

---

## GAP-8 — Return home at day start

**Priority:** Low — minor realism/UX issue.

### What is missing

When `AdvanceToNextDay()` is called (from `EndDay()`), the player wakes at whatever location they were in. `RestAtHome()` also requires being at home, but a player who ends the day at Market cannot rest.

### What to build

In `GameState.EndDay()`, after `Clock.AdvanceToNextDay()`, call `World.TravelTo(LocationId.Home)` and raise `GameEvent("You return home for the night.")`. This is one line; no new types required.

### Tests to add

- `EndDay` sets `CurrentLocationId` to `Home`.
- Confirm existing travel tests are not broken.

### Definition of done

- After any `EndDay`, `CurrentLocationId == Home`.

---

## GAP-9 — Content JSON loading from `content/data/`

**Priority:** Low for the vertical slice; required before Phase 6 expansion.

### What is missing

`JobRegistry`, `BackgroundRegistry`, `WorldState` all have hardcoded static data in C#. `content/data/` is empty. `Slums.Infrastructure` is empty.

### What to build

1. **JSON schemas** in `content/data/`:
   - `backgrounds.json`
   - `locations.json`
   - `jobs.json`
   - `random_events.json` (for GAP-5)

2. **`IContentRepository`** in `Slums.Application` — `LoadBackgrounds()`, `LoadLocations()`, `LoadJobs()`, `LoadRandomEvents()`.

3. **`JsonContentRepository`** in `Slums.Infrastructure` — loads from embedded resources or from a configurable base path; uses `System.Text.Json`.

4. **Wire at startup**: `BackgroundRegistry`, `JobRegistry`, `WorldState` receive data from `IContentRepository` rather than hardcoded arrays. Keep the static registries as fallback defaults so tests do not need file I/O.

5. **Copy content files to output** via `<None Include="..." CopyToOutputDirectory="PreserveNewest" />` in `Slums.Game.csproj`.

### Tests to add

- `JsonContentRepository` loads `backgrounds.json` and returns 3 backgrounds matching expected names.
- Missing file returns empty list, not exception.
- Invalid JSON logs error and returns empty list.

### Definition of done

- Removing a location from `locations.json` removes it from the game without recompiling.

---

## Execution order recommendation

| Order | Gap | Reason |
|-------|-----|--------|
| 1 | GAP-8 | One-liner; fixes an active bug; unblocks clean day-cycle testing |
| 2 | GAP-1 | Ink is already wired; connecting it to the UI completes Phase 3 |
| 3 | GAP-2 | Save/load is visible and expected by players; unblocks longer test sessions |
| 4 | GAP-3 | Core gameplay loop; the honest vs. crime fork is the central REQS.md pillar |
| 5 | GAP-5 | Random events make the world feel alive before factions are built |
| 6 | GAP-6 | Skills layer on top of existing job/crime actions |
| 7 | GAP-4 | Recurring NPCs depend on relationship state which is easier after crime + random events exist |
| 8 | GAP-7 | Endings require crime counters and relationship state to be in place |
| 9 | GAP-9 | Content decoupling can be a final polish pass |

---

## Constraints for all gaps

- Follow `AGENTS.md` layer rules. No simulation logic in `Slums.Game`.
- Use `TUnit` + `FluentAssertions` for all new tests.
- Every new public domain method needs at least one unit test.
- Run `dotnet build Slums.slnx` and `dotnet test` before finishing any gap.
- On ARM64 Windows (this machine), launch the game with:  
  `Start-Process "src\Slums.Game\bin\Debug\net10.0\Slums.Game.exe"` (the project is built as `x64`).
