# Slums Major Expansion Status And Next Roadmap

## Purpose

This document is no longer a speculative handoff plan for the first major expansion pass. The expansion phases it originally described have now been implemented. This file should act as:

- a status snapshot of what the project currently supports
- a record of what the major expansion added
- a guide for the next meaningful slices of work

## Read First

Before making major changes, read these files in order:

1. `REQS.md`
2. `PLAN.MD`
3. `AGENTS.md`
4. `MAJOR-PLAN.md`

## Non-Negotiable Constraints

- Keep core simulation logic in `Slums.Core`.
- Keep authored scene content in `content/ink/` and compile to `content/ink/main.json`.
- Keep static world content in `content/data/` where practical.
- Do not move economy, survival, work, crime, or ending logic into Ink.
- Preserve the current dependency flow.
- Every rules change requires tests.

## Current State Snapshot

The repo now supports:

- travel across Imbaba, Dokki, and Ard al-Liwa
- multiple honest work tracks with progression, reliability, and temporary lockouts
- recurring NPC conversations with trust-based and memory-based knot selection
- contact-specific crime opportunities with distinct outcomes and aftermath scenes
- district-specific random events with event-history-aware eligibility
- work-crime spillover rules
- background-specific systemic advantages and vulnerabilities
- expanded ending logic based on behavior patterns, not only simple thresholds
- persistence for the expanded relationship, work, and event-history state

## Expansion Phases Completed

### Phase 1: Distinct Crime Opportunities

Implemented.

What landed:

- Hanan unlocks a market fencing route.
- Youssef unlocks a Dokki drop route.
- Umm Karim unlocks a higher-risk network errand.
- Crime availability now changes by district, trust, faction standing, and some route context.
- Route-specific first-success, detected-success, and failure scenes were added.

Primary implementation areas:

- `src/Slums.Core/Crimes/*`
- `src/Slums.Core/State/GameState.cs`
- `content/ink/crime.ink`

### Phase 2: Honest Work Progression

Implemented.

What landed:

- Honest jobs now behave like tracks instead of flat repeated actions.
- Reliability and shift history are persisted.
- Better work variants unlock by trust, skills, background, and reliability.
- Mistakes can reduce reliability and create temporary lockouts.

Primary implementation areas:

- `src/Slums.Core/Jobs/*`
- `src/Slums.Core/State/GameState.cs`
- `src/Slums.Infrastructure/Persistence/GameStateDto.cs`

### Phase 3: NPC Memory Beyond Trust

Implemented.

What landed:

- NPC relationships now track more than trust.
- The state includes selective memory such as favors, refusals, unpaid debt, embarrassment, help, and recent contact count.
- Conversation routing now uses these facts for several NPCs.
- Save/load preserves the new relationship memory.

Primary implementation areas:

- `src/Slums.Core/Relationships/*`
- `src/Slums.Infrastructure/Persistence/*`
- `content/ink/npcs.ink`

### Phase 4: District Identity Through Events

Implemented.

What landed:

- Imbaba, Dokki, and Ard al-Liwa now have stronger event identity.
- District events now cover solidarity, checkpoint pressure, supply shortages, transport friction, and localized work instability.
- Event history is recorded and can gate later events.

Primary implementation areas:

- `src/Slums.Core/Events/*`
- `src/Slums.Infrastructure/Content/*`
- `content/data/random_events.json`
- `content/ink/events.ink`

### Phase 5: Work-Crime Spillover

Implemented.

What landed:

- Recent crime heat can make public-facing work harder.
- Honest work can create a same-day alibi effect for some crime routes.
- Some NPCs react differently when the player is trying to maintain both honest work and criminal activity.

Primary implementation areas:

- `src/Slums.Core/State/GameState.cs`
- `src/Slums.Core/Jobs/JobService.cs`
- `src/Slums.Core/Relationships/NpcRegistry.cs`
- `content/ink/npcs.ink`

### Phase 6: Background-Specific Routes

Implemented.

What landed:

- Medical dropout now gains stronger clinic and medicine-related advantages, plus a matching emotional burden around the mother's health.
- Released political prisoner now carries slower police-pressure decay, stronger police scrutiny, and deeper access to ex-prisoner network style escalation.
- Sudanese refugee now gets aid-network support in some systems, but also more friction in Dokki and some work contexts.
- Background-specific scenes were added.

Primary implementation areas:

- `src/Slums.Core/Characters/*`
- `src/Slums.Core/State/GameState.cs`
- `content/ink/main.ink`

### Phase 7: Expanded Ending Logic

Implemented.

What landed:

- Endings now recognize social-network investment, leaving crime after entering it, and being buried by heat rather than only coarse resource thresholds.
- Additional ending knots were added.

Primary implementation areas:

- `src/Slums.Core/Endings/*`
- `content/ink/endings.ink`

## Validation Baseline

Current validation commands:

1. `npm run compile-ink` from `src/Slums.Game`
2. `dotnet build .\Slums.slnx` from repo root
3. `dotnet test .\Slums.slnx` from repo root

The major expansion pass completed with the solution building and all tests passing.

## Recommended Next Work

The next work should not repeat the expansion phases above. The high-value follow-up slices are below.

### Next Slice 1: UI Clarity And Surfacing

Objective:

- make the new systemic depth legible to the player

