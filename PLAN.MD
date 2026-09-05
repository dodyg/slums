# Slums Plan

## Purpose

This file is the current execution roadmap. `REQS.md` defines the product requirements, `AGENTS.md` defines repository rules, and `MEMORY.MD` describes the implemented architecture and simulation. Completed implementation histories belong in Git, not in this plan.

## Current State

The repository contains a playable .NET 10 vertical slice with:

- SadConsole screens for identity and background selection, travel, work, crime, household care, relationships, phone contacts, community activity, training, investments, saving, loading, and endings
- Ink-driven introductions, recurring conversations, system scenes, route consequences, and ending epilogues
- a survival loop covering time, nutrition, sleep, rent, household care, health, energy, stress, weather, calendar rhythms, and holidays
- 13 locations across six Cairo districts, with JSON-authored locations, 13 JSON-authored job definitions, and a robotics workshop catalog
- honest-work progression, mistake risk, lockouts, gender/background modifiers, and location-specific job variants
- consequence-heavy crime routes, district heat, territory tension, faction reputation, rumors, tips, and police pressure
- 13 recurring NPC relationships with four distinct authored scenes per context, phone messages, community events, debt, NPC economy, and small-business investments
- JSON snapshot persistence around `GameSession` and `LoadedGameSession`
- persistent second-hand robotics: three purchasable machine models, spare-part stock, workshop repairs, and scavenging-job wear/recovery
- a nine-skill progression model with shared metadata; `RobotRepair` and `CyberHacking` are presented as Technical Repair and Digital Literacy for save compatibility
- reactive city news with source reliability, district effects, one-shot player responses, and event-journal entries
- infrastructure service state for electricity, water, transport, and clinic medicine, with gameplay impacts and snapshot persistence
- JSON-authored inventory items and NPC schedule windows used by response actions, status views, and talk availability
- eight ending identifiers: four failure outcomes and four good or ambiguous long-term outcomes, with threshold-gated player choices for the latter
- visible morning survival forecasts and one-time background-specific emergency support during the first week
- a research-grounded 2060 climate baseline expressed through hotter seasonal weather, water and power rotations, climate-stressed food supply, heat-aware work, and community-run adaptation
- structured mutation diagnostics and executable TUnit suites for every project layer
- validated Ink gameplay globals with state-reactive branching and typed multi-flag outcomes
- narrative outcomes applied through an application command and the canonical `GameSession` mutation boundary, including immediate failure-ending checks
- ending-knot catalog validation that rejects missing or orphaned compiled ending scenes
- a shared 30-day crisis planner with delayed callbacks and five ordered central-character arcs, each backed by typed decisions and persisted beat history
- a typed technology-obligation ledger for handset exposure, cooperative storage maintenance, transit scrutiny, biometric appeals, telemedicine use, and bounded allocation confidence
- two-stage ending commitments: pending final scenes persist before the sacrifice choice, then existing ending IDs and epilogues remain authoritative

Repo-owned JSON and Ink content are authoritative and fail fast during bootstrap. There is no normal-runtime fallback narrative or content catalog.

## Design Direction

### Core Loop

The intended daily rhythm is:

1. assess survival, household, district, and relationship pressure
2. choose travel and one or more time-consuming actions
3. accept tradeoffs among money, energy, stress, trust, heat, and future access
4. resolve the night atomically at the 22:00 boundary
5. encounter authored or systemic consequences
6. pursue stability, community shelter, escape, criminal power, or collapse

### Pacing

- Days 1-7 teach survival and establish recurring contacts.
- Days 8-20 make work progression, debt, faction pressure, and household strain visible.
- Days 21-35 should produce route commitment and credible ending pressure.
- Longer runs remain supported; recurring dialogue contexts use smaller authored pools and reject duplicate normalized scene bodies in content validation.

### Economy

- Early play should be tight but recoverable.
- Honest work should become more reliable through progression without trivializing survival.
- Crime may accelerate income but must accumulate heat, stress, faction obligations, and social consequences.
- Debt and investments should create choices, not guaranteed rescue or unavoidable failure.

### Endings

`EndingService` is authoritative. The eight ending identifiers are:

- failure: `MotherDied`, `Destitution`, `Arrested`, `Eviction`
- good or ambiguous: `StabilityHonestWork`, `NetworkShelter`, `QuitTheLuxorDream`, `CrimeKingpin`

Critical loan-shark debt contributes to `Destitution`; it is not a separate `DebtViolence` ending. Ink may vary epilogues by background, gender, and strongest support contact without multiplying domain ending identifiers.

## Architecture Guardrails

- End-of-day resolution is entered through the session-owned `EndOfDayPipeline`, which orchestrates the `DailyResolution` collaborator steps (`DailyStatResolution`, `DailyWorldResolution`, `DailyEconomyResolution`, `DailyInformationResolution`) in a fixed order that preserves the historical mutation and RNG sequence.

