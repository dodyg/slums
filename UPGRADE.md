# Narrative and Science-Fiction Upgrade Roadmap

## Purpose

This document turns the August 2026 narrative review into eight implementation-ready work packages. It is a roadmap, not a replacement for `REQS.md`, `PLAN.MD`, or `AGENTS.md`. Future agents must read those files and `MEMORY.MD` before changing code or content.

The goal is a coherent 30-day science-fiction drama in which Cairo's infrastructure, institutions, repair cultures, climate pressures, and unequal digital systems shape what the player can do. Technology must create access, labor, surveillance, maintenance, and political consequences—not act as decorative futurism or effortless magic.

## Implementation status

The first implementation pass is committed in review-sized changes:

- Completed the reachability and truthful-consequence foundation, including the entry-knot catalog, synchronized scene state, typed rent/debt/Ramadan effects, and Tarek's missing context knots.
- Added the shared rooftop water-and-power cooperative crisis with deterministic day beats, typed decisions and resolutions, application commands, persistence, Ink traversal, and ending callbacks.
- Added 100-entry recurring conversation decks with persisted non-repetition history and stable variant state, plus the compiled-story choice audit.
- Added bounded robot capabilities for salvage recovery, clinic triage, and assisted transit; each requires an operational machine and creates condition wear.
- Replaced rumor-location duplication with canonical NPC district data.
- Added deterministic, persisted one-time reachability planning for all 23 weather and seasonal/holiday knots, including background and home-upgrade variants.
- Added five ordered beats for the mother, Mona, Salma, Hajj Mahmoud, and Umm Karim, with refusal-aware Ink choices, typed central decisions, delayed crisis-linked scheduling, and snapshot persistence. The mother remains intentionally unnamed pending the product/cultural review gate in this file.
- Added typed obligations for non-robot technology and synchronized them into Ink: handset data exposure, microgrid repair debt and storage condition, transit permit review, biometric appeal state, telemedicine use, and bounded allocation-model confidence.
- Replaced immediate selectable-ending commits with persisted two-stage commitment scenes. The final Ink choice records a concrete sacrifice before the existing ending ID and epilogue are committed.
- The 48 weather/season/community/debt scenes, Tarek context variants, and recurring 100-variant renderer are covered by deterministic Core planning and compiled-story validation. Human cultural/language review remains an external sign-off requirement and cannot be certified by automated tests.

## Guardrails

- Preserve .NET 10, C# 14, SadConsole, Ink, JSON persistence, and the existing inward dependency direction.
- Keep `GameSession` as the canonical runtime state boundary.
- Put simulation rules and persistent story state in `Slums.Core`, orchestration in `Slums.Application`, persistence in `Slums.Infrastructure`, Ink adaptation in `Slums.Narrative.Ink`, and presentation in `Slums.Game`.
- Keep authored prose and branching scenes in `content/ink/`. Do not move economy or survival rules into Ink.
- Continue to fail fast on missing or invalid Ink and repository-owned JSON. Do not add a fallback narrative runtime.
- Apply completed Ink outcomes through `ApplyNarrativeOutcomeCommand` and `GameSession.ApplyNarrativeOutcome`.
- Preserve the existing eight ending IDs and automatic failure-ending rules unless `REQS.md`, `PLAN.MD`, and `AGENTS.md` are deliberately updated.
- Use implication and consequence rather than graphic violence, child harm, explicit drug use, torture, or police-brutality detail.
- Do not normalize omniscient AI, everyday cybernetics, or portable nuclear power. Any machine-learning system should be bounded, institutional, fallible, and legible through its inputs and consequences.
- Treat a title change, a named medical diagnosis for the mother, and changes to the product's gender design as product decisions. Record a proposal and request approval rather than silently establishing new canon.

## Baseline Defects to Preserve as Regression Tests

Before rewriting content, encode the following findings as tests where practical. This makes the upgrade measurable and prevents content work from concealing broken wiring.

