# Narrative and Mechanics Improvements

## Overall Assessment

Slums has a strong Cairo-2030 identity, an effective survival premise, and a promising blend of authored Ink scenes with systemic simulation. The next improvement pass should focus on playable-run balance, systemic consistency, and meaningful player decisions before adding more content.

## Highest-Priority Fixes

### 1. Make every meaningful action consume time

Crime currently consumes energy but does not advance the clock. This allows multiple crimes in one day and weakens exhaustion, police pressure, and the intended daily rhythm.

NPC conversations also consume no time. Players can rapidly grind trust with every available NPC, unlocking jobs, investments, and endings without sacrificing income or survival.

Recommended changes:

- Add route-specific crime durations, roughly 60–180 minutes.
- Give conversations a 30–60 minute cost.
- Consider limiting each NPC to one meaningful conversation per day.
- Show the time cost in action previews.
- Add automated tests proving that crime and conversation actions advance time and can cross the 22:00 boundary correctly.

Relevant files: `src/Slums.Core/State/GameSession.cs`, `src/Slums.Application/Activities/TalkSceneRequestFactory.cs`.

### 2. Fix invalid and geographically inconsistent faction outcomes

The Bulaq protection route uses `FACTION_REP:BulaqProtection`, but `BulaqProtection` is not a valid `FactionId`. Selecting that narrative outcome can fail during effect parsing.

Successful crime also always increases Imbaba Crew reputation, regardless of the district or route. This makes faction progression geographically incoherent and can make the kingpin ending achievable through unrelated crime.

Recommended changes:

- Replace the invalid Bulaq faction tag with an existing faction or add a properly supported Bulaq faction.
- Map crime outcomes to the faction controlling the current route.
- Add Ink validation for every NPC, faction, and effect target before runtime.
- Add coverage for every crime route’s success, detected, and failure outcomes.

Relevant files: `content/ink/crime.ink`, `src/Slums.Narrative.Ink/InkNarrativeService.cs`, `src/Slums.Core/State/GameSession.cs`.

### 3. Rebalance the first week

The current economy is extremely tight:

- Rent is 20 LE per day.
- Basic jobs generally pay about 18–27 LE before travel.
- Transport usually costs 2 LE.
- Food supplies cost about 15 LE for three household meals.
- Medicine commonly costs 42–58 LE for two units.

This means basic honest work often covers little more than rent and food, while mother care or travel quickly forces crime. Hardship is appropriate, but the honest route should remain difficult rather than structurally nonviable.

Recommended changes:

- Display a morning survival forecast showing rent, food, medicine, transport, and remaining runway.
- Give each background at least one informed first-week honest survival plan.
- Add one emergency community support option per background.
- Keep crime financially attractive, but make its costs visible through heat, stress, lost access, and future obligations.
- Test seeded runs through days 7, 14, and 30 for all identities and backgrounds.

Relevant files: `src/Slums.Core/Expenses/RecurringExpenses.cs`, `src/Slums.Core/World/LocationPricingService.cs`, `content/data/jobs.json`, `content/data/backgrounds.json`.

## Ending Design

The ending checks are mostly threshold-based rather than decision-based. The Luxor ending can trigger from money and low crime without an explicit decision to leave Cairo. Ending priority can also cause honest stability to mask the network, Luxor, or criminal-power outcomes.

Recommended changes:

- Add explicit endgame choices: leave for Luxor, commit to honest stability, accept community shelter, or deepen criminal power.
- Use thresholds to unlock endings, then require a player decision to select one.
- Make the Luxor ending require a train fare, a departure contact, or another concrete preparation step.
- Make the honest ending require a sustained clean period and reliable employment rather than only cumulative earnings.
- Make the kingpin ending require faction control, obligations, and a deliberate commitment to that life.
- Show the player which long-term goals are becoming possible.

Relevant file: `src/Slums.Core/Endings/EndingService.cs`.

## Narrative Improvements

### Recurring NPC dialogue

The project claims 100-day conversation variety, but several recurring knots are literal repetitions. For example, `landlord_default_1` and `landlord_default_6` use the same text, choice, and effects, with the pattern repeated throughout the pool.

Recommended changes:

- Replace copied 100-entry pools with smaller pools of genuinely distinct scenes.
- Vary each NPC’s voice, vocabulary, emotional state, and practical concern.
- Make conversations react to rent, crime suspicion, work reliability, favors, refusals, weather, and infrastructure.
- Give players choices that change access or reveal priorities, not only stress and trust values.
- Add a content test that detects duplicate normalized scene bodies.

Relevant file: `content/ink/npcs.ink`.

### Mother and household

The mother is central to the premise but is currently represented primarily through health, food, medicine, and care flags. Give her more agency:

- personal preferences and memories;
- opinions about honest work and crime;
- requests that sometimes conflict with survival priorities;
- moments of humor, faith, frustration, and pride;
- household decisions that influence the ending.

The goal is for the player to care about her as a person, not only to protect a meter.

### Backgrounds, identity, and gender

The three backgrounds have strong hooks: medical training, political imprisonment, and displacement. Their differences should become longer arcs with distinct contacts, dilemmas, and ending preparation.

Current gender modifiers are mostly static job, crime, and relationship adjustments. Move more gendered vulnerability into specific situations, NPC reactions, and authored choices so Amira and Karim feel different without reducing them to fixed penalty tables.

### Technology and Cairo specificity

The introductions and catalog descriptions establish the uneven technology layer well. More of it should affect play directly:

- biometric checks that can be prepared for or avoided;
- algorithmic job scoring that can be contested through contacts;
- phone credit and shared charging as real constraints;
- solar outages that create community choices;
- delivery-drone salvage with legal and criminal interpretations;
- translation and eligibility systems that produce useful but biased results.

Technology should create tradeoffs, not remain mostly atmospheric description.

### Unimplemented or underdeveloped themes

Romance is not yet visible as a recognizable optional subplot. Addiction and substance use appear mainly as stress-relief activities, with limited consequences. Either defer these themes explicitly or represent them carefully through implication, support, health, sleep, and relationship consequences without glamorizing harmful activity.

## Scope Recommendation

The project currently contains many systems: jobs, crime, investments, news, infrastructure, pets, plants, robots, training, entertainment, debt, territory, and relationships. These are individually promising, but they compete for attention before the central loop is fully proven.

Prioritize a 30-day vertical slice containing:

1. one honest job route;
2. one crime route;
3. rent, food, and mother care;
4. three central NPCs;
5. one faction;
6. one explicit ending choice.

Once that slice produces meaningful daily decisions without exploits, expand the secondary systems around it.

## Acceptance Targets

- Every meaningful action has a visible time, money, energy, stress, and risk cost.
- Honest players can survive the first week with informed choices in every background.
- Crime is profitable but creates persistent consequences that cannot be trivially erased.
- Conversations cannot be trust-farmed without consuming time or opportunity.
- Every authored effect target is validated before runtime.
- At least one failure ending and one non-failure ending are reachable through normal play.
- Ending selection follows an explicit player decision after conditions are met.
- Recurring dialogue contains genuinely distinct scenes rather than repeated labels.
- Seeded day-7 and day-30 playthrough tests pass for both identities and all backgrounds.