- `GameSession` remains the canonical runtime state owner and integration boundary.
- Core queries must be side-effect free. Commands must enforce their own guards even when the UI also filters unavailable actions.
- Actions that cross 22:00 must resolve their action effects against the original context before nightly reset, then end at home on the next day.
- Shared domain rules belong in focused calculators, planners, evaluators, or policies under `Slums.Core`; they must not be duplicated in screens.
- `Slums.Application` owns player-facing commands and context queries.
- `Slums.Game` owns rendering, input, composition, and `ScreenTransition` navigation only.
- Persistence remains centered on `GameSession` snapshots and `LoadedGameSession`.
- Completed Ink outcomes must pass through `ApplyNarrativeOutcomeCommand` and `GameSession.ApplyNarrativeOutcome`; SadConsole screens do not mutate narrative effects directly.
- `Slums.Infrastructure` validates repo-owned JSON and must reject incomplete or contradictory catalogs when they can invalidate runtime behavior.
- `Slums.Narrative.Ink` synchronizes state and applies narrative outcomes; it does not own survival or economy rules.
- Prefer small extractions from `GameSession` when a cohesive rule can be tested independently. Do not create parallel state models.

## Current Priorities

### 1. Playable-run acceptance pass

Run several seeded playthroughs through day 35 across both identities and all three backgrounds. Record dead ends, inaccessible actions, economy traps, ending reachability, and confusing feedback. Convert each reproducible issue into an automated regression test before changing rules.

Acceptance targets:

- every JSON-authored job is reachable at its intended location
- honest and criminal routes can both survive the first week with informed choices
- at least one failure and one non-failure ending can be reached through normal play
- save/load preserves the same next-day and ending behavior as an uninterrupted run
- all global action blocks expose a reason in menu contexts and are enforced again by commands

### 2. Balance and visibility

- audit pay, food, rent, clinic, transport, debt, investment, news, and infrastructure values as one economy
- make schedule, weather, district condition, territory, relationship, news, and service modifiers visible before commitment
- keep preview values and committed outcomes on the same calculation path
- verify walking and paid travel remain meaningful alternatives

### 3. Skill expansion vertical slices

- ship Provisioning, Community Organizing, and Composure with reachable evening training, shared threshold metadata, diagnostics, previews, and focused tests
- broaden Technical Repair and Digital Literacy beyond their original robotics/crime applications while preserving `RobotRepair` and `CyberHacking` snapshot keys
- validate food, community, repair, digital-service, and pressure actions with preview/commit parity and at least one credible early-game route each

### 4. Architecture hardening

- continue extracting cohesive daily-resolution and action-policy rules from `GameSession`; entertainment, household asset, training, home, food-shop, travel, clinic, community, debt, investment, phone, tip, work, crime, random-event, calendar, district-condition, territory-event, and ending policy now live in focused core services
- route snapshot hydration through one validated `GameSession.RestoreFromSnapshot` boundary while keeping `LoadedGameSession` as the persistence handoff
- keep query builders immutable and free of event emission or state mutation
- add catalog-level validation for cross-file assumptions where a valid individual JSON file can still create an invalid world
- reduce static registry coupling only when doing so preserves the canonical session boundary and materially improves test isolation

### 5. Authored reactivity

- add selected Ink follow-ups to high-impact news flashes after the systemic responses have been playtested
- audit recurring dialogue contexts for distinct authored scenes and reactive follow-ups
- expand route-specific aftermath for debt, work mistakes, district heat, household care, and leaving crime
- keep gender and background reactions grounded and consequential without turning them into deterministic success/failure choices
- preserve the established content boundaries: implication and consequence over graphic detail

### 6. UI clarity and accessibility

- keep layout tests around all fixed-size screens
- ensure disabled actions show concise reasons rather than disappearing when the player needs an explanation
- retain keyboard-only operation and consistent back-navigation suppression
- use `FadeTo` for forward navigation and `SwitchTo` for returns

## Content Boundaries

- Large static catalogs belong in `content/data/`.
- Authored branching scenes belong in `content/ink/` and must be compiled to `content/ink/main.json` after source edits.
- C# owns deterministic simulation rules, action guards, calculations, and ending conditions.
- Ink owns prose, choices, and tagged narrative outcomes.
- Cairo-specific writing should remain grounded in Cairo 2060 through a realistic Afrofuturist lens: mature but unequal digital technology, local ingenuity, African cultural continuity, and older infrastructure coexist without turning the setting into generic cyberpunk. Content must remain consistent with the safety constraints in `REQS.md` and `AGENTS.md`.

## Validation

From the repository root:

```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

When Ink source changes, first run `npm run compile-ink` from `src/Slums.Game` and commit the regenerated `content/ink/main.json`.

## Update Rules

- Keep this plan short and forward-looking.
- Update `Current State` when implementation changes materially.
- Remove completed tickets instead of appending historical phase logs.
- Update `REQS.md` for product-direction changes.
- Update `AGENTS.md` for repository or architecture-rule changes.
- Update `MEMORY.MD` when the implemented architecture or simulation changes.