- The former 48-knot reachability gap covered by `content/ink/weather.ink`, `seasons.ink`, `community.ink`, and `debt.ink` is now guarded by deterministic Core planners and entry-knot validation.
- Tarek's registry can request `tarek_warm_1` through `tarek_warm_4` and `tarek_streetwise_1` through `tarek_streetwise_4`, but the source currently defines only the default sequence.
- Recurring NPC conversation now composes 10 authored openers with 10 authored topical bodies in Ink, and the validator exercises all 100 combinations for the supported deck.
- The checked-in Ink source contains approximately 343 knots: 92 with no choice, 202 with exactly one choice, and 49 with multiple choices. A one-option prompt generally creates no meaningful agency and should be converted to continuation unless selecting it has a real purpose.
- Consequence tags are lopsided: stress and trust changes are common, while persistent decision flags are rare. Branches often change tone without changing later play.
- `event_rent_final_warning` promises three more days and describes Mona paying half the rent, but neither effect reaches rent or money state.
- `event_loan_shark_visit` removes money without reducing the loan balance, and its promised extra week does not extend the debt due date.
- `event_ramadan_start` now emits the typed `RAMADAN_FASTING` effect through `ApplyNarrativeOutcomeCommand`, and the seasonal planner provides its entry path.
- The medical introduction says rent is due in ten days, while the simulation deducts daily rent beginning with the first end-of-day cycle.
- `NarrativeSceneState` exposes only basic player and household statistics, day, background, and gender. Ink therefore cannot honestly react to district, weather, season, holiday, debts, rent, infrastructure, police pressure, news, relationships, robots, or central-story decisions.
- Abu Samir's workshop is in Ard al-Liwa in canonical location data but is called a Bulaq workshop in crime prose.
- `RumorGenerator.GetNpcsInDistrict` duplicates location knowledge and assigns several NPCs to the wrong districts. Canonical NPC schedules or locations should drive this query.
- The Sudanese background's unused `StoryIntro` says the character is Egyptian by law while also describing refugee status and a Sudanese ID.
- Currency labels alternate between `EGP` and `LE`, and the intended relationship between 2060 nominal prices and present-day player-readable values is not documented.

## Recommended Delivery Order

Implement the packages in this order because later narrative work depends on earlier reachability and state plumbing:

1. Narrative reachability and truthful consequences.
2. Shared 30-day story spine.
3. Central character arcs.
4. Technology capabilities and obligations.
5. Persistent, consequential choices. Apply this rule continuously while implementing packages 2–4 rather than postponing all choice work.
6. Recurring dialogue depth and non-repetition.
7. Geographic, language, cultural, and economic continuity pass.
8. Expanded final scenes and relationship resolution.

Keep commits or review units aligned with these packages. Do not mix a large prose rewrite with unrelated simulation refactoring; that makes regressions and cultural review harder to isolate.

---

## 1. Fix Narrative Reachability, Missing Knots, and False Consequences

### Outcome

Every authored knot is either reachable through a declared trigger/route or explicitly marked as non-entry support content. Every line that promises a mechanical consequence applies that consequence to the canonical game state.

### Implementation steps

1. Add an explicit entry-knot catalog in `src/Slums.Core/Narrative/` for every player-visible scene category: introductions, scheduled events, weather, seasons, holidays, community, debt, work, crime, recurring NPC scenes, follow-ups, and endings. Reuse `NarrativeKnots`, `NarrativeSignalRules`, and `NarrativeFollowUpPlanner` rather than creating a second trigger system.
2. Extend the artifact validator so every top-level authored knot must satisfy exactly one condition:
   - referenced by the Core entry-knot catalog;
   - referenced by repository JSON content;
   - referenced by another reachable Ink path; or
   - declared as a support-only knot with a documented reason.
3. Wire the 48 orphaned weather, seasonal, community, and debt scenes into deterministic eligibility rules. Put eligibility and cooldown rules in Core; use application commands to queue scenes. Do not let Ink decide whether a simulation event occurred.
4. Repair the Tarek mismatch by either authoring the missing warm and streetwise knots or changing the registry to request knots that actually exist. Prefer authored context-specific material because relationship context is a stated feature.
5. Extend `NarrativeSceneState` with the smallest stable, typed snapshot Ink needs to render truthfully. Include at least current district, weather, season, active holiday/Ramadan state, rent status, debts, infrastructure status, police pressure, relevant relationships, owned operational robots, current news/crisis phase, and persistent story decisions. Use normalized Ink-friendly strings and scalar values; do not pass mutable domain objects into the adapter.
6. Introduce typed application-level narrative effects for consequences that generic stat tags cannot express. Initial effects should cover:
   - rent payment and rent-grace days;
   - payment against a specific debt source;
   - extension of a specific debt due date;
   - Ramadan fasting choice;
   - explicit central-story decisions.
