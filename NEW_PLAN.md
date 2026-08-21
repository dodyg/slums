# World Enrichment Implementation Plan

## Purpose

This plan adds a broader, more reactive Cairo around the existing survival loop. It is written as a handoff for a future agent session. The implementation must preserve the current architecture:

- `Slums.Core` owns state, deterministic rules, calculations, and effects.
- `Slums.Application` owns commands, queries, and orchestration.
- `Slums.Infrastructure` owns JSON content, validation, persistence, and adapters.
- `Slums.Narrative.Ink` owns authored prose and optional scene follow-up.
- `Slums.Game` owns SadConsole presentation and input only.

`GameSession` remains the canonical runtime boundary. Do not create parallel save-state or simulation models in the UI.

## Scope

Implement these six connected features:

1. News flashes and a city-wide information layer.
2. Persistent infrastructure/service disruptions.
3. A small general inventory system.
4. NPC movement and daily schedules.
5. News-driven NPC economy and employment effects.
6. Player responses and counterplay to news.

The five gaps are deliberately connected to the news system, but each must remain useful when no news is active.

## Design Principles

- News is broader than rumors, tips, and district conditions.
- News should create informed tradeoffs, not invisible punishment.
- Regional conflict is represented through displacement, supply chains, checkpoints, remittances, medicine, and mutual aid; avoid graphic violence or combat missions.
- Economic downturns should be survivable through adaptation, relationships, and community action.
- Keep effects modest, capped, and temporary unless a clearly authored long-term crisis is intended.
- Use seeded session randomness. Save/load must preserve the exact future news sequence.
- Prefer JSON-authored catalogs for content and C#-owned effect rules.
- Keep the first release small: approximately 8-12 news definitions, 4 infrastructure services, 8-12 inventory items, and schedules for the existing NPCs.

## Current Baseline

The repository already has:

- rumors, tips, phone messages, and an event journal;
- daily district conditions with `BulletinText` and gameplay summaries;
- weather, holidays, random events, territory conflict, and NPC economy;
- JSON content loading and validation;
- `GameSession` snapshots and deterministic randomness;
- HUD overview, City status page, Phone screen, and Event Log screen.

Do not duplicate these systems. The closest existing feature is `DistrictConditionDefinition`; news must be a separate persistent, city-wide layer. The existing district `BulletinText` should be surfaced where appropriate, but it should not become the news model.

## Target Runtime Flow

At the start of each new day:

1. Resolve active infrastructure disruptions and news expiry.
2. Resolve weekly or multi-day macro effects.
3. Generate at most one new major news flash, subject to cooldown and eligibility.
4. Apply any immediate news effects.
5. Generate related NPC economy, phone, tip, and schedule changes.
6. Add the flash to the event journal and expose it through the HUD/phone.
7. Queue an Ink scene only for high-impact authored events.

News should not be generated every day. Use a low daily chance plus explicit minimum-day and cooldown gates. A flash should normally last 2-7 days.

## Phase 0: Baseline and Safety

Before implementation:

1. Read `REQS.md`, `PLAN.MD`, `AGENTS.md`, and `MEMORY.MD`.
2. Run the existing build and test workflow from the repository root.
3. Record the current `GameSession.EndDay()` ordering and snapshot format.
4. Inspect existing content validation and random-event effect mapping before adding new effect types.
5. Add no feature until the current baseline is green, or document an unrelated pre-existing failure.

Create a short implementation branch or commit boundary after this baseline.

## Phase 1: Shared World Event Contracts

Create focused Core types under feature-oriented folders, likely `World/News`, `World/Infrastructure`, and `Inventory`.

### News model

Add a `NewsFlashDefinition` or equivalent immutable definition with:

- stable `Id`;
- category: `Conflict`, `Displacement`, `Economy`, `Infrastructure`, `Labor`, `Civic`, or `Technology`;
- title/headline;
- body text;
- source label and source type;
- affected regions/districts;
- minimum day, weight, cooldown, and duration;
- reliability/confirmation label for presentation;
- typed gameplay effects;
- optional Ink knot;
- optional response definitions.

