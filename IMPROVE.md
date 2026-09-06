# IMPROVE

Fix list from the mechanics/story review of the current vertical slice. Each item states the problem, evidence, the fix, the tests, and the docs to update. Items are ordered: cheap corrections first, then economy, then content, then the largest new feature. Implement in order unless a dependency forces otherwise; items 3 and 4 (economy) should be validated together with seeded runs.

Follow the rules in `AGENTS.md`: TUnit + FluentAssertions + NSubstitute, fail-fast content, warnings as errors, mutations routed through `Slums.Core` services and `Slums.Application` commands, no business logic in `Slums.Game`. The daily pipeline's execution order and random-draw sequence are observable behavior; deliberate rule changes here are allowed but must update the pipeline-order and seeded-run tests in the same change.

Validation workflow after every item:

```bash
dotnet build Slums.slnx
dotnet run --project tests/Slums.Core.Tests
dotnet run --project tests/Slums.Application.Tests
dotnet run --project tests/Slums.Game.Tests
dotnet run --project tests/Slums.Infrastructure.Tests
dotnet run --project tests/Slums.Narrative.Ink.Tests
```

If Ink sources change: run `npm run compile-ink` from `src/Slums.Game` first and commit the regenerated `content/ink/main.json`.

---

## 1. Fix the dead Wednesday investment bonus

**Problem.** Weekly investment resolution only runs on Monday (`DailyEconomyResolution.cs:55`), and `InvestmentPurchaseService.ResolveWeekly` reads the *current day's* schedule (`InvestmentPurchaseService.cs:97,118`). Monday's `InvestmentRevenueModifier` is 0; only Wednesday's is 1 (`DayScheduleRegistry.cs:85`). The Wednesday +1 income modifier can never apply. `REQS.md` also contradicts itself: the day-of-week table says investments distribute on Wednesday, while the Weekly Economy Resolution section says "on Mondays".

**Decision (made).** Split the weekly cycle, matching REQS as written: household-asset neglect and NPC economy resolution stay on **Monday** (per the Weekly Economy Resolution section); investment resolution moves to **Wednesday** (per the day-of-week table, which also carries the +1 restock modifier).

**Change.**

- In `DailyEconomyResolution.ResolveWeeklyCycle`, keep `ResolveWeeklyHouseholdAssets` and `ResolveWeeklyEconomy` behind the Monday gate; move `ResolveWeeklyInvestments` behind a Wednesday gate (`GameSession.GetCurrentDayOfWeek() != GameDayOfWeek.Wednesday`).
- Do not otherwise reorder pipeline steps; investment resolution keeps its position relative to the other economy blocks, just gated to a different day.
- Update the `DailyEconomyResolution` XML doc comment (it currently says "the Monday weekly block").

**Tests.**

- Pipeline test: investments resolve on Wednesday, not Monday; household assets and NPC economy still resolve on Monday.
- Resolution test: a profitable investment resolved on Wednesday pays `base + 1` (the modifier path at `InvestmentPurchaseService.cs:118` is now reachable).
- Update any seeded-run fixtures that assert money on Mondays.

**Docs.** `REQS.md` Weekly Economy Resolution section: state that NPC wealth events resolve Mondays and investment returns distribute Wednesdays. `MEMORY.MD` EndDay step list: change "On Mondays: weekly household asset neglect, investment resolution, NPC economy resolution" to reflect the split.

**Acceptance.** A seeded run shows investment payouts arriving Wednesday with the +1 restock modifier and no payout on Monday.

---

## 2. Make investments worth buying inside a normal run

**Problem.** Costs are 100–300 LE with 5–50 LE/week returns, so payback takes 5–20 weeks. PLAN pacing targets route commitment by day 21–35 and good endings unlock at day 30, so only `HashishCourier` ever breaks even; every other investment is a trap choice.

**Decision (made).** Two-part fix: raise weekly income so midpoint payback lands at 5–8 weeks, and give each investment a small ongoing perk so it has value before pure payback. Perks stay bounded and must be visible in previews (PLAN priority: preview values and committed outcomes on the same calculation path).

**Proposed income ranges** (keep costs and risk profiles unchanged; tune after seeded runs):