7. Parse the corresponding Ink tags in `Slums.Narrative.Ink` and translate them to typed effects. Suggested tag forms are `RENT_PAYMENT:10`, `RENT_GRACE_DAYS:3`, `DEBT_PAYMENT:LoanShark,40`, `DEBT_DUE_EXTENSION:LoanShark,7`, `RAMADAN_FASTING:true`, and `CRISIS_DECISION:key,value`. Final syntax must be centralized and validated rather than parsed ad hoc in screens.
8. Apply those effects through `ApplyNarrativeOutcomeCommand`; add focused methods or evaluators in Core when a consequence has domain invariants. For example, debt cannot be reduced below zero, and a due-date extension must operate on the correct debt.
9. Rewrite the known false-consequence scenes so prose and mechanics agree. Decide whether Mona transfers money or directly credits rent, but do not imply both. Align the introduction's rent language with the daily-rent model or change the model only through an approved requirements update.
10. Ensure `NarrativeScreen` treats zero-choice passages as ordinary continuation. Remove single choices that merely mean “continue” unless the explicit acknowledgment is narratively important and tested.

### Primary file areas

- `src/Slums.Core/Narrative/`
- `src/Slums.Core/State/GameSession.cs`
- `src/Slums.Application/Narrative/`
- `src/Slums.Narrative.Ink/`
- `src/Slums.Game/Screens/NarrativeScreen.cs`
- `content/ink/`
- `tests/Slums.Core.Tests/Narrative/`
- `tests/Slums.Application.Tests/Narrative/`
- `tests/Slums.Narrative.Ink.Tests/Coverage/`

### Required tests

- Artifact coverage test proving no player-visible knot is orphaned.
- Catalog test proving every registered entry knot exists in the compiled story.
- Trigger tests for representative weather, season, community, debt, and Ramadan states, including cooldown behavior.
- Tarek path test covering default, warm, and streetwise contexts.
- Outcome integration tests for rent grace/payment, partial debt payment, due-date extension, and fasting on/off.
- Save/load tests proving each new persistent consequence survives serialization.
- UI-shell test proving a no-choice passage advances without manufacturing a choice.

### Acceptance criteria

- The validator reports zero unexplained orphaned entry knots.
- Every knot returned by `NpcRegistry` exists and starts successfully.
- All 48 currently disconnected scenes have tested entry paths or are deliberately reclassified and documented.
- Prose never promises rent, debt, fasting, or money effects that are absent from the resulting `GameSession`.
- Missing globals, invalid tags, and unknown effect targets fail at bootstrap or in the artifact tests.

---

## 2. Build One Shared 30-Day Story Spine Around an Infrastructure Crisis

### Outcome

All play styles participate in the same escalating civic crisis, with route-specific methods and consequences. The month should feel like one story rather than a collection of jobs and random events.

### Proposed spine

Use a neighborhood rooftop water-and-power cooperative whose allocation is downgraded after an automated irregular-consumer review. The system is not sentient and there is no omniscient conspiracy: it is a bounded allocation model operating amid scarcity, incomplete records, institutional neglect, and opportunism.

- Days 1–5: establish the cooperative, its cooling room, water schedule, repaired storage, and the people who rely on it.
- Days 6–15: an irregular-consumer classification reduces access. The neighborhood gathers meter evidence, pursues an appeal, negotiates, or seeks illicit workarounds.
- Days 16–24: a heat dome and hotter nights turn the administrative problem into an immediate survival problem.
- Days 25–29: the player commits resources and relationships to one response.
- Day 30: the infrastructure outcome combines with the player's economic route and relationships to shape the final scene.

### Implementation steps