Add runtime state for:

- active flashes;
- seen/expired flash history;
- last generated day;
- response state for active flashes;
- any source-specific unread/acknowledged state required by the UI.

Keep presentation text in content files. Keep effect interpretation in tested C# policies/calculators.

### Infrastructure model

Add a persistent service state keyed by district and service type. Start with:

- electricity/solar battery supply;
- water/pump access;
- transport routes/fares;
- clinic and medicine supply.

Each disruption needs severity, start day, remaining duration, source event/news id, and a recoverable status. Avoid using raw booleans when a three-level state (`Normal`, `Strained`, `Disrupted`) is enough.

### Inventory model

Add a deliberately small `InventoryState` with item id and quantity. Do not migrate food stockpile, medicine stock, plants, pets, or robots into this system in the first slice; those are already modeled and persisted separately.

Initial portable item definitions may include:

- work papers or replacement documents;
- phone repair parts;
- clinic supply packets;
- water containers;
- repair components not already represented by robotics state;
- community food parcels;
- transit passes or route tokens, if useful for counterplay.

Every item must have a clear use. Do not add crafting or encumbrance until a real gameplay need appears.

### NPC schedule model

Add schedule definitions without changing relationship storage:

- NPC id;
- day-of-week availability;
- time windows;
- location;
- absence reason;
- special conditions such as heat, holidays, active news, or infrastructure disruption.

Create a side-effect-free `NpcAvailabilityResolver` that answers whether an NPC can currently be contacted and where they can be found.

## Phase 2: JSON Content and Validation

Add content files under `content/data/`:

- `news_flashes.json`;
- `items.json`;
- `npc_schedules.json`;
- optionally `infrastructure_services.json` if definitions are not code-owned.

Extend `IContentRepository`, `JsonContentRepository`, source-generated JSON metadata, and `ContentCatalogValidator`.

Validation must reject:

- duplicate ids;
- empty titles, bodies, or source labels;
- invalid districts, NPC ids, item ids, or effect ids;
- non-positive weights/durations;
- unknown Ink knots;
- news effects that reference missing infrastructure services;
- schedules referencing missing locations or NPCs;
- response actions with impossible or negative costs;
- duplicate or conflicting schedule entries that make an NPC unavailable all day without an explicit reason.

Use fail-fast behavior consistent with existing repo-owned content.

## Phase 3: News Flash Runtime

Add a focused `NewsService`, `NewsGenerator`, or equivalent. It must:

- select from eligible JSON definitions using `GameSession.SharedRandom`;
- respect minimum day, cooldown, category limits, and duplicate history;
- activate and expire flashes deterministically;
- apply typed effects through Core calculators;
- record a journal entry with day, source, and headline;
- emit a mutation record under `MutationCategories.Information` or add a dedicated `News` category if that improves diagnostics;
- expose active news through a read-only session query;
- queue optional narrative follow-up without putting simulation rules in Ink.

Do not let news directly modify arbitrary stats from JSON. Route effects through named rules such as `ApplyFoodPriceModifier`, `StartInfrastructureDisruption`, `AdjustJobDemand`, `AdjustNpcWealth`, or `AdjustFactionPressure`.

### Initial authored news set

Implement at least these examples:

1. Regional fighting delays medicine shipments.
2. Crossings tighten after failed ceasefire talks.
3. Red Sea freight insurance raises food and transport costs.
4. A major platform cuts contractor rates after an automated pricing review.
5. Solar battery imports are delayed at the port.
6. A clinic supply partnership receives emergency funding.
7. A community remittance service experiences an outage.
8. A public transport route dispute expands into a multi-district slowdown.

Effects should be small and legible. A news item may create or extend an infrastructure disruption, alter prices, change job pay/demand, increase NPC hardship probability, or unlock a community opportunity.

## Phase 4: Persistent Infrastructure Effects

Implement an `InfrastructureImpactCalculator` or equivalent policy that maps service state to existing systems.

Examples:

- electricity disruption: higher home stress, reduced sleep quality, increased utility random events;
- water disruption: home-care cost/time pressure and community collection opportunities;
- transport disruption: higher travel time/cost or unavailable route, with walking still possible where safe;
- clinic supply disruption: medicine price/availability change and a clinic work opportunity;
- solar battery shortage: workshop repair pressure and robotics part scarcity.

All previews must use the same calculation path as committed actions. The player should see the relevant effect before selecting work, travel, shopping, or clinic actions.

Infrastructure state must not create automatic death or eviction. It should increase pressure and create alternatives.

## Phase 5: NPC Schedules and World Presence

Integrate `NpcAvailabilityResolver` with:

- talk/relationship menu queries;
- phone response eligibility;
- job employer access;
- tips and personal warnings;
- investment opportunities;
- crime contacts and faction errands.

At minimum, schedules should distinguish:

- work hours;
- Friday/community or prayer availability;
- clinic and market hours;
- evening ahwa or rooftop contact windows;
- occasional absence caused by hardship, infrastructure problems, or news.

When an NPC is unavailable, show a reason such as “Iman is at the laundry until evening” instead of silently removing them. Schedule changes should be visible in the relevant menu and optionally in phone messages.

## Phase 6: News-Driven NPC Economy

Extend `NpcEconomyResolver` with named macro effects rather than direct global stat edits.

Possible effects:

- increased hardship probability for transport, clinic, or call-center workers;
- delayed NPC repayments or remittances;
- temporary discounts from a well-funded clinic or community group;
- increased demand for food, repairs, or medicine;
- lower investment income for affected businesses;
- stronger mutual-aid lending in refugee or neighborhood networks;
- police or document pressure after a registration or checkpoint news item.

The effect must be bounded and reversible. NPC economy changes should be observable through conversations, tips, phone messages, and loan requests. High-trust NPCs may explain what the broadcast means locally.

Add a macro-economy status to the City or Economy status page so the player can see active pressure without inspecting hidden values.

## Phase 7: Player Responses and Counterplay

Not every news flash needs a choice. For selected flashes, expose one to three responses through the Phone or a News screen.

Possible response types:

- `Prepare`: spend money or inventory to reduce a future cost;
- `HelpCommunity`: spend time/items to improve trust, aid, or infrastructure recovery;
- `ChangeRoute`: move to another district or choose a different job;
- `UseContact`: ask a specific NPC for a loan, warning, or alternate supply;
- `Ignore`: no immediate cost, but lose the opportunity;
- `ShareInformation`: pass the news through the community, improving trust or tip quality.

Responses belong in Application commands and must be guarded in Core. They must not be implemented as screen-side mutations.

Examples:

- Buy clinic supplies early, reducing the impact of a medicine shortage.
- Help a community collection, improving mutual-aid access and reducing local stress.
- Switch from call-center work to workshop or market work during a platform downturn.
- Use a trusted contact to learn whether an official checkpoint report is relevant to the player.

Each response should have a visible cost, requirement, and expected consequence. Cap responses to once per flash unless the design explicitly requires repeated participation.

## Phase 8: Inventory Integration

Add Application commands and queries for:

- viewing inventory;
- acquiring an item through a valid activity or event;
- using an item in a supported action;
- giving or contributing an item to a community/NPC response;
- discarding or resolving expired perishable items if any are introduced.

Keep item acquisition grounded in existing locations and relationships. Examples: clinic packets from Rahma Clinic, phone parts from a repair stall, water containers during a utility event, or work papers through an employer.

Persist inventory through the existing `GameSessionSnapshot`; validate quantities and item ids during load.

## Phase 9: UI and Narrative Integration

### SadConsole

Add either a `NewsScreen` or a News section to `PhoneScreen`. The first implementation should use:

- a short day-start flash in the event log/HUD;
- a readable News view with headline, source, age, affected areas, and gameplay summary;
- response controls only when a response is available;
- City status lines for active macro conditions;
- clear “unconfirmed” or “community report” labels where appropriate.

