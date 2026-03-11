# Slums Implementation Plan

## Purpose

# Slums Implementation Plan

## Purpose

Build and continue evolving a text-driven RPG about survival in Cairo using SadConsole, Ink, and .NET 10. This document is the high-level implementation map for future agents. It now reflects the current repository state rather than the original bootstrap-only plan.

## Source Of Truth

Before making major changes, read these files in order:

1. `REQS.md`
2. `PLAN.MD`
3. `AGENTS.md`
4. `MAJOR-PLAN.md`

## Technical Direction

- Runtime/UI: SadConsole
- Narrative system: Ink runtime
- Target framework: `net10.0`
- Language version: C# 14
- Persistence: local JSON save files
- Content strategy: authored Ink scripts plus JSON static content
- Architecture style: modular game architecture with inward dependencies

## Current Repository State

The repository is no longer at scaffold or thin-slice stage. It already includes:

- a working multi-project solution
- SadConsole game host and screen flow
- core survival loop with time, rent, food, health, energy, and stress
- background selection and background-specific mechanical identity
- Ink-based intro, NPC, event, crime, and ending scenes
- honest work progression with reliability and lockout behavior
- contact-specific crime routes and contact aftermath scenes
- relationship and faction systems
- NPC memory beyond trust
- district-specific events with event-history-aware gating
- save/load of the expanded simulation and narrative state
- expanded ending logic with multiple survival patterns

## Solution Layout

The current repository is organized as:

```text
slums/
├─ Slums.slnx
├─ global.json
├─ Directory.Build.props
├─ Directory.Packages.props
├─ src/
│  ├─ Slums.Game/               # executable host and SadConsole screens
│  ├─ Slums.Core/               # domain and simulation rules
│  ├─ Slums.Application/        # orchestration and narrative contracts
│  ├─ Slums.Infrastructure/     # saves, content loading, JSON adapters
│  └─ Slums.Narrative.Ink/      # Ink adapter and narrative bridge
├─ tests/
│  ├─ Slums.Core.Tests/
│  ├─ Slums.Application.Tests/
│  ├─ Slums.Infrastructure.Tests/
│  └─ Slums.Narrative.Ink.Tests/
└─ content/
  ├─ ink/
  └─ data/
```

## Dependency Rules

Keep dependencies pointed inward:

```text
Slums.Game -> Slums.Application
Slums.Application -> Slums.Core
Slums.Infrastructure -> Slums.Application
Slums.Infrastructure -> Slums.Core
Slums.Narrative.Ink -> Slums.Application
Slums.Narrative.Ink -> Slums.Core
Slums.Core -> nothing
```

Hard rules:

- `Slums.Core` must not reference SadConsole, Ink, or file system APIs.
- `Slums.Application` must not depend on SadConsole.
- `Slums.Game` must not implement business rules directly.
- `Slums.Narrative.Ink` must not become the home of economy, survival, work, crime, or ending calculations.

## Responsibilities By Project

### `Slums.Game`

Owns:

- app startup
- DI container and composition root
- SadConsole screen navigation
- HUD and action menus
- input handling
- save/load UI

Must not own:

- rent, hunger, crime, work, police, or relationship rules
- direct file persistence logic
- story-branching rules beyond invoking application and narrative services

### `Slums.Core`

Owns pure simulation and state:

- player state
- household state
- money, hunger, energy, health, and stress
- relationships, faction standing, and NPC memory
- work and crime outcome logic
- travel and in-game time
- district and location rules
- random event eligibility and effects
- ending evaluation

Key model roots include:

- `GameState`
- `PlayerCharacter`
- `HouseholdCareState`
- `WorldState`
- `GameClock`
- `RelationshipState`
- `JobProgressState`
- `LocationId`, `DistrictId`, `NpcId`, `FactionId`

### `Slums.Application`

Owns orchestration contracts and narrative mapping surfaces:

- narrative contracts
- content contracts
- persistence contracts
- use-case level extension points

Important contracts include:

- `ISaveGameStore`
- `IContentRepository`
- `INarrativeService`
- `IRandomSource`

### `Slums.Infrastructure`

Owns adapters:

- JSON save and load
- save-slot metadata
- static content loading
- seeded randomness implementation
- environment-specific file paths

### `Slums.Narrative.Ink`

Owns the Ink bridge:

- loading compiled Ink stories
- syncing domain values into Ink variables
- mapping Ink tags and outcomes back into application-facing narrative outcomes
- exposing scene progression safely to the game

## Narrative Integration Pattern

Use Ink for authored scenes and C# for simulation.

### Ink owns

- intros
- NPC conversations
- event scenes
- crime aftermath scenes
- background-specific authored beats
- ending scenes

### C# owns

- recurring bills
- hunger, health, and stress changes
- work and crime success calculations
- relationship memory
- police pressure and faction reputation
- event eligibility and progression
- ending condition evaluation

### Story Bridge Flow

1. The game or application layer decides a scene should trigger.
2. The Ink adapter loads the correct knot.
3. Selected `GameState` values are synchronized into Ink variables.
4. Ink emits text, choices, and tags.
5. The UI renders the scene.
6. Player choice advances the Ink story.
7. The narrative adapter exposes an accumulated `NarrativeOutcome`.
8. The application bridge maps that outcome back into domain changes.

## Save Model

Use a versioned JSON save envelope.

Guidance:

- `GameState` remains the canonical source of truth.
- Persist enough narrative state to resume safely.
- Persist relationship memory, event history, work progression, and route counters.
- Save files should remain human-readable for debugging.
- Keep serialization in `Slums.Infrastructure`.

## Static Content Strategy

Do not hardcode most authored content in C#.

Use JSON in `content/data/` for:

- backgrounds
- locations
- jobs
- random event definitions
- other static world data that benefits from lightweight authoring

Use Ink in `content/ink/` for:

- intros
- NPC scenes
- crime scenes
- event scenes
- ending scenes

## Tooling And Build Baseline

At repo root, keep these current:

- `global.json`
- `Directory.Build.props`
- `Directory.Packages.props`
- `.editorconfig`

Project standards:

- `TargetFramework` = `net10.0`
- `LangVersion` = `14`
- nullable enabled
- warnings treated as errors
- repo-wide shared build settings

## Test Strategy

Use TUnit consistently across all test projects.

### `Slums.Core.Tests`

Prioritize tests for:

- recurring expenses
- stat changes
- time advancement
- honest work progression
- crime availability and outcome logic
- police pressure
- relationship memory and faction updates
- event eligibility and event-history behavior
- ending checks

### `Slums.Application.Tests`

Test:

- orchestration behavior
- save/load workflows at contract boundaries
- narrative trigger selection behavior when applicable

### `Slums.Narrative.Ink.Tests`

Test:

- Ink story loading
- variable synchronization
- choice progression
- narrative outcome tag parsing
- presence of critical knots after content changes

### `Slums.Infrastructure.Tests`

Test:

- JSON round-trips
- save compatibility and expanded state persistence
- invalid content-loading behavior
- content-condition mapping

## Completed Phases

The original execution path has already been completed in substance.

### Completed Phase A: Scaffold And Host

Done.

- solution and projects exist
- shared build files are configured
- SadConsole host runs

### Completed Phase B: Core Survival Loop

Done.

- `GameState`, `GameClock`, travel, household pressure, end-of-day costs, and HUD state all exist

### Completed Phase C: Backgrounds And Early Narrative

Done.

- backgrounds are selectable
- background-based starting identity exists
- Ink intros are wired

### Completed Phase D: Honest Work, Crime, Relationships, Events, Save/Load, Endings

Done.

- honest work progression exists
- crime routes are differentiated
- relationships and memory exist
- district events exist
- save/load persists the expanded model
- endings recognize multiple long-term survival patterns

For a more detailed record of the major expansion pass, see `MAJOR-PLAN.md`.

## Next Execution Priorities

The next implementation work should improve readability, balance, and authored reactivity rather than rebuilding already-completed systems.

### Priority 1: UI Explanation Pass

Objective:

- make the new simulation depth visible and understandable in the UI

Deliverables:

- explain why a job improved or worsened
- explain why a crime route is unlocked, blocked, or hotter than usual
- surface debt, suspicion, and relationship-memory effects more clearly
- improve event-log messaging for background and spillover modifiers