1. Add a focused Core state object, provisionally `CityCrisisState` or `StoryArcState`, owned by `GameSession`. It should hold a phase enum, discovered evidence, committed resources, key decisions, cooperative condition, and resolution. Do not represent the entire arc as untyped story flags.
2. Add a Core planner/evaluator that derives eligible crisis beats from day, phase, state, and prior decisions. It must be deterministic for a given session state and must not depend on SadConsole or Ink.
3. Expose crisis status through player-facing application queries. Queue the next scene through existing narrative orchestration, and prevent duplicate or out-of-order beats.
4. Persist the crisis state in a dedicated snapshot owned by the existing `GameSessionSnapshot` aggregate. Define backward-compatible defaults for saves created before the field exists.
5. Author a concise design sheet before writing Ink. For each beat, specify entry conditions, new information, active NPCs, available approaches, persistent effects, and later callbacks. Reject beats that only restate the crisis.
6. Author a shared introduction and escalation sequence in Ink, then branch methods by route:
   - honest-work play uses clinic, depot, repair, and official-appeal contacts;
   - crime play can obtain diverted parts, forged access, or coercive leverage but increases exposure and social cost;
   - community play organizes water rosters, evidence gathering, mutual aid, and public pressure.
7. Allow mixed strategies. A player should not be locked into a route because of one early action, but late commitments should close incompatible options.
8. Make survival systems interact with the crisis: heat, water pressure, power/storage condition, work hours, food spoilage, household health, and access to cooling should change available actions or their cost.
9. Add visible but concise HUD/menu status for the current crisis phase and any immediate obligation. Keep detailed prose in scenes, not in UI rule logic.
10. Update `PLAN.MD` only if the new spine changes established scope or milestones; update architecture documentation if the chosen state/evaluator structure becomes canonical.

### Required tests

- Core phase-transition tests for days and decisions, including no skipped or repeated phases.
- Application tests for scene eligibility, queue order, and mixed-route availability.
- Persistence round-trip tests at every phase.
- Full-playthrough tests for honest, crime, community, and mixed responses.
- Narrative traversal tests proving every crisis branch terminates and returns valid effects.
- Boundary tests proving failure endings still preempt the story when survival conditions require it.

### Acceptance criteria

- Every normal playthrough encounters the shared crisis by day 6.
- At least three mechanically distinct approaches exist, and each uses existing game systems rather than prose-only labels.
- At least two early decisions alter a later scene or action, and the final crisis state contributes to the ending.
- The antagonist remains institutional scarcity and human choice, not magical technology or a generic evil AI.

---

## 3. Turn the Mother and Four Central NPCs Into Genuine Character Arcs

### Outcome

The mother, Mona, Salma, Hajj Mahmoud, and Umm Karim become recurring people with independent needs, changing relationships, conflicts, and end states. Other NPCs may remain supporting contacts until these five arcs work.

### Implementation steps

1. Create a short character bible in repository documentation or a content design file. For each central character, record voice, history, material need, private fear, practical skill, boundary they will not cross, relationship to two other NPCs, and possible month-end outcomes.
2. Give the mother a name, preferences, agency, and a social network through explicit content review. If a diagnosis is named, obtain product/cultural review first and ensure gameplay never reduces her to a health meter. She should make requests, disagree, conceal or reveal information, help others, and react to how the player secures resources.
3. Design five or six beats per character: introduction, transaction, vulnerability, conflict, reckoning, and outcome. Each beat must change information, a relationship, an obligation, or an available action.
4. Tie each arc to the shared crisis without making every conversation about the protagonist. Examples: Mona must account for scarce medicine; Salma weighs evidence against a source's safety; Hajj Mahmoud must decide how the cooling room is governed; Umm Karim balances household survival with a water roster.
5. Add inter-NPC consequences. Helping one character can disappoint or endanger another; reconciliation should sometimes require resources or a concession rather than a trust threshold alone.
6. Add typed persistent arc state for high-impact decisions, either within the central story state or a focused character-arc structure. Keep affinity/trust as a signal, not a substitute for remembered events.
7. Gate beats using both history and present conditions. A high-trust scene must not assume help the player refused; a low-trust scene must still recognize concrete sacrifices.
8. Write Ink scenes with distinct diction, sentence rhythm, priorities, and knowledge. Avoid using exposition to make all five characters sound like the same setting guide.
9. Surface only actionable relationship information in UI. Do not expose hidden dramatic variables unless doing so helps the player make a fair decision.
10. Add each character's resolved or unresolved state to the final scene matrix in recommendation 8.

