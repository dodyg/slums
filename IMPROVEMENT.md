# Skill-System Improvement Plan

## Purpose

Deepen the skill system so that more of the existing survival, community, and Cairo-2060 technology systems create meaningful player choices. This plan is for implementation agents. It describes the intended behavior, boundaries, migration concerns, and validation work; it does not authorize implementing all of the work in one change.

## Current Baseline

The current implementation has six `SkillId` values:

- `Medical`
- `Persuasion`
- `StreetSmarts`
- `Physical`
- `RobotRepair`
- `CyberHacking`

The last two were recently added to the implementation but are not yet fully represented in `REQS.md` or `MEMORY.MD`. `RobotRepair` currently supports robotics scavenging and robot repair costs. `CyberHacking` currently supports the network crime route and its training activity.

The target design is nine player skills. Add three skills and broaden the player-facing meaning of the two technology skills:

1. `Provisioning` — food preparation, preservation, and household resource use.
2. `CommunityOrganizing` — collective care, mutual aid, and neighborhood adaptation.
3. `Composure` — functioning under stress, intimidation, and crisis pressure.
4. Broaden `RobotRepair` as user-facing **Technical Repair** without breaking its persisted identifier.
5. Broaden `CyberHacking` as user-facing **Digital Literacy** without breaking its persisted identifier.

Do not add a separate Stealth or Negotiation skill. Those concepts are already covered by StreetSmarts and Persuasion, respectively.

## Design Principles

- Every skill must affect at least three meaningful gameplay decisions or systems.
- Skills should create options and tradeoffs, not turn survival into an automatic success state.
- Existing relationships, jobs, weather, infrastructure, food, and narrative signals remain authoritative where they already own a rule.
- Skill thresholds must be shared between action execution and menu previews.
- Skills must not replace trust, reputation, household care, police heat, or authored choices.
- Benefits should be useful at low and medium levels; level 10 should provide mastery without invalidating the economy.
- Training must remain time- and energy-consuming and retain the once-per-skill-per-day limit.
- The new community skills should create viable non-criminal progress without making crime mechanically irrelevant.
- Preserve the grounded Cairo setting: repair, food knowledge, mutual aid, and local institutions are central; advanced technology remains uneven and fallible.

## Skill Specifications

### 1. Provisioning

Provisioning represents practical food and household resource knowledge. It is distinct from Medical: Medical treats illness and improves clinic outcomes; Provisioning keeps people fed with limited money, stock, water, and time.

Suggested implementation surface:

- Add `SkillId.Provisioning` and a training activity such as `CommunityKitchenPractice`.
- Training should occur at Home or the home-area market, preferably with Mona or another community contact. Use an NPC trust gate only if the activity remains reachable for every background through normal play.
- Integrate with the existing food shop, meal, plant, weekly household-care, and climate-stress rules.
- Suggested thresholds:
  - Level 2: basic meals use food stock more efficiently or receive a small quality improvement.
  - Level 4: herbs from the household garden can improve a meal or substitute for a small ingredient cost.
  - Level 6: preserve or portion food to reduce summer spoilage and protect one stored meal from a price shock.
  - Level 8: prepare a better mother-care meal with a modest additional health or stress benefit.
- Keep the effects bounded. Provisioning must not create free food, eliminate food scarcity, or make sellable herbs unprofitable.
- Allow relevant honest jobs such as bakery, market porter, fish sorting, and house cleaning to provide occasional passive growth, but do not assign every food-adjacent job to this skill.
- Display the next available food benefit and its current cost impact before cooking or buying.

Acceptance criteria:

- A player can choose between selling, cooking, preserving, or saving food/herbs.
- Provisioning changes at least one decision on normal, summer, and food-price-shock days.
- Mother care receives a useful but non-essential benefit.
- Food previews and committed food outcomes use the same calculator.

### 2. Community Organizing

Community Organizing represents the ability to coordinate people and maintain locally governed adaptation. It must be more than Persuasion applied to several NPCs: Persuasion handles one-to-one trust and negotiation; Community Organizing handles group participation, shared resources, and neighborhood outcomes.

Suggested implementation surface:

