# AGENTS.md

## Mission

Build a **.NET 10** text-driven RPG set in Cairo using **SadConsole** for presentation and **Ink** for narrative scenes. Future agents should optimize for a playable vertical slice first, then expand the world through content and well-tested simulation rules.

## Read This First

Before coding, read:

1. `REQS.md`
2. `PLAN.MD`
3. `AGENTS.md`
4. `MEMORY.MD`

Do not start implementation from memory alone.

## Non-Negotiable Technical Decisions

- Target framework: **`net10.0`**
- C# language version: **14**
- UI/runtime: **SadConsole**
- Narrative runtime: **Ink**
- Persistence: **JSON save files**
- Static world data: **JSON in `content/data/`**
- Authored story content: **Ink files in `content/ink/`**

Do not replace SadConsole or Ink without explicitly updating both `PLAN.MD` and this file.

## Required Repository Layout

Implementation should create and preserve this structure:

```text
src/
  Slums.Game/
  Slums.Core/
  Slums.Application/
  Slums.Infrastructure/
  Slums.Narrative.Ink/
tests/
  Slums.Core.Tests/
  Slums.Application.Tests/
  Slums.Game.Tests/
  Slums.Infrastructure.Tests/
  Slums.Narrative.Ink.Tests/
content/
  ink/
  data/
```

## Dependency Discipline

Keep dependencies pointed inward:

- `Slums.Core` references nothing project-local.
- `Slums.Application` references `Slums.Core`.
- `Slums.Infrastructure` implements application contracts.
- `Slums.Narrative.Ink` adapts Ink to application contracts.
- `Slums.Game` handles SadConsole and invokes application use cases.

Never put business rules directly in the UI project.

## What Goes Where

### `Slums.Core`

Put these here:

- the canonical `GameSession` runtime boundary backed by EntitiesDb
- survival rules
- time progression
- money/rent/food logic
- honest work outcomes
- crime route outcomes
- police pressure
- faction reputation
- ending checks

Do not put SadConsole, Ink, or file IO here.

### `Slums.Application`

Put orchestration here:

- contracts that operate on `GameSession`
- player-facing action commands and menu/action availability queries
- new game flow
- background selection
- travel commands
- activity commands
- scene triggering
- save/load use cases, including `LoadedGameSession` handoff

### `Slums.Infrastructure`

Put adapters here:

- save file storage
- `GameSession` snapshot capture/restore and JSON serialization
- content loading
- fail-fast validation for repo-owned JSON content
- environment-specific paths
- seeded randomness implementation
- `LoadedGameSession` creation at the persistence boundary

### `Slums.Narrative.Ink`

Put these here:

- Ink story loading
- variable synchronization
- choice advancement
- mapping narrative outcomes back to application/domain concepts
- fail-fast story loading when Ink content is missing or invalid

Do not move core simulation rules into Ink scripts.
Do not add a fallback narrative runtime that hides Ink failures.

### `Slums.Game`

Put these here:

- screen classes
- menus
- HUD
- input mapping
- state display
- app startup and dependency wiring

## Coding Standards

Follow these defaults:

- one type per file
- file-scoped namespaces
- explicit access modifiers
- PascalCase for types and members
- `_camelCase` private fields
- braces on all control flow
- nullable enabled
- warnings treated as errors
- XML docs on public APIs that matter

Prefer simple, testable classes over deep inheritance.

## Build and Tooling Rules

At repo root, keep these files current:

- `global.json`
- `Directory.Build.props`
- `Directory.Packages.props`
- `.editorconfig`

Use central package management. Prefer repo-wide shared settings instead of duplicating build properties in each project.

## Platform and Architecture Notes

This project uses SadConsole with the SFML host, which depends on CSFML native libraries. Be aware of the following when switching between environments:

**Windows (PowerShell/CMD):**
- Builds and runs without issues on x86-64 Windows
- CSFML native libraries are correctly resolved

**Windows on ARM64:**
- `Slums.Game` must run against the x64 .NET host because CSFML does not provide ARM64 Windows binaries in this setup
- The game project defaults to `RuntimeIdentifier=win-x64` and `PlatformTarget=x64` when `PROCESSOR_ARCHITECTURE=ARM64`
- Use `dotnet run --project .\src\Slums.Game\Slums.Game.csproj` from the repo root or `dotnet run` from `src/Slums.Game`; both should resolve to the x64 runtime automatically