### Required tests

- Arc-order tests preventing reckoning/outcome scenes before prerequisites.
- Refusal and failure-path tests; arcs must continue plausibly when the player says no.
- Save/load tests for high-impact character decisions.
- Narrative tests proving prose assumptions match stored history.
- A content matrix test or validated catalog proving each central character has the required beats and at least two relationship callbacks.

### Acceptance criteria

- Each central character has a desire independent of helping or obstructing the player.
- Each has at least one conflict with the player, one meaningful interaction with another central character, and more than one plausible end state.
- The mother's scenes contain agency and relationship content beyond changes to `MotherHealth`.
- Trust alone cannot unlock a scene whose factual prerequisites are false.

---

## 4. Make Technology Grant Capabilities and Create New Obligations

### Outcome

Repairable handsets, wallets, microgrids, autonomous transit, delivery machines, biometrics, telemedicine, and bounded ML services materially change play. Each advantage also creates maintenance, access, surveillance, labor, or governance pressure.

### Implementation steps

1. Audit every future technology mentioned in Ink or JSON. For each, document owner, user, power source, connectivity assumption, maintenance path, failure mode, data exposure, and gameplay capability. Cut or revise technology that has only cosmetic prose.
2. Start with the existing robot subsystem because it already has purchase, repair, condition, and wear. Give every operational robot a bounded capability and an obligation:
   - `SalvageCrawler`: improves parts recovery or unlocks hazardous salvage, but is conspicuous and can attract faction, permit, or extortion pressure.
   - `RepairDrone`: reduces selected maintenance/service-outage costs, but consumes parts or battery and may retain route/camera data others want.
   - `CargoMule`: reduces transport energy or enables bulk purchases, but needs repair and can trigger police/permit scrutiny.
3. Put capability and wear calculations in Core registry/rule classes. Use application queries to describe availability and commands to perform actions. Screens must not calculate robot bonuses.
4. Ensure benefits require an operational robot, apply condition loss consistently, and cannot stack beyond documented limits. Add repair, battery, parts, or permit costs using existing resources where possible.
5. Add at least one non-robot technology interaction per major system: handset/wallet access, microgrid storage/repair, autonomous transit disruption, biometric exclusion or appeal, telemedicine triage, and a bounded allocation/recommendation model in the crisis.
6. Make low-tech and collective alternatives viable. A broken handset or failed biometric check should create a harder route through people and institutions, not an unwinnable state.
7. Author consequences from local ownership and repair culture: who can inspect a device, who controls replacement parts, whose roof holds the battery, and who is accountable when automation fails.
8. Reflect capabilities and liabilities in `NarrativeSceneState` so Ink can acknowledge actual ownership and condition.
9. Persist any new obligations, such as permits, data exposure, battery debt, or promised repair work, through typed state rather than prose-only flags.

### Required tests

- Rule tests for each robot's benefit, wear, cost, unavailable state, and stack limit.
- Application tests for newly unlocked actions and truthful menu descriptions.
- Persistence tests for condition and new obligations.
- Narrative reactivity tests with and without each relevant technology.
- Full-playthrough test proving a player without advanced equipment still has a viable response to the central crisis.

### Acceptance criteria

- Every purchasable robot provides a tested gameplay capability.
- Every significant capability has at least one ongoing cost or risk.
- No technology is described as generally intelligent or omniscient unless the requirements are explicitly changed.
- Repair knowledge, shared infrastructure, and human networks remain as important as ownership.

---

## 5. Replace Faux Choices With Persistent Decisions

### Outcome

Choices communicate genuine tradeoffs. Routine continuation is presented as continuation, while major decisions persist and affect later scenes, availability, resources, relationships, or endings.

### Implementation steps