- Add `SkillId.CommunityOrganizing` and a training activity such as `WaterCommitteePractice`, `CoolingRoomCoordination`, or `NeighborhoodMutualAid`.
- Tie training to a recurring community contact, but provide an early low-level route through the Friday rooftop gathering or neighborhood cleanup.
- Integrate with `CommunityEventService`, community-event skip tracking, mutual-aid loans, infrastructure service state, territory tension, and the `NetworkShelter` ending trajectory.
- Suggested thresholds:
  - Level 2: improve attendance rewards for small community events and soften one skipped-event penalty.
  - Level 4: unlock a bulk food, water, or cooling-room action with a time contribution instead of a large cash cost.
  - Level 6: reduce the impact or duration of a local water/power disruption when the player helps coordinate repairs or rationing.
  - Level 8: unlock a neighborhood response to elevated territory tension or heatwave pressure that reduces tension modestly without removing faction consequences.
- Group benefits should depend on attendance, available NPC trust, supplies, and current infrastructure. The skill alone must not solve a crisis.
- Do not turn the skill into a universal reputation multiplier. Its main effects should be group benefits, event access, and shared-resource efficiency.

Acceptance criteria:

- A community-focused run has at least one distinct progression route that does not require crime.
- At least two community actions have meaningful costs or opportunity costs.
- Community Organizing affects a group-level result, not just individual trust numbers.
- The skill can help with heat, water, cooling, or food pressure while preserving failure and scarcity.

### 3. Composure

Composure represents practiced emotional control under pressure. It should make difficult days more manageable without becoming a flat stress-resistance stat.

Suggested implementation surface:

- Add `SkillId.Composure` and a training activity such as `QuietBreathing`, `PrayerAndReflection`, or `CrisisPreparation` at Home or a community location.
- Integrate with high-stress work mistakes, police contact, debt demands, heatwave events, family-crisis scenes, and selected Ink conditions.
- Suggested thresholds:
  - Level 2: reduce the chance of a work mistake caused solely by stress by a small amount.
  - Level 4: unlock one additional calm response in selected debt, police, or relationship scenes.
  - Level 6: reduce one-time stress spikes from intimidation or crisis events, but not daily baseline stress.
  - Level 8: preserve a small amount of energy or reliability when a high-pressure event is survived.
- Never make Composure reduce all stress every day. Existing hunger, rent, heat, sleep, crime, and household pressures must remain visible.
- Avoid deterministic success in police or crime outcomes. Composure should improve choices and mitigation, not erase detection or consequences.

Acceptance criteria:

- Composure has at least one honest-work use, one household/community use, and one high-pressure narrative or debt use.
- Stress remains capable of reaching dangerous levels in an unmanaged run.
- Menu previews explain when Composure applies.

### 4. Technical Repair (persisted as `RobotRepair`)

Do not rename the enum member in the first implementation. Skill levels are serialized in save snapshots, and renaming an enum key can invalidate existing saves. Change the display label and description to **Technical Repair**, or add an explicit save migration before changing the persisted identifier.

Expand the skill from robots to the repair culture already present in the setting:

- Existing robotics scavenging, robot condition, spare parts, and bench-fee discounts remain supported.
- Add bounded interactions with repairable smart handsets, solar/storage equipment, water pumps, and selected transport or clinic equipment.
- Suggested thresholds:
  - Level 2: reduce ordinary workshop bench fees or repair time.
  - Level 4: unlock a handset or battery repair option using parts and time.
  - Level 6: improve outcomes when restoring a local storage or water-pump service after an outage.
  - Level 8: unlock a paid repair job or higher-quality robotics salvage route.
- Every repair must consume time, parts, money, or an opportunity. Skill should reduce waste and improve access rather than produce material from nothing.
- Keep advanced infrastructure fallible and locally maintained; do not introduce ubiquitous autonomous systems.

Acceptance criteria:

- Technical Repair matters outside the robotics screen.
- At least one repair decision affects household survival and one affects income or mobility.
- Existing saves containing `RobotRepair` load without data loss.
- Repair previews show parts, cost, time, condition, and the skill benefit.

### 5. Digital Literacy (persisted as `CyberHacking`)