**WSL Linux on ARM64 (e.g., Apple Silicon with Parallels, Snapdragon Windows):**
- Build succeeds but may produce warning `NETSDK1206` about `fedora-x64` runtime identifiers
- This is a warning only and does not prevent building or running tests
- The game itself may fail to launch due to missing ARM64 native libraries for CSFML
- Tests that don't involve SadConsole UI will run correctly

**Compiling Ink Files:**
- `inklecate` binaries from inkle are x86-64 only
- On ARM64 Linux, you cannot run `inklecate` directly to compile `.ink` files to `.json`
- Use the local `inkjs` compiler instead: from `src/Slums.Game`, run `npm run compile-ink`
- The compiled output is `content/ink/main.json`; if you modify `.ink` source files, regenerate it before building or testing

## Testing Expectations

Every rule change should come with comprehensive tests using **TUnit** as the testing framework. Comprehensive unit tests are mandatory for all layers to ensure reliability and enable safe refactoring.

- Test framework: **TUnit** (latest version)
- Assertion library: **FluentAssertions**
- Mocking library: **NSubstitute**

**All test projects must use TUnit consistently.** Do not mix xUnit or NUnit with TUnit.

Minimum expectation by layer:

- `Slums.Core.Tests`: unit tests for rules and state transitions
- `Slums.Application.Tests`: use-case orchestration tests
- `Slums.Game.Tests`: UI-shell and input-helper tests that do not belong in application/domain test assemblies
- `Slums.Narrative.Ink.Tests`: scene loading, variable sync, and choice progression
- `Slums.Infrastructure.Tests`: serialization and content-loading tests

**Running Tests:**
- Use this validation workflow from the repo root:
  1. `dotnet build Slums.slnx`
  2. `dotnet run --project tests/Slums.Core.Tests`
  3. `dotnet run --project tests/Slums.Application.Tests`
  4. `dotnet run --project tests/Slums.Game.Tests`
  5. `dotnet run --project tests/Slums.Infrastructure.Tests`
  6. `dotnet run --project tests/Slums.Narrative.Ink.Tests`
- Test projects are configured as executables (`OutputType=Exe`) for direct execution

Run existing build and test commands before finishing work.

## Vertical Slice Priority

Implement in this order unless the plan is updated:

1. solution scaffold
2. basic SadConsole host
3. core survival state and time loop
4. background selection
5. one Ink narrative slice
6. honest work loop
7. crime loop with police pressure
8. save/load
9. expanded content and endings

Prefer finishing one thin playable slice over partially building many systems.

## Content and Writing Constraints

Respect the requirements file:

- keep the setting grounded in Cairo
- keep the tone gritty and realistic
- keep crime consequence-heavy
- do not glamorize harmful activity
- avoid graphic violence
- avoid child harm
- avoid explicit drug use depiction
- avoid explicit torture or police brutality detail

If content touches those boundaries, choose implication and consequence over explicit detail.

## Working Rules for Future Agents

- Read existing files before creating new abstractions.
- Reuse models and services instead of duplicating state.
- Prefer feature-oriented folders within each project.
- Keep `GameSession` as the canonical runtime boundary, with EntitiesDb as its backing runtime store.
- When `GameSession` starts collecting orchestration-heavy logic, extract focused core planners/evaluators/calculators and keep the session as the state owner and integration surface.
- Keep shared narrative signal rules and scene-trigger catalogs in `Slums.Core` when both `GameSession` and application queries need the same logic.
- Route player-triggered gameplay mutations through `Slums.Application` commands/queries instead of calling `GameSession` directly from SadConsole screens.
- Keep persistence centered on `GameSession` snapshots and `LoadedGameSession`; do not reintroduce parallel save-state models.
- Use Ink for authored branching scenes, not for core economy simulation.
- Treat missing or invalid Ink content as a hard failure; do not restore fallback narrative behavior.
- Treat missing or invalid repo-owned JSON content as a hard failure during app bootstrap; do not silently fall back in normal runtime.
- Add content in `content/` before hardcoding large world datasets.
- Update `PLAN.MD` and `AGENTS.md` if architectural direction changes.

## Completion Checklist

Before ending an implementation task:

1. build succeeds
2. relevant tests pass
3. docs are updated if architecture or workflow changed
4. no UI-specific logic leaked into domain projects
5. no simulation-specific logic leaked into the SadConsole project