1. Generate a choice audit from the compiled Ink artifact listing every knot, choice count, emitted tags by branch, and referenced follow-up. Check the audit into test output only if it is stable and useful; otherwise make it an on-demand test diagnostic.
2. Classify each choice as navigation, tone, information, tactical resource tradeoff, relationship commitment, or strategic commitment.
3. Convert one-option choices to ordinary continuation unless pressing the option conveys a meaningful acknowledgment that is referenced later.
4. For every tactical or strategic choice, define an outcome signature: state changes, typed decisions, relationship effects, obligations, unlocked/closed actions, and queued follow-ups.
5. Require sibling branches of a high-impact choice to differ in at least one persistent effect or future availability condition. Different prose followed by identical state is not sufficient.
6. Store named decisions in typed Core state when rules depend on them. Reserve free-form story flags for lightweight narrative memory and keep flag names centralized in `StoryFlags`.
7. Add delayed callbacks. At least one consequence of each central crisis decision should appear several days later rather than resolving in the same passage.
8. Show the player enough cost information to make fair choices, while allowing uncertainty about human reactions. Avoid surprise penalties unrelated to the presented dilemma.
9. Keep refusal paths authored. Saying no should often preserve time or resources while costing trust, information, or opportunity; it must not silently terminate a character arc.
10. Have the narrative validator reject unknown tags, malformed effect arguments, duplicate choice text within a knot, and high-impact branches with identical normalized outcome signatures.

### Required tests

- Choice-audit coverage for every multi-choice knot.
- Branch integration tests comparing high-impact outcome signatures.
- Delayed-callback tests that advance days and assert the correct later scene/action.
- Refusal-path traversal tests.
- Save/load tests immediately after major decisions and before their callbacks.

### Acceptance criteria

- No routine scene uses a single “continue” choice merely because the UI expects choices.
- Every high-impact choice changes persistent state or future availability.
- At least one delayed consequence exists for every major crisis commitment and central-character conflict.
- The player can understand the immediate tradeoff before committing.

---

## 6. Rewrite Recurring NPC Content for Voice and Dramatic Progression

### Outcome

Talking to an NPC remains fresh for at least 100 in-game days in the same relationship context, while milestone conversations advance character and story arcs instead of behaving like interchangeable barks.

### Implementation steps

1. Separate recurring contact into two layers:
   - short, variable everyday exchanges for texture and relationship maintenance;
   - authored milestone scenes selected by arc state, recent events, and obligations.
2. Replace the fixed pool size of four with an authored variant system capable of at least 100 distinct rendered exchanges per NPC/context before repetition. A maintainable starting design is ten compatible openers multiplied by ten topical bodies, selected without replacement from the 100 combinations.
3. Keep all prose in Ink. Extend `TalkSceneRequest`/`NarrativeSceneState` with stable variant identifiers or parameters so Ink renders the selected authored components; do not hardcode conversation sentences in C#.
4. Track seen conversation variant IDs per NPC and context in relationship state, and persist them. Reset or migrate the deck only after all valid variants are exhausted or the relationship context changes.
5. Select variants using session-owned seeded randomness so save/load and tests are reproducible. Saving before a conversation and reloading must not reroll until a preferred line appears.
6. Condition topical bodies on recent observable state: weather, neighborhood event, work route, crisis phase, promise/debt, holiday, news, or another NPC. Ensure composed opener/body pairs are grammatically and emotionally compatible.
7. Write a voice guide for each recurring NPC: favored vocabulary, sentence length, humor, formality, what they notice, what they avoid, and their relationship to Arabic/English code-switching. Do not simulate dialect through caricatured spelling.
8. Reserve long choices for milestone scenes. Ordinary exchanges may end with continuation or a small response only when that response changes the relationship or conversation state.
9. Add normalized duplicate detection across all generated combinations. The test should ignore whitespace and markup but retain meaningful wording.
10. Phase the rewrite: complete the five central characters first, validate the system, then expand supporting NPCs without reducing the 100-day guarantee.

### Primary file areas

- `src/Slums.Core/Relationships/ConversationPoolRegistry.cs`
- `src/Slums.Core/Relationships/NpcRelationship.cs`
- `src/Slums.Application/Activities/TalkSceneRequest.cs`
- `src/Slums.Application/Activities/TalkSceneRequestFactory.cs`
- `src/Slums.Application/Narrative/NarrativeSceneState.cs`
- `src/Slums.Infrastructure/Persistence/GameSessionNpcRelationshipSnapshot.cs`
- `content/ink/npcs.ink`
- `tests/Slums.Narrative.Ink.Tests/Coverage/RecurringNpcSceneValidationTests.cs`