Do not rename the persisted enum member until a save migration exists. Change the UI and narrative framing to **Digital Literacy** or **Network Literacy**. Retain the criminal network-errand benefit as one application of the skill, not its definition.

Expand its legitimate and ambiguous uses:

- Existing network-errand success bonus and training remain available, with crime consequences intact.
- Add support for repairable handset setup, digital wallet fees and mistakes, phone messages, platform-work or call-center variants, biometric appeals, transit permits, telemedicine scheduling, and bounded allocation-model decisions.
- Suggested thresholds:
  - Level 2: reduce phone or wallet friction and unlock a basic legitimate digital-work variant.
  - Level 4: improve call-center, dispatch, or platform-work access and reduce one avoidable digital fee.
  - Level 6: unlock a biometric appeal or document-correction route with time and uncertainty costs.
  - Level 8: improve the reliability of selected tips, telemedicine access, or infrastructure information without making news omniscient.
- The skill must not become a universal hacking key. Keep security, identity, and institutional consequences meaningful.
- Use `TechnologyObligationState` for lasting liabilities such as exposure, scrutiny, or confidence debt rather than hiding those consequences in the skill.

Acceptance criteria:

- Digital Literacy affects at least one honest work path, one household or service path, and the criminal network route.
- Digital actions expose uncertainty and possible obligations before commitment.
- Existing `CyberHacking` save values load correctly.
- The skill does not enable persistent omniscient AI or frictionless identity bypass.

## Implementation Order

Implement in thin vertical slices. Complete tests and documentation for each slice before starting the next.

### Phase 0: Baseline and compatibility

1. Confirm the current six-skill implementation and identify all skill serialization, UI-label, training, job, crime, and snapshot call sites.
2. Add shared display metadata and threshold definitions rather than scattering skill names and numbers through screens and services.
3. Decide whether user-facing labels should be `Technical Repair`/`Digital Literacy` while keeping `RobotRepair`/`CyberHacking` as persisted identifiers.
4. Add save-load regression tests for old snapshots containing both existing technology skill IDs.

### Phase 1: Provisioning

1. Add the domain skill and training activity.
2. Extract or extend a pure provisioning calculator used by food purchase, cooking, preservation, and mother-care previews.
3. Add application commands and menu status fields.
4. Add event-journal and mutation diagnostics for training and food conversions.
5. Add focused core, application, infrastructure snapshot, and game-layout tests.

### Phase 2: Community Organizing

1. Add the domain skill and reachable training path.
2. Extend community-event contexts with group-level outcomes and transparent costs.
3. Add one infrastructure-adaptation action and one mutual-aid or food/water action.
4. Connect outcomes to narrative signals without moving community rules into Ink.
5. Add tests for attendance, skipped events, resource scarcity, tension, and Network Shelter progress.

### Phase 3: Technical Repair expansion

1. Preserve `RobotRepair` snapshot compatibility.
2. Extract reusable repair cost, part, time, and success calculations.
3. Add one household repair action and one service/income repair action before adding more catalog content.
4. Add previews and diagnostics, including failed repairs and consumed resources.
5. Test outage recovery, repair failure, save/load, and existing robot behavior.

### Phase 4: Digital Literacy expansion

1. Preserve `CyberHacking` snapshot compatibility.
2. Add one legitimate digital-work or service interaction.
3. Add one biometric, wallet, phone, or telemedicine interaction with a persistent technology obligation where appropriate.
4. Keep Network Errand as the first criminal integration and add no more than one new criminal interaction in the first slice.
5. Test preview/commit parity, uncertainty, obligations, and crime consequences.

### Phase 5: Composure

1. Add the skill and a grounded training activity.
2. Integrate with work mistake risk and one debt or police-pressure scene.
3. Add narrative conditions only after the systemic effects are stable.
4. Verify that stress remains a consequential survival pressure across seeded runs.

### Phase 6: Balance and acceptance

1. Run seeded playthroughs across Amira/Karim and all three backgrounds through at least day 35.
2. Compare skill investment against rent, food, travel, medicine, debt, and training opportunity costs.
3. Verify that honest, community, technical, digital, and criminal routes each have a credible early-game use.
4. Ensure no skill is mandatory for survival and no skill is useless after its first unlock.
5. Update regression tests for any balance change.

