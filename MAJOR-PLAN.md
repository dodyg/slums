# Slums Major Expansion Plan

## Purpose

This document is a handoff plan for the next implementation agent. It assumes the current vertical slice already includes:

- expanded locations in Imbaba, Dokki, and Ard al-Liwa
- additional honest jobs
- recurring talkable NPCs
- contact-influenced crime availability
- contact-influenced crime aftermath and mitigation

The goal of this plan is to move the project from a thin playable slice into a denser, more reactive simulation without breaking the current architecture.

## Read First

Before making changes, read these files in order:

1. `REQS.md`
2. `PLAN.MD`
3. `AGENTS.md`
4. `MAJOR-PLAN.md`

## Non-Negotiable Constraints

- Keep core simulation logic in `Slums.Core`.
- Keep authored scene content in `content/ink/` and compile to `content/ink/main.json`.
- Keep static world content in `content/data/` where practical.
- Do not move economy, survival, or crime calculations into Ink.
- Preserve the current dependency flow.
- Every rules change requires tests.

## Current State Snapshot

The repo already supports:

- travel between multiple Cairo locations
- honest work opportunities across several districts
- recurring NPC conversations with trust-based knot selection
- random events tied to some locations
- crime availability modified by trusted contacts
- post-crime heat reduction and failure mitigation from trusted contacts

What is still missing is depth: more differentiated progression, stronger district identity, more persistent NPC memory, more varied route outcomes, and more medium-term consequence chains.

## Recommended Execution Order

Implement these phases in order unless blocked by discovered technical constraints.

### Phase 1: Distinct Crime Opportunities

#### Objective

Replace some of the remaining generic crime menu entries with contact-specific opportunities that feel tied to people and places.

#### Why First

- The crime route now has the relationship scaffolding to support it.
- The current crime menu is still mechanically repetitive.
- This adds depth without forcing a major save-model rewrite.

#### Deliverables

- Add unique crime opportunities tied to Hanan, Youssef, and Umm Karim.
- Give each opportunity a distinct risk, reward, pressure profile, and narrative tone.
- Gate some opportunities behind trust, faction standing, or story flags.
- Add follow-up scenes for first completion, first failure, and first detected completion where useful.

#### Likely Files

- `src/Slums.Core/Crimes/CrimeRegistry.cs`
- `src/Slums.Core/Crimes/CrimeAttempt.cs`
- `src/Slums.Core/State/GameState.cs`
- `content/ink/crime.ink`
- `tests/Slums.Core.Tests/Crimes/*`
- `tests/Slums.Core.Tests/State/GameStateTests.cs`
- `tests/Slums.Narrative.Ink.Tests/InkNarrativeServiceTests.cs`

#### Acceptance Criteria

- At least 3 distinct contact-specific crime opportunities exist.
- At least 2 of them are gated by relationship or faction conditions.
- The crime menu shows materially different choices by district and contact state.
- Tests prove availability, gating, and outcome differences.

### Phase 2: Honest Work Progression

#### Objective

Turn honest jobs into progression tracks instead of flat repeat actions.

#### Why Second

- Honest work must remain a viable path, not only a fallback.
- The project already has job locations and NPCs that can anchor this progression.

#### Deliverables

- Add progression tiers for clinic, workshop, cafe, bakery, and call center work.
- Tie better shifts or better rates to trust, skills, or reliability.
- Add downsides such as stress buildup, fatigue, stricter attendance expectations, or temporary lockout after mistakes.
- Make some NPCs affect work access or pay quality.

#### Likely Files

- `src/Slums.Core/Jobs/JobRegistry.cs`
- `src/Slums.Core/Jobs/JobService.cs`
- `src/Slums.Core/State/GameState.cs`
- `src/Slums.Core/Relationships/NpcRegistry.cs`
- `content/ink/npcs.ink`
- `content/data/jobs.json`
- `tests/Slums.Core.Tests/Jobs/*`

#### Acceptance Criteria

- At least 3 job tracks have progression or branching outcomes.
- Honest work becomes materially more varied across locations.
- Reliability or relationship state affects access, pay, or penalties.
- Tests cover the new progression rules.

### Phase 3: NPC Memory Beyond Trust

#### Objective

Make NPCs remember more than a trust number.

#### Why Third

- The current trust-only model is already useful but too shallow.
- More memory unlocks better authored scene branching and better systemic reactions.

#### Deliverables

- Extend relationship state to remember selective facts such as:
  - last favor given
  - last refusal
  - unpaid debt state
  - whether the player embarrassed or helped the NPC
  - recent frequency of contact
- Use these memories to route conversation knots or modify outcomes.
- Persist the new relationship facts in saves.

#### Likely Files

- `src/Slums.Core/Relationships/*`
- `src/Slums.Infrastructure/Persistence/*`
- `src/Slums.Core/State/GameState.cs`
- `content/ink/npcs.ink`
- `tests/Slums.Core.Tests/Relationships/*`
- `tests/Slums.Infrastructure.Tests/JsonSaveGameStoreTests.cs`

#### Acceptance Criteria

- At least 2 NPCs use non-trust memory in knot selection or outcome logic.
- Save/load preserves the new relationship memory correctly.
- Tests cover persistence and branching behavior.

### Phase 4: District Identity Through Events

#### Objective

Make Imbaba, Dokki, and Ard al-Liwa feel socially and economically different.

#### Why Fourth

- The map is already broader, but the event layer still needs more district-specific weight.
- This improves atmosphere without forcing UI or save refactors.

#### Deliverables