### Required tests

- Generate 100 conversations for one unchanged NPC/context and assert no normalized duplicate.
- Repeat the test for every supported NPC/context combination.
- Save/load midway through the deck and assert the same next selection and no reset.
- Context-transition tests proving the correct deck and milestone scenes become eligible.
- Composition tests proving every opener/body combination starts, renders, and terminates without malformed text.
- Tarek regression tests for warm and streetwise variants.

### Acceptance criteria

- No recurring exchange repeats within 100 in-game days in the same relationship context.
- Milestone scenes take priority when eligible and cannot be consumed as ordinary variants.
- Variants react to actual game state and preserve each NPC's voice.
- The implementation does not create hundreds of hardcoded prose strings in Core or Application.

---

## 7. Perform Geographic, Arabic-Language, Cultural, and Economic Continuity Passes

### Outcome

Cairo is internally consistent and locally specific. Arabic and English presentation is deliberate, institutions and routes make geographic sense, and economic values communicate a coherent 2060 model.

### Implementation steps

1. Establish one canonical source for NPC home/work locations and schedules. Refactor rumor and availability queries to derive district membership from that source instead of duplicating hardcoded lists.
2. Add validation that every location, district, NPC schedule stop, work site, and Ink location reference resolves to canonical content. Where prose cannot be parsed safely, maintain a small reviewed glossary/tag catalog rather than fragile free-text inference.
3. Correct known contradictions: Abu Samir's Ard al-Liwa/Bulaq workshop mismatch and the Sudanese background's legal-status introduction. Search for every reuse before deciding the canonical wording.
4. Create a compact setting bible covering district relationships, plausible travel times, transit modes, major institutions, roof/water/power governance, climate conditions, and which services are public, cooperative, informal, or private.
5. Create an Arabic-language style sheet with reviewed forms for names, honorifics, common terms, transliteration, pluralization, punctuation, and when untranslated Arabic is appropriate. Prefer clarity and context over decorative Arabic words.
6. Commission or request review from people with Egyptian/Cairene and Sudanese cultural competence before declaring the pass complete. Record disputed terms and decisions so later agents do not oscillate between spellings or assumptions.
7. Audit gender mechanics and prose for schematic or essentialist assumptions. Replace unconditional daily modifiers and broad sex-based occupation assumptions with situation-specific pressures only after approving the product-level rule change and updating requirements/tests.
8. Standardize currency display on one player-facing label (`LE` or `EGP`) while allowing authentic in-world speech where appropriate. Document whether values are compressed gameplay units or projected 2060 nominal amounts, then rebalance or explain them consistently.
9. Audit heat and rainfall language: maintain scarce average rainfall, hotter nights, recurrent heatwaves, water pressure, and rare drain-overwhelming rain. Remove generic wet-climate imagery or effortless adaptation.
10. Audit speculative technology against the guardrails: connectivity must fail sometimes, batteries age, repairs require labor, biometrics exclude, and models make bounded errors.
11. Treat the title `Slums` as a decision gate. Document alternatives and the framing risk, but do not rename assemblies, saves, or product branding without explicit approval and a migration plan.

### Required tests and review artifacts

- Canonical NPC/district mapping tests used by rumors, schedules, travel, and talk availability.
- JSON content validation for all referenced IDs.
- Search-based continuity checks for superseded location and currency terms where reliable.
- Full UI smoke tests after transliteration or text-width changes.
- A human review checklist signed off in the setting/language bible for central scenes, backgrounds, and endings.

Automated tests cannot certify cultural authenticity. They can prevent identifier drift and reintroduced contradictions; human review remains required for voice, framing, and language.

### Acceptance criteria

- NPC district information is derived from one canonical registry or content source.
- Known location and background contradictions are removed from source and compiled Ink.
- Currency and transliteration rules are documented and consistently applied.
- Central story scenes have completed geographic, language, and cultural review.
- Any gender-system or title change is accompanied by explicit requirements and migration updates.

---

## 8. Expand Endings Into Final Scenes That Resolve Relationships and Sacrifices

### Outcome

Endings remain grounded in the established simulation thresholds but become dramatic final scenes that ask for a last commitment and resolve the central crisis, mother relationship, key NPC arcs, and the costs of the player's route.