## Required Code Touchpoints

Agents should inspect and reuse existing services before adding abstractions. Likely touchpoints include:

- `src/Slums.Core/Skills/SkillId.cs`, `SkillState.cs`, and shared skill metadata/thresholds.
- `src/Slums.Core/Training/TrainingRegistry.cs`, `TrainingService.cs`, and `TrainingActivityType.cs`.
- Food, household, plant, weather, and economy services for Provisioning.
- `CommunityEventService`, infrastructure state, territory/tension services, debt services, and ending signal rules for Community Organizing.
- `HouseholdAssetsService`, robotics services, phone state, infrastructure services, and repair calculators for Technical Repair.
- Phone, news/tips, clinic/telemedicine, transport, technology obligations, jobs, and crime services for Digital Literacy.
- Work mistake calculation, debt/police pressure resolution, narrative signals, and selected Ink globals for Composure.
- Application menu queries and commands for every new action.
- `GameSession` snapshot capture/restore and infrastructure validators for all new persisted state.

Do not mutate `GameSession` directly from SadConsole screens. Do not put food, community, repair, digital-service, or stress rules into the UI or Ink scripts.

## Testing Requirements

Use TUnit, FluentAssertions, and NSubstitute according to repository conventions.

Minimum tests:

- Skill metadata exposes all target skills with stable display names and descriptions.
- Each training activity obeys location, trust, time-of-day, money, energy, level-cap, and once-per-day guards.
- Each skill has tests for level 0, the first meaningful threshold, a high threshold, and level 10.
- Preview calculations exactly match committed outcomes.
- Food, community, repair, digital, and composure mutations emit diagnostics and event-journal entries where required.
- Snapshot round trips preserve all skill levels and daily training flags.
- Old snapshots containing `RobotRepair` and `CyberHacking` remain loadable.
- Application queries explain unavailable actions instead of silently hiding them where the player needs the reason.
- Game tests cover skill display, menu layout, disabled-action text, and keyboard navigation.
- Narrative tests cover only synchronized variables and authored follow-ups; core skill rules remain in C#.

Run the repository validation workflow after implementation:

```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

Run the repository's .NET slopwatch check after every code modification, as required by the available repository skill.

## Documentation Updates Required

After each implemented phase, update documentation in the same change. Do not leave the implementation and design documents describing different skill counts.

### `REQS.md`

- Replace the four-skill description with the target nine-skill model.
- Document Provisioning, Community Organizing, and Composure training methods, gates, costs, time, and energy.
- Document Technical Repair and Digital Literacy as the broader player-facing roles of the persisted technology skill IDs.
- Add the skill effects to the relevant food, community, infrastructure, phone, clinic, work, crime, and technology sections.
- State the save-compatibility policy for existing technology skill IDs if display names differ from enum names.

### `MEMORY.MD`

- Update `SkillState` from four to nine documented skills.
- Update the skill training count and table from four activities to the implemented set.
- Record every service that now consults each skill.
- Add snapshot and migration notes for `RobotRepair` and `CyberHacking`.
- Update content-data and UI status-page notes if new authored or JSON data is introduced.

### `PLAN.MD`

- Add the skill expansion phases to the current priorities, keeping the plan forward-looking and concise.
- Add acceptance targets for route reachability, early survival, preview/commit parity, and ending reachability.
- Remove completed phase tickets as each slice lands rather than appending implementation history.

### `AGENTS.md`

- Update only if implementation introduces a new architectural rule, persistence boundary, content format, or required validation command.
- If no repository rule changes, leave `AGENTS.md` unchanged and record that decision in the implementation change summary.

## Definition of Done

This improvement is complete when:

- The target skills are implemented or deliberately deferred with a documented reason.
- Each implemented skill has multiple meaningful uses, a grounded training route, previews, diagnostics, and tests.
- The existing economy remains difficult but recoverable, and no skill is a mandatory universal key.
- Technology skills remain compatible with existing saves.
- Relevant Ink, JSON, application, UI, and persistence changes are validated at their proper layers.
- `REQS.md`, `MEMORY.MD`, and `PLAN.MD` accurately describe the shipped behavior.
- The full build and all five executable test suites pass.
