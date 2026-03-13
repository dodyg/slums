# Slums Plan

## Purpose

This is the single active planning document for the repository.

It should stay short and current. Historical expansion notes and old gap lists are archived elsewhere and should not be treated as the live roadmap.

## Read First

Before making major changes, read these files in order:

1. `REQS.md`
2. `PLAN.MD`
3. `AGENTS.md`

## Current State

The game is already past the initial vertical-slice stage. The current repo supports:

- a working multi-project .NET 10 solution with shared build settings
- SadConsole game flow with main menu, background selection, narrative, and gameplay screens
- core survival simulation for money, hunger, energy, health, stress, rent, food, and time
- three playable backgrounds with distinct systemic advantages and liabilities
- travel across Imbaba, Dokki, and Ard al-Liwa
- honest work progression with reliability, variant unlocks, and temporary lockouts
- crime routes with contact-specific access, risk, aftermath, and police pressure behavior
- recurring NPC interactions driven by trust, faction standing, and relationship memory
- conversation pool system with 100+ unique dialogues per NPC context to prevent repeats for 100 days
- district-specific random events with event-history-aware gating
- save/load for the expanded simulation and narrative state
- multiple ending paths based on long-term behavior, pressure, and survival trajectory

## Architecture Guardrails

- `Slums.Core` owns simulation rules and state. It must not depend on SadConsole, Ink, or file IO.
- `Slums.Application` owns contracts and orchestration. It must not depend on SadConsole.
- `Slums.Infrastructure` owns JSON persistence, static content loading, and environment-specific adapters.
- `Slums.Narrative.Ink` owns Ink loading, variable sync, and narrative outcome mapping.
- `Slums.Game` owns screen flow, HUDs, menus, and input. It must not become the home of business rules.

Dependency flow must remain inward:

```text
Slums.Game -> Slums.Application
Slums.Application -> Slums.Core
Slums.Infrastructure -> Slums.Application
Slums.Infrastructure -> Slums.Core
Slums.Narrative.Ink -> Slums.Application
Slums.Narrative.Ink -> Slums.Core
Slums.Core -> nothing
```

## Content Boundaries

Use Ink for authored scenes:

- intros
- NPC conversations
- crime aftermath scenes
- event scenes
- ending scenes

Use C# for simulation:

- economy and household pressure
- work and crime calculations
- relationship memory and faction state
- police pressure and event eligibility
- ending checks

Keep authored narrative in `content/ink/` and compile to `content/ink/main.json`.
Keep static world data in `content/data/` when it does not need hardcoded rules.

## Current Priorities

### 1. UI Clarity

Make the deeper systems legible to the player.

- explain why work variants improve, worsen, or lock out
- explain why crime routes are available, blocked, or unusually risky
- surface debt, suspicion, relationship-memory, and district-pressure signals in menus or logs

### 2. Balance Pass

Tune the expanded simulation without flattening background identity.

- review pay, stress, pressure, reliability, and lockout values
- review district event frequency and recurrence
- add regression tests around balance-sensitive thresholds

### 3. More Authored Reactivity

Expand content that reflects the systems already in place.

- add more memory-reactive NPC variants
- add more spillover follow-up scenes
- add more district-event and background-specific aftermath content
- expand conversation pools beyond 100 days if needed

## Suggested Next Ticket

If only one follow-up slice is implemented next, do the UI explanation pass for work and crime menus.

Deliver:

- visible reason strings for work variant changes
- visible gating and modifier reason strings for crime routes
- clearer event-log messaging for background and spillover effects

## Narrative Enhancement Pass

Status: implemented.

Completed steps:

1. Expanded recurring NPC reactivity for Hajj Mahmoud, Umm Karim, Mona, and Nurse Salma using trust, debt, police heat, and double-life state.
2. Added queued spillover narrative scenes for public-facing work under heat, criminal money reaching home, and neighborhood protection when Mona trusts you.
3. Expanded ending epilogues so stability and leaving-crime endings react to background, and network-shelter endings react to the strongest support contact.

## Validation

When relevant to the change:

1. Run `npm run compile-ink` from `src/Slums.Game` after editing Ink files.
2. Run `dotnet build .\Slums.slnx` from the repo root.
3. Run `dotnet test .\Slums.slnx` from the repo root.

Every rules change requires tests.

## Update Rules

- Keep this file focused on current state, guardrails, and near-term priorities.
- Do not add back long historical phase breakdowns or obsolete gap lists.
- Update `AGENTS.md` when architecture or workflow expectations change.
- Update `README.md` if the description of planning documents changes.