Suggested deliverables:

- show why a job shifted to a better or worse variant
- show why a crime route is unlocked, blocked, or hotter than usual
- surface employer suspicion, debt states, and route consequences in menus or logs
- surface district-event pressure more clearly in the HUD or event log

Likely files:

- `src/Slums.Game/Screens/*`
- `src/Slums.Core/State/GameState.cs`

### Next Slice 2: Balance Pass

Objective:

- tune the expanded systems so honest work, crime, and background differences remain meaningful without becoming dominant traps or exploits

Suggested deliverables:

- review pay, pressure, stress, and lockout values
- review district event weights and recurrence
- review ending thresholds against realistic run outcomes
- add targeted regression tests for balance-sensitive breakpoints

Likely files:

- `src/Slums.Core/Jobs/*`
- `src/Slums.Core/Crimes/*`
- `src/Slums.Core/Events/*`
- `src/Slums.Core/Endings/*`
- `content/data/random_events.json`

### Next Slice 3: More Authored Reactivity

Objective:

- expand the amount of authored content that reflects the new memory and district systems

Suggested deliverables:

- add more memory-reactive variants for Mona, Umm Karim, Hanan, and Youssef
- add more follow-up scenes tied to repeated spillover states
- add more district-event aftermath scenes and background-specific narration

Likely files:

- `content/ink/npcs.ink`
- `content/ink/crime.ink`
- `content/ink/events.ink`
- `content/ink/main.ink`

## Suggested First Ticket

If only one next feature should be implemented, do this:

### UI Explanation Pass For Work And Crime Menus

Deliver:

- visible reason strings for why a job variant changed
- visible gating reason strings for why a crime route is present or absent
- event log text that makes background and spillover modifiers easier to understand

Why this first:

- the underlying simulation is now much denser
- the next biggest quality gain is helping the player read the systems correctly
- this improves usability without forcing another major state-model rewrite

## Concrete Implementation Plan: UI Surfacing Pass

Execute this work in order. Treat each step as a self-contained slice with focused tests before moving on.

### Step 1: Work And Crime Explanation Pass

Objective:

- make existing work and crime mechanics legible without changing their rules

Deliver:

- extend the work-menu query model with explanation fields for current variant, active modifiers, and next unlock hint
- extend the crime-menu query model with explanation fields for effective risk, effective pressure change, and active route modifiers
- update the work and crime screens to show a selected-item detail panel instead of only a flat list
- keep rule calculation in `Slums.Core` or `Slums.Application`, not in `Slums.Game`

Primary files:

- `src/Slums.Application/Activities/*`
- `src/Slums.Core/Jobs/*`
- `src/Slums.Core/Crimes/*`
- `src/Slums.Core/State/GameState.cs`
- `src/Slums.Game/Screens/WorkScreen.cs`
- `src/Slums.Game/Screens/CrimeScreen.cs`

Validation:

- add or expand `WorkMenuStatusQueryTests`
- add or expand `CrimeMenuStatusQueryTests`
- run targeted application tests before proceeding

### Step 2: Main HUD Status Pages

Objective:

- expose persistent mechanics that currently exist only in state and logs

Deliver:

- add switchable HUD/status pages on the main game screen
- keep the default page focused on immediate survival stats
- add a skills page showing all current skill levels
- add a network page showing faction standing and key NPC trust summaries
- add a progress page showing day count, honest/crime counters, and ending-relevant trajectory data

Primary files:

- `src/Slums.Game/Screens/GameScreen.cs`
- `src/Slums.Core/State/GameState.cs`
- optional small UI helper types under `src/Slums.Game/Screens/`

Validation:

- add focused tests for any new non-UI formatting helpers or query helpers
- run the relevant test projects after the slice

### Step 3: Relationship Detail Pass

Objective:

- surface the relationship-memory system that currently drives scene routing invisibly

Deliver:

- expand the talk screen to show memory flags such as debt, embarrassment, help, and recent contact
- show faction linkage or scene-state hints where that improves player understanding
- keep authored knot names hidden; present player-facing summaries instead

Primary files:

- `src/Slums.Game/Screens/TalkScreen.cs`
- `src/Slums.Core/Relationships/*`
- `src/Slums.Application/*` if a formatting/query helper is needed

Validation:

- add tests for any new helper/query logic
- regression-check existing narrative routing tests if needed

### Step 4: Final Integration And Validation

Objective:

- confirm the UI surfacing pass works as a cohesive whole

Deliver:

- do a final readability pass on game-screen hints and keybind text
- ensure the 80x25 layout remains usable on each updated screen
- update this file if implementation differs materially from the plan above

Validation:

1. `npm run compile-ink` from `src/Slums.Game` if any Ink content changes
2. `dotnet build .\Slums.slnx` from repo root
3. `dotnet test .\Slums.slnx` from repo root

## Notes For The Next Agent

- Do not re-implement the seven expansion phases described above. They are already present.
- Prefer small vertical improvements over another large speculative systems pass.
- Reuse the expanded relationship-memory, event-history, job-progress, and ending infrastructure already in place.
- Keep authored consequence scenes grounded and implication-heavy.
- Do not glamorize crime. The route should remain materially useful, morally corrosive, and increasingly dangerous.
- If you extend saved state again, add persistence tests immediately.