Do not replace the existing event log. News entries must be distinguishable by source and color but remain accessible after dismissal.

### Ink

Add only a small number of authored follow-ups for high-impact events, for example:

- a refugee-network response to tightened crossings;
- a clinic scene during a medicine shortage;
- a rooftop discussion about a regional downturn;
- a workshop scene during the battery import delay.

Ink may apply narrative flags, trust, messages, or small outcomes through the existing outcome contract. It must not calculate prices, job pay, infrastructure severity, or economy resolution.

## Persistence and Save/Load

Extend snapshots for:

- active and expired news state;
- news response state;
- infrastructure service states;
- inventory quantities;
- any schedule exceptions or temporary absences;
- macro economy modifiers if not derivable from active news.

The restored session must produce the same next-day news, infrastructure, NPC economy, and response behavior as an uninterrupted session with the same random state.

Use the existing `LoadedGameSession` boundary. Do not add an alternate save-state handoff.

## Testing Requirements

### `Slums.Core.Tests`

- news eligibility, weighting, cooldown, expiry, and deterministic selection;
- infrastructure severity and duration resolution;
- price, travel, job, clinic, and sleep effects from infrastructure state;
- inventory quantity clamping and supported item use;
- NPC schedule availability by time, day, location, and override;
- bounded/reversible macro economy effects;
- response guards, costs, and outcomes;
- no news effect can bypass normal ending rules or create impossible negative values.

### `Slums.Application.Tests`

- news queries expose active and unread flashes;
- phone/news responses invoke commands and not UI mutations;
- unavailable NPCs expose a reason;
- work, travel, clinic, shopping, and investment previews include active macro effects;
- response commands reject expired, already-used, or unaffordable choices.

### `Slums.Infrastructure.Tests`

- JSON loading and source-generated serialization;
- catalog validation for all cross-references;
- snapshot round-trip for news, infrastructure, inventory, schedules, and response state;
- save/load continuation preserves deterministic future news and economy results.

### `Slums.Game.Tests`

- fixed-size News/Phone layout;
- long headlines and body text wrap safely;
- response controls show costs and disabled reasons;
- HUD and City page remain readable with active macro effects;
- keyboard-only navigation and return-key suppression.

### `Slums.Narrative.Ink.Tests`

- every new Ink knot loads from the compiled artifact;
- each authored news follow-up can be traversed;
- outcome tags map only to supported application/domain effects;
- no missing-news fallback hides invalid content.

## Recommended Implementation Order

1. Baseline tests and runtime-order notes.
2. Shared news, infrastructure, inventory, and schedule contracts.
3. JSON content, repository loading, and validation.
4. News generation, persistence, and event-journal integration.
5. Infrastructure effects and preview visibility.
6. NPC schedule resolution and menu integration.
7. News-driven NPC economy.
8. Inventory acquisition/use and persistence.
9. Player response commands.
10. Phone/News UI and City status presentation.
11. Small Ink follow-up set and compiled artifact validation.
12. Full build/test workflow and seeded playthroughs.

## Acceptance Criteria

The work is complete when:

- a normal seeded run receives occasional news without daily spam;
- at least one conflict/displacement flash and one economic downturn flash alter the world for multiple days;
- active news effects are visible before affected actions are committed;
- infrastructure disruptions persist, recover, and create counterplay;
- NPC availability changes according to schedules with visible reasons;
- at least one NPC economy outcome is caused by a news flash and can be observed through conversation, tips, or a loan request;
- the player can respond constructively, change plans, or ignore the news;
- inventory items have real uses and save/load correctly;
- rumors, tips, district conditions, and news remain distinct systems;
- save/load preserves exact future behavior for the same seed;
- no UI project contains simulation rules;
- all repository validation commands pass.

## Final Validation Workflow

Run from the repository root:

```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

If Ink source changes, compile it first from `src/Slums.Game` using the repository's existing `npm run compile-ink` workflow.

After implementation, update `MEMORY.MD` with the actual runtime architecture and update `PLAN.MD` only if the roadmap or priorities materially change.