- Add district-specific random events for:
  - police attention
  - work instability
  - neighborhood solidarity
  - faction pressure
  - transport friction
- Add more authored event knots for those events.
- Where useful, let district or event history influence later event eligibility.

#### Likely Files

- `content/data/random_events.json`
- `content/ink/events.ink`
- `src/Slums.Core/Events/RandomEventRegistry.cs`
- `src/Slums.Infrastructure/Content/JsonContentRepository.cs`
- `tests/Slums.Core.Tests/Events/*`
- `tests/Slums.Infrastructure.Tests/JsonContentRepositoryTests.cs`

#### Acceptance Criteria

- Each core district gains multiple distinct event identities.
- Event conditions cover more than just current location.
- Tests verify eligibility mapping and deterministic selection behavior where appropriate.

### Phase 5: Work-Crime Spillover

#### Objective

Create medium-term consequences where one route affects the other.

#### Why Fifth

- This is one of the highest-value realism upgrades.
- It turns the game from a set of isolated menus into a connected life simulation.

#### Deliverables

- Crimes can endanger work access, raise suspicion, or create exhaustion that hurts honest work.
- Honest work can create witnesses, alibis, or access to locations that change crime options.
- Some NPCs should react differently if the player is trying to maintain both routes.

#### Likely Files

- `src/Slums.Core/State/GameState.cs`
- `src/Slums.Core/Jobs/JobService.cs`
- `src/Slums.Core/Crimes/*`
- `src/Slums.Core/Relationships/*`
- `content/ink/npcs.ink`
- `content/ink/crime.ink`
- `tests/Slums.Core.Tests/State/*`

#### Acceptance Criteria

- There are at least 3 meaningful spillover rules.
- Spillover affects both routes, not only crime.
- The resulting systems remain understandable from the UI and event log.

### Phase 6: Background-Specific Routes

#### Objective

Make the three starting backgrounds diverge more strongly over time.

#### Why Sixth

- The backgrounds exist but need more long-tail mechanical identity.
- This improves replay value.

#### Deliverables

- Give each background at least:
  - one systemic advantage
  - one systemic vulnerability
  - one or more exclusive or favored scenes
- Examples:
  - medical dropout: clinic and medicine advantages
  - former prisoner: police scrutiny and ex-prisoner network access
  - Sudanese refugee: migration pressure, aid-network access, discrimination-related friction

#### Likely Files

- `src/Slums.Core/Characters/*`
- `src/Slums.Core/State/GameState.cs`
- `content/ink/main.ink`
- `content/ink/npcs.ink`
- `tests/Slums.Core.Tests/*`
- `tests/Slums.Narrative.Ink.Tests/*`

#### Acceptance Criteria

- Background choice materially changes play after day 1.
- Differences affect both authored narrative and simulation.
- Tests prove at least one meaningful mechanical divergence per background.

### Phase 7: Expanded Ending Logic

#### Objective

Make endings reflect patterns of survival, compromise, stability, and decline rather than only coarse thresholds.

#### Why Seventh

- Endings should come after the systems that feed them.
- Current work should first improve the run-level state space.

#### Deliverables

- Add more ending conditions based on accumulated behavior:
  - stability through honest work
  - crime success with escalating losses
  - social protection through trusted networks
  - collapse through heat, debt, or household neglect
- Add or expand ending knots in Ink.

#### Likely Files

- `src/Slums.Core/Endings/*`
- `content/ink/endings.ink`
- `tests/Slums.Core.Tests/Endings/*`

#### Acceptance Criteria

- Endings depend on more than one variable.
- At least one ending recognizes social-network investment.
- At least one ending recognizes trying to leave crime after entering it.

## Cross-Cutting Improvements

These can be tackled opportunistically during the phases above.

### UI Clarity

- Show why a job or crime option improved or worsened.
- Surface contact modifiers in menus where possible.
- Improve event log messaging for relationship-driven effects.

### Data-Driven Migration

- Move more hardcoded mappings into content if the implementation remains readable.
- Do not force full data-driven design if it significantly slows delivery.

### Save Compatibility

- If you extend state shape, preserve reasonable compatibility with older saves where practical.
- If compatibility becomes too costly, explicitly version the save format and update tests.

## Suggested First Ticket

If only one major feature should be implemented next, do this:

### Contact-Specific Crime Opportunities

Implement 3 new crime opportunities with the following structure:

- Hanan unlocks a low-visibility market fencing route with moderate reward and lower detection risk.
- Youssef unlocks a Dokki drop route with higher reward and higher pressure if detected.
- Umm Karim unlocks a more dangerous, higher-upside network errand after sufficient reputation.

For each one, add:

- registry logic
- outcome tuning
- first-time narrative scene
- detected variant or failure variant where useful
- tests for gating and outcomes

## Validation Checklist

Before finishing any phase:

1. Recompile Ink if any `.ink` file changed.
2. Run `dotnet test .\Slums.slnx` from repo root.
3. Run `dotnet build .\Slums.slnx` from repo root.
4. Confirm no UI logic leaked into `Slums.Core`.
5. Confirm no core simulation logic leaked into `Slums.Game`.

## Notes For The Next Agent

- Prefer thin vertical additions over speculative framework work.
- Reuse the relationship and story-flag infrastructure already present.
- Keep authored consequence scenes grounded and implication-heavy.
- Do not glamorize crime. The route should remain materially useful, morally corrosive, and increasingly dangerous.
- If a phase turns out to require save-model changes, add tests immediately rather than after the feature is complete.