### Priority 2: Balance Pass

Objective:

- tune honest work, crime, background friction, event weights, and endings after the larger systems pass

Deliverables:

- tune pay, stress, pressure, and reliability curves
- review event frequency and district pressure
- review ending thresholds against realistic runs
- add targeted regression tests for balance-sensitive rules

### Priority 3: More Authored Reactivity

Objective:

- expand content so more of the systemic state is reflected in authored scenes

Deliverables:

- more memory-reactive NPC variants
- more repeated-state aftermath scenes
- more district-event follow-ups
- more background-specific authored scenes

## Validation Checklist

Before finishing substantial work:

1. Recompile Ink if any `.ink` file changed.
2. Run `dotnet build .\Slums.slnx` from repo root.
3. Run `dotnet test .\Slums.slnx` from repo root.
4. Confirm no UI logic leaked into `Slums.Core`.
5. Confirm no core simulation logic leaked into `Slums.Game`.

## Non-Goals

Avoid these unless the project direction changes materially:

- database integration
- networking
- mod or plugin systems
- large animation ambitions
- over-engineered ECS-style rewrites
- splitting the repo into many extra projects without strong reason

## Content Constraints

All implementation must continue to respect the current requirements:

- crime remains consequence-heavy
- avoid graphic violence
- avoid child harm
- avoid explicit drug-use depiction
- avoid explicit torture or police-brutality detail
- keep Cairo-specific detail grounded and respectful

## Update Rules

When implementation changes architecture or execution direction materially:

- update `PLAN.MD`
- update `AGENTS.md`
- update `MAJOR-PLAN.md` if roadmap status or next-slice priorities changed
- update `REQS.md` only if the requirements themselves changed

## Source of Truth

Before making changes, read these files in order:

1. `REQS.md` - game requirements, world rules, scope, and content boundaries
2. `PLAN.MD` - implementation phases and architecture plan
3. `AGENTS.md` - execution rules and coding conventions for future agents

## Technical Direction

- **Runtime/UI:** SadConsole
- **Narrative system:** Ink runtime (`ink-engine-runtime`)
- **Target framework:** `net10.0`
- **Language version:** C# 14
- **Persistence:** local JSON save files
- **Content strategy:** authored Ink scripts plus JSON static content
- **Architecture style:** modular game architecture with inward dependencies

## Solution Layout

Create the repository as a single solution with focused projects:

```text
slums/
├─ Slums.sln
├─ global.json
├─ Directory.Build.props
├─ Directory.Build.targets
├─ Directory.Packages.props
├─ .editorconfig
├─ src/
│  ├─ Slums.Game/               # executable host and SadConsole screens
│  ├─ Slums.Core/               # domain and simulation rules
│  ├─ Slums.Application/        # use cases and orchestration
│  ├─ Slums.Infrastructure/     # file IO, JSON saves, content loading
│  └─ Slums.Narrative.Ink/      # Ink adapter and narrative bridge
├─ tests/
│  ├─ Slums.Core.Tests/
│  ├─ Slums.Application.Tests/
│  ├─ Slums.Infrastructure.Tests/
│  └─ Slums.Narrative.Ink.Tests/
└─ content/
   ├─ ink/
   └─ data/
```

## Dependency Rules

Keep dependencies pointed inward:

```text
Slums.Game -> Slums.Application
Slums.Application -> Slums.Core
Slums.Infrastructure -> Slums.Application
Slums.Infrastructure -> Slums.Core
Slums.Narrative.Ink -> Slums.Application
Slums.Narrative.Ink -> Slums.Core
Slums.Core -> nothing
```

Hard rules:

- `Slums.Core` must not reference SadConsole, Ink, or file system APIs.
- `Slums.Application` must not depend on SadConsole.
- `Slums.Game` must not implement business rules directly.
- `Slums.Narrative.Ink` must not become the home of economy or survival calculations.

## Responsibilities by Project

### `Slums.Game`

Owns:

- app startup
- DI container/composition root
- SadConsole screen navigation
- HUD and action menus
- input handling
- save/load UI

