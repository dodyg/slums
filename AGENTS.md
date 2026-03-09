# AGENTS.md

## Mission

Build a **.NET 10** text-driven RPG set in Cairo using **SadConsole** for presentation and **Ink** for narrative scenes. Future agents should optimize for a playable vertical slice first, then expand the world through content and well-tested simulation rules.

## Read This First

Before coding, read:

1. `REQS.md`
2. `PLAN.MD`
3. `AGENTS.md`

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

- new game flow
- background selection
- travel commands
- activity commands
- scene triggering
- save/load use cases

### `Slums.Infrastructure`

Put adapters here:

- save file storage
- JSON serialization
- content loading
- environment-specific paths
- seeded randomness implementation

### `Slums.Narrative.Ink`

Put these here:

- Ink story loading
- variable synchronization
- choice advancement
- mapping narrative outcomes back to application/domain concepts

Do not move core simulation rules into Ink scripts.

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

## Testing Expectations

Every rule change should come with tests in the appropriate project.

Minimum expectation by layer:

- `Slums.Core.Tests`: unit tests for rules and state transitions
- `Slums.Application.Tests`: use-case orchestration tests
- `Slums.Narrative.Ink.Tests`: scene loading, variable sync, and choice progression
- `Slums.Infrastructure.Tests`: serialization and content-loading tests

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
- Keep `GameState` as the canonical source of truth.
- Use Ink for authored branching scenes, not for core economy simulation.
- Add content in `content/` before hardcoding large world datasets.
- Update `PLAN.MD` and `AGENTS.md` if architectural direction changes.

## Completion Checklist

Before ending an implementation task:

1. build succeeds
2. relevant tests pass
3. docs are updated if architecture or workflow changed
4. no UI-specific logic leaked into domain projects
5. no simulation-specific logic leaked into the SadConsole project

