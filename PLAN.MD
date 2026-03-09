# Slums Implementation Plan

## Purpose

Build a text-driven RPG about survival in Cairo using **SadConsole + Ink** on **.NET 10**. This plan is written for future implementation agents and should be treated as the working execution map unless a later architectural decision explicitly replaces it.

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