Must not own:

- rent, hunger, crime, police, or relationship rules
- direct file persistence logic
- story branching rules beyond invoking use cases

### `Slums.Core`

Owns pure simulation and state:

- player state
- household state
- money, hunger, energy, health, stress
- inventory
- relationships and reputation
- factions
- travel and in-game time
- job and crime outcomes
- ending condition evaluation

Recommended model roots:

- `GameState`
- `PlayerCharacter`
- `HouseholdState`
- `WorldState`
- `GameClock`
- `InventoryState`
- `RelationshipState`
- `FactionState`
- `LocationId`, `DistrictId`, `NpcId`, `FactionId`

### `Slums.Application`

Owns use cases and orchestration:

- `StartNewGame`
- `ChooseBackground`
- `AdvanceTime`
- `TravelToLocation`
- `PerformJobShift`
- `PerformCrimeAction`
- `TalkToNpc`
- `TriggerNarrativeScene`
- `SaveGame`
- `LoadGame`

Owns contracts:

- `ISaveGameStore`
- `IContentRepository`
- `INarrativeService`
- `IRandomSource`
- `IClock`
- `IGameSession`

### `Slums.Infrastructure`

Owns adapters:

- JSON save and load
- save-slot metadata
- content file discovery
- static data loading
- seeded randomness implementation
- environment-specific file paths

### `Slums.Narrative.Ink`

Owns the Ink bridge:

- loading compiled Ink stories
- syncing domain values into Ink variables
- converting story choices into application results
- reading post-scene flags, tags, and outcomes
- exposing safe external functions when needed

## Narrative Integration Pattern

Use **Ink for authored scenes** and **C# for simulation**.

### What Ink should own

- intro sequences
- NPC conversations
- story beats
- moral choice scenes
- route-specific branching
- ending scenes

### What C# should own

- recurring bills
- hunger/health/stress changes
- travel timing
- inventory changes
- success/failure calculations
- police pressure
- faction reputation
- long-term progression and balancing

### Story bridge flow

1. Application decides a scene should trigger.
2. Narrative adapter loads the correct knot/stitch.
3. Selected `GameState` values are pushed into Ink variables.
4. Ink emits text and choices.
5. UI renders the text and choices through SadConsole.
6. Player selects an option.
7. Ink advances and returns narrative output.
8. Application maps resulting flags/tags/variables into domain changes.

## Save Model

Use a versioned JSON save envelope:

```json
{
  "saveVersion": 1,
  "runId": "guid",
  "createdUtc": "timestamp",
  "lastPlayedUtc": "timestamp",
  "checkpointName": "string",
  "gameState": {},
  "narrativeState": {}
}
```

Guidance:

- `GameState` is the canonical source of truth.
- Persist enough Ink state to resume scenes safely.
- Save files should remain human-readable for debugging.
- Keep serialization in `Slums.Infrastructure`.

## Static Content Strategy

Do not hardcode most authored content in C#.

Use JSON in `content/data/` for:

- districts
- locations
- jobs
- items
- factions
- NPC definitions
- random event definitions
- starting backgrounds

Use Ink in `content/ink/` for:

- intros
- recurring character scenes
- route scenes
- endings

## Initial Open-Source Dependencies

Add these first unless implementation reveals a better equivalent:

- `SadConsole`
- `ink-engine-runtime`
- `Microsoft.Extensions.Hosting`
- `Microsoft.Extensions.DependencyInjection`
- `Microsoft.Extensions.Logging`
- `System.Text.Json`
- `xunit`
- `FluentAssertions`
- `NSubstitute` or `Moq`

Prefer central package management via `Directory.Packages.props`.

## Build Baseline

At repo root, establish:

- `global.json` pinned to a .NET 10 SDK
- `Directory.Build.props` with:
  - `TargetFramework` = `net10.0`
  - `LangVersion` = `14`
  - `Nullable` = `enable`
  - `ImplicitUsings` = `enable`
  - `TreatWarningsAsErrors` = `true`
  - `EnforceCodeStyleInBuild` = `true`
  - `AnalysisLevel` = `latest-all`
- `.editorconfig` enforcing file-scoped namespaces and braces