| Investment | Cost | Old income | New income | Midpoint payback |
|---|---|---|---|---|
| TeaCart | 100 | 5-8 | 13-18 | ~7 wk |
| FoulCart | 150 | 8-12 | 16-22 | ~8 wk |
| PhoneChargingStation | 160 | 8-14 | 20-27 | ~7 wk |
| ScrapCollection | 180 | 15-22 | 26-34 | ~6 wk |
| HerbalRemedyTrade | 180 | 10-16 | 24-32 | ~6 wk |
| MicroLaundry | 200 | 10-15 | 22-30 | ~8 wk |
| MarketStall | 220 | 20-30 | 32-44 | ~6 wk |
| SewingSideBusiness | 220 | 14-20 | 30-40 | ~6 wk |
| Kiosk | 250 | 18-25 | 32-42 | ~7 wk |
| CafeSupplyPartnership | 250 | 16-24 | 34-45 | ~6 wk |
| HashishCourier | 300 | 35-50 | 48-64 | ~5 wk |

**Proposed perks** (one each, applied weekly while the investment is active and not suspended; each perk application must produce an event-log entry):

| Investment | Perk |
|---|---|
| FoulCart | Food staples cost -1 LE while active |
| MicroLaundry | +1 trust per week with LaundryOwnerIman |
| ScrapCollection | 25% chance per week of +1 spare part (respects part cap) |
| Kiosk | Phone credit refill costs -2 LE |
| MarketStall | Food cost -1 LE in the district where purchased |
| HashishCourier | none (income is the draw; keep risk events) |
| TeaCart | Rooftop tea circle invitation arrives 1 day sooner |
| PhoneChargingStation | Phone credit refill costs -2 LE |
| HerbalRemedyTrade | Medicine purchases -5 LE |
| SewingSideBusiness | WorkshopSewing pay +2 LE |
| CafeSupplyPartnership | Entertainment stress relief +2 at cafe locations |

**Change.**

- Update `InvestmentRegistry` income bounds (and any JSON/content mirror if one exists).
- Add perk evaluation to the weekly investment resolution path in `InvestmentPurchaseService` (money-affecting perks via the same auto-transaction event-log pattern; state-affecting perks via the existing relationship/phone/pricing services).
- Expose perk + expected income in the investment preview query so `Slums.Application` and the UI show them before purchase.

**Tests.**

- Registry validation: every investment's midpoint payback is within the 5-8 week target (this test makes future rebalancing deliberate).
- Perk tests: one per perk type, asserting effect, event-log entry, and no effect while suspended.
- Preview/commit parity test: the preview query reports the same numbers the commit path applies.
- Seeded day-35 run: an investor strategy (buy TeaCart day ~5, FoulCart when affordable) ends measurably ahead of a no-investment run and never causes an unintended eviction.

**Docs.** `REQS.md` Small Business Investments: replace "Returns should be modest and realistic" with the payback expectation ("midpoint payback 5-8 weeks"), add the perk column, keep the 100-300 LE cost band. `MEMORY.MD` Investments table: new numbers plus perks. `PLAN.MD` Current Priorities item 2 can note this audit is done once it ships.

**Acceptance.** Investments are purchase-worthy in a 35-day run: at least three non-crime investments break even or profit before day 35 with their perks counted.

---

## 3. Restore honest-work viability at the baseline

**Problem.** Daily rent is 20 LE (`RecurringExpenses.cs:5` = 140/week) while base shift pay is 18-27 LE for 5-8 hour shifts, plus 5-15 LE food and 2 LE transit. A single-shift honest day runs a deficit before variants unlock, and the energy budget (overnight +15, shifts 15-32 each) makes the required double-shift loop punishing. The honest route is non-viable at the baseline.

**Decision (made).** Tune pay upward rather than rent downward: rent drives the eviction clock and the eviction drama; pay drives the work feeling. Target heuristic: **one standard base shift ≈ one day's rent + one cheap meal + one transit fare, with pay variance straddling the line.** Today that line is 20 + 5 + 2 = 27 LE; the lowest jobs sit at 18-20.

**Change.**

- Raise the four lowest-paying jobs in both `content/data/jobs.json` and the `JobRegistry` defaults (the two must stay in sync; check which source wins at load and keep both consistent — content JSON is authoritative at bootstrap):
  - HouseCleaning 18 → 23
  - StreetVending 19 → 23
  - MarketPorter 19 → 23
  - FishSorter 20 → 24
- Leave the mid-tier (21-25) unchanged for now; variant progression and Saturday/weekday modifiers should lift those above the line as reliability grows. If seeded runs still show deficit weeks at reliability 60+, apply a smaller bump (+2) to BakeryWork and WorkshopSewing.
- Keep eviction math (7 unpaid days), warnings (day 3/5), and `DailyRentCost = 20` untouched.

**Tests.**