### Implementation steps

1. Preserve `EndingService` as the authority for route eligibility and failure endings. Failure endings should remain automatic when survival conditions demand them.
2. Change selectable long-term endings into a two-stage flow:
   - the ending menu selects an eligible commitment scene;
   - after the scene's final choice is applied, `EndingChoiceCommand` commits the ending and shows its resolved epilogue.
3. Add a pending-ending state so save/load works after entering a commitment scene but before the last decision. Prevent selecting a second ending while one is pending.
4. Give every non-failure route a costly final choice related to its central tension. The choice must trade resources, safety, status, independence, or a relationship—not simply restate the ending label.
5. Build a bounded reactivity matrix for each final scene. Include:
   - background and gender only where materially relevant;
   - strongest support contact and unresolved central-character conflicts;
   - mother's relationship, knowledge, preference, and outcome;
   - central-crisis resolution and cooperative condition;
   - clean/crime history and police pressure;
   - important technology capability or obligation;
   - the sacrifice made in the final choice.
6. Avoid combinatorial knot explosion. Use shared Ink gathers and conditional paragraphs for secondary callbacks, with route-specific core scenes for the dramatic action.
7. Let ambiguous outcomes remain ambiguous, but state concrete consequences: who has water or cooling, what debt remains, which relationship survived, and what work the player wakes to next.
8. Update ending artifact validation so every ending ID maps to an existing entry knot, every entry knot terminates, and no authored ending knot is orphaned.
9. Add a post-ending summary only if it complements rather than replaces the final scene. Prefer a few specific consequences over a ledger of every statistic.

### Required tests

- Eligibility tests for all existing ending IDs and all automatic failure endings.
- Application tests for pending commitment, cancellation rules if allowed, final application, and duplicate-selection prevention.
- Persistence tests immediately before and after the final choice.
- Narrative traversal of every final branch.
- Reactivity tests for mother state, strongest contact, crisis resolution, route history, and final sacrifice.
- Test proving the final choice changes persistent state before the ending is committed.
- Artifact validation proving no orphaned ending knots.

### Acceptance criteria

- Every selectable route has a playable commitment scene with a meaningful final decision.
- Every ending resolves the shared infrastructure crisis and at least the mother plus one central NPC relationship.
- The epilogue reflects actual stored decisions rather than inferring history from trust alone.
- Save/load during the finale is deterministic and cannot duplicate rewards or consequences.
- Existing failure conditions and ending IDs remain compatible unless the governing documents are explicitly revised.

---

## Cross-Package Engineering Checklist

For every work package:

1. Read the current implementation before adding abstractions.
2. Add or update Core rules and tests first when the feature changes simulation state.
3. Add Application commands/queries; do not let screens call new `GameSession` mutations directly.
4. Add snapshot capture/restore and backward-compatible defaults for every new persistent field.
5. Update `NarrativeSceneState`, Ink globals, tag parsing, and validators together so synchronization cannot drift.
6. Author or revise Ink source, then run `npm run compile-ink` from `src/Slums.Game` and commit the resulting `content/ink/main.json`.
7. Run focused tests during development and the full required validation workflow before handoff:

   ```text
   dotnet build Slums.slnx
   dotnet run --project tests/Slums.Core.Tests
   dotnet run --project tests/Slums.Application.Tests
   dotnet run --project tests/Slums.Game.Tests
   dotnet run --project tests/Slums.Infrastructure.Tests
   dotnet run --project tests/Slums.Narrative.Ink.Tests
   ```

8. Manually play at least one path affected by the package. Automated traversal catches broken knots, but it cannot assess pacing, voice, clarity, or emotional continuity.
9. Update `PLAN.MD`, `AGENTS.md`, and `MEMORY.MD` when architecture, workflow, or established product direction changes.

## Upgrade Completion Definition

The upgrade is complete only when all eight acceptance sections pass, the full build and test suite succeeds, all changed Ink is recompiled, no narrative fallback was introduced, and a human content review has covered the central spine, five principal characters, backgrounds, and endings. A high knot count or larger word count is not completion; the target is reachable content, truthful consequences, persistent decisions, distinct people, and a coherent month-long story.