## Testing Strategy

### `Slums.Core.Tests`

Prioritize unit tests for:

- recurring expenses
- stat changes
- time advancement
- honest work outcomes
- crime risk/reward outcomes
- police pressure
- faction and relationship updates
- ending checks

### `Slums.Application.Tests`

Test:

- use-case orchestration
- save/load workflows
- narrative trigger selection
- command failure behavior

### `Slums.Narrative.Ink.Tests`

Test:

- Ink story loading
- variable sync
- choice progression
- narrative outcome mapping

### `Slums.Infrastructure.Tests`

Test:

- JSON round-trips
- versioned save compatibility
- invalid content loading errors

UI rendering tests are optional early on; keep UI logic thin and test use-case outputs instead.

## Implementation Phases

### Phase 0 - Scaffold the solution

Deliverables:

- solution and projects created
- shared .NET 10 build files added
- package management configured
- minimal SadConsole host starts
- all test projects build

Definition of done:

- `dotnet build` passes
- `dotnet test` passes
- executable opens a placeholder main menu

### Phase 1 - Core survival loop

Deliverables:

- `GameState`
- `GameClock`
- player survival stats
- district travel between Dokki, Imbaba, and Ard al-Liwa
- end-of-day expenses
- HUD with money, time, hunger, health, and stress

Definition of done:

- one in-game day can be simulated without narrative integration

### Phase 2 - New game and backgrounds

Deliverables:

- preset background selection
- background-based stat modifiers and starting flags
- widowed mother household dependency
- first content JSON files for backgrounds and districts

Definition of done:

- each background creates a distinct starting state

### Phase 3 - Ink vertical slice

Deliverables:

- Ink runtime integrated
- one intro scene per background
- one landlord scene
- one fixer or police encounter
- domain-to-Ink variable sync
- Ink choices updating domain state

Definition of done:

- a full story interaction loop works end-to-end in SadConsole

### Phase 4 - Honest work path

Deliverables:

- house cleaning
- bakery work
- call center work
- pay, exhaustion, schedule, and consequence logic

Definition of done:

- the player can survive at least short-term through honest work alone

### Phase 5 - Crime path and police pressure

Deliverables:

- stealing
- robbery
- selling hashish through implication-only handling
- no-snitching social rule effects
- police pressure model
- rival faction responses

Definition of done:

- the crime route is playable and consequence-heavy without glamorization

### Phase 6 - Relationships, factions, and random events

Deliverables:

- recurring NPC tracking
- relationship state
- faction reputation
- random event framework
- more district-specific world content

Definition of done:

- repeated interactions visibly change future choices and risks

### Phase 7 - Save/load and content growth

Deliverables:

- save slots
- narrative state persistence
- more authored scenes
- JSON-backed content expansion

Definition of done:

- save/load restores both simulation and narrative continuity

### Phase 8 - Endings and balancing

Deliverables:

- multiple endings
- Luxor family ending
- decline/stability/respect outcomes
- balancing pass over jobs, crime, rent, and survival pressure

Definition of done:

- a complete first playable campaign exists

## Ordered Starter Backlog

The first implementation agent should follow this order:

1. Create `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`
2. Create the solution and all projects
3. Wire DI and logging
4. Launch a minimal SadConsole app with a main menu
5. Add `GameState`, `GameClock`, and core stat models
6. Add travel and end-of-day expenses
7. Add background selection
8. Add content loaders for JSON data
9. Integrate Ink with a single `main.ink`
10. Implement one narrative scene that round-trips state

## Non-Goals for Early Phases

Avoid these before the vertical slice is working:

- database integration
- networking
- mod/plugin systems
- large animation or graphical ambitions
- over-engineered ECS-style abstraction
- dozens of micro-projects

## Content Constraints

All implementation must respect the current requirements:

- crime must remain consequence-heavy
- avoid graphic violence
- avoid child harm
- avoid explicit drug use depiction
- avoid explicit torture or police brutality detail
- keep Cairo-specific detail grounded and respectful

## Update Rules

When implementation changes architecture materially:

- update `PLAN.MD`
- update `AGENTS.md`
- update `REQS.md` only if requirements themselves change