- Content/registry parity test: `jobs.json` values match `JobRegistry` defaults (add if missing).
- Economy test: for every job, `BasePay - PayVariance >= DailyRentCost + CheapMealCost` minus a documented tolerance, so future job edits cannot silently re-break the heuristic.
- Seeded honest-route acceptance run (day 1-10): a competent player working one shift most days and eating cheap finishes with non-negative money and no unpaid-rent days.

**Docs.** `PLAN.MD` priority 2 (economy audit): record the pay heuristic so future tuning keeps the invariant. `MEMORY.MD` if any headline numbers are quoted there.

**Acceptance.** Seeded runs across all three backgrounds survive the first week on the honest route with informed choices (this is already an acceptance target in PLAN priority 1 — the fix makes it actually pass).

---

## 4. Make seasonal/holiday content reachable

**Problem.** Day 1 is October 1. The first calendar holiday is Coptic Christmas at day ~99, Ramadan at ~151, with endings unlockable at day 30 and pacing targeting day 21-35 commitment. The Ramadan fasting mechanic, Eid gift pressure, Sham el-Nessim, and the seasonal iftar event are dead content in normal runs; players see only autumn weather. Separately, `CommunityEventDefinition.IsSeasonal` is set on `MulidFestival` (`CommunityEventRegistry.cs:88`) but never consumed anywhere — the flag is inert.

**Decision (made).** Do not move the calendar or raise ending gates. Instead: (a) anchor the mulid to fixed calendar days so at least one seasonal street festival lands inside the standard run window, and (b) let witnessed holidays flavor ending epilogues so extended runs that reach Ramadan/Eid get narrative payoff.

**Change.**

- Give `IsSeasonal` meaning: add a fixed calendar anchor to `CommunityEventDefinition` (e.g., `AnchorDays` or a small `CommunityEventCalendar` in `Slums.Core.Calendar`) and gate `MulidFestival` availability to those days. Proposed anchors: day 23-24 (autumn mulid, inside the standard run) and day 110-111 (winter mulid, for extended runs). The HUD/community screen must show why the event is unavailable ("returns during the mulid season") instead of hiding it silently.
- Keep the existing pickpocket risk, costs, and trust effects of `MulidFestival` as authored.
- Add holiday-witnessed story flags: when the player attends a holiday-tied event or experiences a holiday effect (iftar gathering, Eid gift payment, Coptic Christmas gathering, Sham el-Nessim), set a persistent flag (`holiday_witnessed_<id>` or per-holiday flags via the existing FLAG outcome pattern).
- Extend ending epilogue selection so `EndingService`/Ink can vary epilogue prose when those flags exist (e.g., the Luxor departure epilogue references the last mulid or Ramadan the player lived through). Ink conditionals only; no new ending identifiers.
- If authored scenes are wanted for the mulid day itself, reuse the existing mulid Ink scene; add new knots only if cheap. Compile Ink and commit `main.json` if touched; extend `StoryArtifactValidationTests` for any new knot.

**Tests.**

- Calendar test: `MulidFestival` is available exactly on its anchor days and unavailable otherwise, with a reason string surfaced in the query.
- Flag test: attending each holiday event sets its flag exactly once.
- Ink artifact test: epilogue knots referenced by holiday-flag conditionals exist (already covered by the orphan-knot validation; extend the catalog if new knots are added).
- Seeded run: the day-23 mulid appears in a standard run and attending it applies the authored trust/stress/pickpocket effects.

**Docs.** `REQS.md` Community Solidarity Events table: replace Mulid's "Seasonal" frequency with the fixed anchors. `MEMORY.MD`: note the anchor mechanic and the holiday-witnessed epilogue flags.

**Acceptance.** A standard 30-40 day run contains at least one seasonal festival with authored effects; a day-150+ run shows Ramadan mechanics still working and references them in its epilogue.

---

## 5. Implement the no-snitching street code

**Problem.** `REQS.md` requires a "strong no-snitching street code, especially around police contact and retaliation," but nothing models being *seen* with Officer Khalid while active on the crime route. Rumors, faction reputation, and retaliation all exist as systems; the street code is the missing rule connecting them.

**Design.**

- **Observation.** When a conversation with `OfficerKhalid` completes while the player has committed crimes with the last crime within 7 days (reuse the double-life signal in `NarrativeSignalRules.cs:61`), roll observation: base 40%, reduced to 15% if the player worked a public honest shift the same day (mirrors the thin-alibi rule), +10% if home-district heat >= 50. Home location is exempt (private). Seeded roll via `GameRandom`.
- **Rumor.** A successful observation spawns the tenth rumor type `SeenWithPolice` ("Someone saw you talking to the police") at intensity 6-8 in the district where it happened. Existing propagation carries it to criminal-contact NPCs (Hanan, Youssef, UmmKarim), whose trust drops via the standard rumor trust-modifier path.
- **Faction consequence.** On observation, faction reputation -5 with the faction where the player holds the highest standing (or the home district's controlling faction if none stands out). High-trust Khalid relationships (the Information Network's trust >= 20 police tips) halve the observation roll — the street knows he feeds the player tips, but known arrangements read as informancy at half strength.
- **Retaliation.** If the rumor reaches a criminal contact while faction reputation with the offended faction is below 0, trigger a one-time-per-faction retaliation scene: an Ink knot (implication and menace only — respect the content boundaries; no graphic detail) where the player pays 10-25 LE, loses 3 days of crime-route access, and takes stress +8. Refusing the payment costs -10 faction reputation instead and locks crime routes 5 days. The scene queues through the existing narrative trigger mechanism and applies outcomes via the standard tagged-outcome path.
- **Warning shot.** At criminal-contact trust >= 10, the tip pool gains a warning variant ("The street watches who walks with Khalid") before any observation has happened, so the rule is discoverable, not a gotcha.

**Change.**

- `RumorId`: add `SeenWithPolice`; wire generation into the conversation-completion path for Khalid in `Slums.Core` (wherever conversation mutations live), not in the UI.
- Retaliation scene: new Ink knot + outcome tags; queue via `NarrativeSceneTrigger`; once-per-faction guard persisted in story flags or territory state.
- Crime-route lockout: extend the existing route-availability evaluator with a temporary lock reason that the menu context can surface ("The corner is cold toward you").
- Event journal entries for observation, reputation loss, and retaliation payment.

**Tests.**

- Observation roll: probabilities under seeded randomness (base, alibi, heat, Khalid-tip, home exemption, no-recent-crime exemption).
- Rumor: `SeenWithPolice` propagates and applies trust modifiers to criminal contacts.
- Faction: reputation penalty applied once per observation; retaliation fires at most once per faction; refusal path locks routes with a visible reason.
- Ink artifact: retaliation knot exists and its outcome tags parse.
- Seeded crime-route run: commit crimes, talk to Khalid twice, observation eventually fires; ending checks unaffected.

**Docs.** `REQS.md` Crime Path: add a "No-Snitching Street Code" subsection describing the rule as designed above. `MEMORY.MD`: rumor list 9 → 10 types, plus a short paragraph on the street code.

**Acceptance.** A crime-route player who courts police contact suffers visible social and faction consequences inside the same run, with warning tips available beforehand; an honest-route player is unaffected.

---

## 6. Documentation drift cleanup

Small corrections so future agents stop tripping on them. No code changes expected except verifying against the registry.

- `MEMORY.MD` Content Data Files table says `jobs.json` holds 12 job shift definitions; the file has 13 (includes `RoboticsScavenging`). Fix the count.
- Eid al-Adha duration: `REQS.md` says 3 days, `MEMORY.MD` says 4. Check `HolidayRegistry.cs` and align both docs to the code (or fix the code if it disagrees with the intended 3).
- `REQS.md` line ~103: "working at a forn/bakery" → "working at a forn (bakery)".
- Romance subplot: REQS marks it optional and nothing implements it. Record the decision explicitly — add one line to `PLAN.MD` Current Priorities or Content Boundaries: "Romance remains an optional deferred subplot; do not build it without a product decision." This prevents other agents from either silently dropping the requirement or inventing scope.
- While in there, confirm the `IsSeasonal` inert-flag note is resolved by item 4.

**Tests.** None required beyond whatever the Eid duration check forces.

**Acceptance.** `grep`-level consistency: the numbers in `REQS.md`, `MEMORY.MD`, and the registries agree on job count, Eid duration, and mulid anchoring.

---

## Suggested implementation order

1. Item 6 (docs) — fast, removes contradictions before other work starts.
2. Item 1 (investment day fix) — small, unblocks item 2's numbers.
3. Item 3 (honest-work tuning) + item 2 (investment retune) — do together; both need the same seeded-run harness and are validated as one economy.
4. Item 4 (seasonal reachability) — content work, independent.
5. Item 5 (street code) — largest new feature; last so the rumor/tip/retaliation systems are stable underneath it.

## Global acceptance checklist

- Build and all five test suites pass.
- Seeded day-35 runs, both identities, all three backgrounds: honest route survives week one; Wednesday investment payouts visible; mulid fires ~day 23.
- A seeded crime run demonstrates the street-code rumor and retaliation path.
- `REQS.md`, `MEMORY.MD`, `PLAN.MD`, and the registries tell the same story on every number touched above.
