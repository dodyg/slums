# MEMORY.MD

This document explains the game architecture and simulation mechanics for future agent sessions. Read this alongside REQS.md, PLAN.MD, and AGENTS.md.

## Project Overview

**Slums** is a text-driven survival RPG set in contemporary Cairo slums. The player controls a young woman (20-29) who must balance survival needs, care for her widowed mother, and navigate moral choices between honest underpaid work and risky criminal activity.

## Solution Architecture

```
src/
  Slums.Core/           # Domain layer - pure simulation rules, no dependencies
  Slums.Application/    # Orchestration, contracts, use cases
  Slums.Infrastructure/ # JSON persistence, content loading, adapters
  Slums.Narrative.Ink/  # Ink story runtime, variable sync
  Slums.Game/           # SadConsole UI, screens, input, DI wiring
tests/
  Slums.Core.Tests/
  Slums.Application.Tests/
  Slums.Game.Tests/
  Slums.Infrastructure.Tests/
  Slums.Narrative.Ink.Tests/
content/
  ink/                  # .ink source files, compiled to main.json
  data/                 # JSON: backgrounds, jobs, locations, random_events, district_conditions, plants, pets
```

**Dependency Direction**: Always inward. `Slums.Core` references nothing. UI never contains business logic.
`Slums.Application` owns player-facing action commands and menu/action queries; `Slums.Game` renders and delegates rather than mutating `GameSession` directly for player actions.
`GameSession` is the canonical runtime boundary, and persistence crosses that boundary through snapshots plus `LoadedGameSession`.
`GameSession` delegates orchestration-heavy pieces to focused core collaborators such as `CrimeNarrativePlanner`, `WorkNarrativePlanner`, `NarrativeFollowUpPlanner`, `InvestmentEligibilityEvaluator`, and `InvestmentResolutionCalculator`.
Shared signal helpers like `NarrativeSignalRules` live in `Slums.Core.Narrative` so both the core runtime flow and application menu/status queries reuse the same rules.
Application-layer context builders prefer narrow snapshots over carrying the full `GameSession` when a smaller view is enough.
Repo-owned JSON content in `content/data/` is authoritative during app bootstrap; missing or invalid files fail fast.

## Core Simulation Loop

### GameSession (src/Slums.Core/State/GameSession.cs)

The canonical runtime boundary for an active run, backed internally by EntitiesDb. Key state groups:

- **Clock**: Day/hour/minute tracking, time-of-day, day-of-week, season, holiday state, Ramadan
- **Player**: Stats (SurvivalStats), Nutrition, Household care, Household assets, Skills, Home upgrades
- **World**: Current location, district conditions
- **Relationships**: 13 NPC trust levels, 3 faction reputations, conversation pool tracking
- **Economy**: Debts (4 sources), NPC economies, investments
- **Crime/Law**: District heat (per-district police pressure), territory control, crime counters
- **Information**: Phone state/messages, tips, rumors
- **Progress**: Job progress, event history, story flags, community event attendance, training
- **Narrative**: Pending scene queue, ending knot
- **Diagnostics**: Mutation record list with GameMutationRecord entries

### Time Flow

1. **AdvanceTime(minutes)**: Advances clock, triggers EndDay at 22:00
2. **EndDay()** performs ~40 steps in order:
   - Apply daily stat decay (Hunger -12, Energy -10, Stress +3)
   - Apply season modifiers, weather modifiers, holiday effects, Ramadan fasting
   - Resolve nutrition (satiety -15, compute energy/health/stress from meal quality and undereating streak)
   - Calculate overnight sleep recovery via SleepQualityCalculator (stress, nutrition, mother condition, rent anxiety, home upgrades, season bonus)
   - Apply home upgrade stress bonus
   - Resolve household care (mother health based on feeding, medication, checking)
   - Process rent (pay or accumulate debt, issue warnings, narrative triggers)
   - Decay all district heat, apply heat bleed-over between adjacent districts
   - Apply daily territory decay (tension -2), roll territory events
   - Background-specific effects (medical dropout stress, refugee heat, prisoner decay)
   - Resolve sellable plant income
   - Advance clock to next day (Day++, reset to 06:00, travel to Home)
   - Update Ramadan state, roll district conditions, roll new weather
   - On Mondays: weekly household asset neglect, investment resolution, NPC economy resolution
   - Reset daily flags (nutrition, household, training, crime)
   - Check community event skip consequences (3+ skips: concern, 5+ skips: trust penalties)
   - Roll daily random events, street cat encounter
   - Queue narrative follow-up scenes
   - Propagate and decay rumors, generate skip rumors
   - Process daily debt (loan shark penalties/escalation)
   - Process daily phone (credit drain, generate messages, expire old)
   - Process daily tips (generate, deliver via phone, apply ignore erosion)
   - Check ending conditions
   - Record EndDay mutation

### Player Character

**SurvivalStats** (0-100 scale):
- Money (can go to 0, tracks earnings)
- Hunger (mapped from NutritionState.Satiety)
- Energy (restored by sleep, consumed by work/travel)
- Health (decays from starvation, injury, events)
- Stress (increases from pressure, crime, poverty)

**NutritionState**:
- Satiety: 0-100, decays 15/day during EndDay resolution
- DaysUndereating: tracks consecutive poor meals
- MealQuality: None/Scraps/Basic/HotMeal
- Skipping meals drains energy, increases stress, eventually damages health

**HouseholdCareState**:
- MotherHealth: 0-100, death at 0 triggers MotherDied ending
- MotherCondition: Stable(65+)/Fragile(30-64)/Crisis(0-29)
- StaplesUnits/FoodStockpile, MedicineStock
- Daily flags: FedMotherToday, MedicationGivenToday, CheckedOnMotherToday

**HouseholdAssetsState**:
- Pets: up to 3 cats or fish tanks, passive mother health bonus
- Plants: up to 10 plants (15 types across culinary herbs, sellable herbs, flowers, medicinal)
- Weekly care costs deducted automatically; neglect pauses bonuses but nothing dies
- Sellable herbs generate harvest income automatically

**HomeUpgradeState**:
- 4 upgrades: CleanBedding (25 LE), Fan (40 LE), WindowScreen (20 LE), Curtain (15 LE)
- Each provides energy recovery, stress reduction, or privacy bonuses
- Bonuses apply to sleep recovery calculation

**SkillState** (0-10 scale):
- Medical: Unlocks clinic variants, reduces medicine cost
- Persuasion: Improves trust gains, unlocks call center/dispatch variants
- StreetSmarts: Lowers crime detection, unlocks crime variants
- Physical: Reduces energy cost for manual labor jobs

## Three Playable Backgrounds

Defined in `content/data/backgrounds.json`:

1. **MedicalSchoolDropout**: High stress, Medical 3, better clinic outcomes, +3 stress when mother health <60
2. **ReleasedPoliticalPrisoner**: Low money, higher police scrutiny (half heat decay), Persuasion 2 + StreetSmarts 1
3. **SudaneseRefugee**: Accent friction in Dokki (+2 stress walking there, Dokki baseline heat 10), StreetSmarts 2 + Physical 1, community support

## Locations and Districts

### Districts (6)
- **Imbaba**: Home district, market, bakery, fish market
- **Dokki**: Call center, square, cafe, plant shop - more expensive, refugee stress
- **ArdAlLiwa**: Clinic, workshop - informal settlement
- **BulaqAlDakrour**: Pharmacy, microbus depot
- **Shubra**: Laundry - crowded northern district
- **DowntownCairo**: No current locations, referenced for events

### Locations (13)
| Location | District | Key Facilities |
|---|---|---|
| Home (Your Apartment) | Imbaba | Rest, cooking, mother care, rooftop exercise, clinic travel |
| Market (Souk Al-Gom'a) | Imbaba | Food shopping, HasCafe, crime (Hanan/UmmKarim) |
| Bakery (Al-Forn Al-Baladi) | Imbaba | BakeryWork job |
| FishMarket (Wikalet Al-Samak) | Imbaba | FishSorter job |
| CallCenter (TechConnect Office) | Dokki | CallCenterWork job |
| Square (Midan Al-Tahrir) | Dokki | HasBar, crime (Youssef), VendorTarek |
| Cafe (Ahwa El-Galaa) | Dokki | CafeService job, HasCafe, entertainment |
| PlantShop (Mashrabiya Stall) | Dokki | Buy plants |
| Clinic (Rahma Clinic) | ArdAlLiwa | Mother treatment, ClinicReception job |
| Workshop (Abu Samir Sewing) | ArdAlLiwa | WorkshopSewing job, crime (contraband) |
| Pharmacy (Saidaleya Al-Nahda) | BulaqAlDakrour | Medicine shopping, HasCafe, PharmacyStock job |
| Depot (Bulaq Microbus Depot) | BulaqAlDakrour | MicrobusDispatch job, HasBilliards |
| Laundry (Shubra Steam Laundry) | Shubra | LaundryPressing job, HasBilliards |

## Travel

**Paid Travel**: Costs 2 LE per trip, consumes energy and time based on distance.

**Walking**: `TryWalkTo()` allows free travel at 3x energy cost, 3x time, and +3 stress vs paid travel. SudaneseRefugee gets additional +2 stress walking to Dokki.

## Day-of-Week Rhythms

Egyptian week (Saturday-Friday). `GameClock.GetDayOfWeek()` derives day from calendar day.

| Day | Effects |
|---|---|
| Friday | Markets partially closed, Bakery/CallCenter/CafeService blocked, prayer gathering available, crime detection -10%, foul cart revenue x2 |
| Saturday | Food costs -2 LE, laundry pay +2 LE |
| Monday | Clinic discount -5 LE |
| Wednesday | Investment returns distributed |
| Other days | No modifiers |

## Seasonal Calendar

Game starts October 1, 2024. Seasons determine weather probabilities, food costs, energy drain, and more.

| Season | Key Effects |
|---|---|
| Autumn (start) | No modifiers |
| Winter | Food cost -2, rest recovery +3, illness frequency x1.5 |
| Spring | Food cost +2, investment returns +10% |
| Summer | Energy drain +5, stress +3, outdoor work stress +3 |

### Holidays (5)
- **CopticChristmas** (Jan 7): Job pay +5, stress -5
- **Ramadan** (Feb 28, 30 days): Fasting choice mechanic (energy -5, stress +3, trust +1 with religious NPCs, job pay -10%)
- **EidAlFitr** (Mar 30, 3 days): Stress +5, trust with neighbors -3
- **ShamElNessim** (Apr 21): Stress -8
- **EidAlAdha** (Jun 6, 4 days): Mother health +5, food cost +5

## Weather System

Each day rolls weather based on season via `WeatherRoller`. 7 types:

| Weather | Key Effects |
|---|---|
| Clear | No modifiers |
| Hot | Energy drain +5, stress +3, food cost +2, crime detection +5 |
| Heatwave | Energy drain +10, stress +5, health damage if energy low, blocks outdoor jobs |
| Khamsin | Energy drain +8, stress +5, travel cost +5, blocks outdoor jobs AND crime |
| CoolOvercast | Stress -2, food cost -2, crime detection -5 |
| Rain | Food cost +5, blocks travel to flood-prone areas |
| Windy | Energy drain +2, crime detection -5 |

## Honest Work System

### Job Types (12)
BakeryWork, HouseCleaning, CallCenterWork, ClinicReception, WorkshopSewing, CafeService, PharmacyStock, MicrobusDispatch, LaundryPressing, StreetVending, FishSorter, MarketPorter

### Job Variants
Each job has multiple variants unlocked by:
- **Reliability**: 55/60/70/75/80 thresholds
- **NPC Trust**: 10/20 thresholds with employer
- **Skills**: Physical 2/3, Persuasion 2, Medical 2/3
- **Background**: Medical dropout gets clinic advantages

### Mistake System
Jobs can trigger mistakes based on high stress (60-65+) or low energy. Mistakes cause reduced pay, extra stress, reliability loss (-10 to -15), temporary lockout (1-2 days), and employer trust loss (-4).

## Crime System

### Crime Types (10)
- **Basic** (available everywhere): PettyTheft, Robbery, HashishTrade
- **NPC-gated**: MarketFencing (Hanan trust 10), DokkiDrop (Youssef/Dokki rep), NetworkErrand (UmmKarim + Imbaba rep), DepotFareSkim (Safaa/Imbaba rep), ShubraBundleLift (Iman/Imbaba rep), WorkshopContraband (AbuSamir + ExPrisoner rep), BulaqProtectionRacket (Safaa/Imbaba rep)

### Crime Resolution
Preview calculates detection/success chances. StreetSmarts 3 reduces detection by 10%. Police pressure increases risk. Same-day public work provides thin alibi. Released prisoner: increased scrutiny.

## District-Specific Heat

Replaces single global PolicePressure with per-district tracking (`DistrictHeatState`).

- Each district has its own heat level and decay rate (Dokki: 6, Imbaba: 4, Bulaq/Shubra/ArdAlLiwa: 3)
- Heat bleeds between adjacent districts (Imbaba-Bulaq: 15%, Imbaba-Dokki: 5%, ArdAlLiwa-Imbaba: 10%, ArdAlLiwa-Dokki: 5%)
- `PolicePressure` property returns max heat across all districts
- Backgrounds modify heat: SudaneseRefugee sets Dokki baseline 10, ReleasedPoliticalPrisoner halves decay rate
- At 100 global pressure: Arrested ending. At 80+: warning scene.

## Territory Dynamics

Each district tracks faction influence (ImbabaCrew, DokkiThugs, ExPrisonerNetwork) and tension (0-100).

- **Controlling faction**: >50 influence in a district
- **Tension levels**: Normal/Elevated/High/Dangerous
- Crime increases tension; honest work reduces it. Daily decay: -2.
- High tension: food price increases. Dangerous: blocks all crime.
- Territory events (street arguments, crossfire, police crackdowns, flips, refugee solidarity) trigger at elevated/dangerous tension.

## Relationships and Factions

### NPCs (13)
1. LandlordHajjMahmoud (Home) - rent, landlord loans, investment access
2. FixerUmmKarim (Market/Square) - crime contact, persuasion training, investments
3. OfficerKhalid (Square, or any when heat >=50) - police pressure
4. NeighborMona (Home) - community, neighbor loans
5. NurseSalma (Clinic) - medical training, clinic access
6. WorkshopBossAbuSamir (Workshop) - employer, contraband crime
7. CafeOwnerNadia (Cafe) - employer
8. FenceHanan (Market) - fencing crime
9. RunnerYoussef (Square) - crime contact, market stall investment, street dice training
10. PharmacistMariam (Pharmacy) - employer
11. DispatcherSafaa (Depot) - employer, crime contact
12. LaundryOwnerIman (Laundry) - employer, crime contact, micro-laundry investment
13. VendorTarek (Square) - street vending job access, trust 15 unlocks

### NpcRelationship Tracks
- Trust: -100 to 100
- SeenConversationKnots: prevents dialogue repetition
- WasHelped, HasUnpaidDebt, WasEmbarrassed: relationship memory flags
- LastRefusalDay, RecentContactCount: timing-based reactivity
- FactionReputation: ImbabaCrew, DokkiThugs, ExPrisonerNetwork

### Conversation Pool System
- Each NPC has 100 unique dialogue knots per context
- Context determined by: trust level, police pressure, money, mother health, double-life status, recent events
- Seen knots are excluded, ensuring 100 days without repetition

## Phone System

Player has a basic mobile phone (`PhoneState`) with weekly prepaid credit (7 days, 5 LE refill).

- Phone can be lost (random event), recovered, or replaced (30 LE)
- `PhoneMessageGenerator` creates messages daily: criminal opportunities (20%), heat warnings (heat>60), family alerts (mother health<40, 30%), network requests (10%, faction-based), background-specific (15%)
- Messages expire, can be responded to or ignored
- Ignoring 3+ messages from same sender costs trust
- When phone is non-operational, pending messages are marked as missed

## Debt and NPC Economy

### Player Debt (4 sources)
- **NeighborLoan**: Borrow from NPCs with trust >=10
- **LandlordAdvance**: 50-100 LE, added to rent debt
- **LoanShark**: 100-300 LE, 20-30% weekly interest, 7-day due. Critical state triggers Destitution ending.
- **CommunityMutualAid**: Community-based lending

Loan shark escalation: daily stress/health penalties, violence ending at critical state (14+ days overdue).

### NPC Economy (`NpcEconomyState`)
Per-NPC wealth tracking (Struggling/Poor/Stable/Comfortable). Weekly resolution: NPCs with financial hardship affect player. Player can lend to NPCs for trust gain.

## Rumors and Word of Mouth

9 rumor types (CrimeSuccess, CrimeDetected, RentUnpaid, ExpensivePurchase, SeenWithCriminal, DevotedDaughter, SkippingCommunityEvents, HighDistrictHeat, FlashingMoneyAround).

- Rumors propagate to NPCs based on trust networks over 1-3 days
- Positive rumors decay -3/day, negative -2/day. Expire after 4 days or intensity reaches 0.
- When an NPC hears a rumor, their trust is modified by the rumor's TrustModifier

## Information Network (Tips)

5 tip types: PoliceTip, JobLead, MarketIntel, CrimeWarning, PersonalWarning.

- Generated daily based on NPC relationships, district heat, NPC economies, background, crime history
- Delivery: Emergency (immediate), InPerson (same district), Phone (if operational)
- Ignoring 3+ tips from same NPC (trust >=10) costs -1 trust

## Community Solidarity Events

5 events: FridayRooftopGathering, RamadanIftarSharing, NeighborhoodCleanup, RooftopTeaCircle, MulidFestival.

- Attendance tracked with consecutive skips
- Skipping 3+: neighbor concern or trust loss. Skipping 5+: trust penalties with Mona, UmmKarim, Salma
- Tea circle requires NPC invitation. Mulid has pickpocket risk (10%).
- Community events provide information tips.

## Entertainment Activities

6 activities gated by location facilities (cafe/bar/billiards):

| Activity | Cost | Time | Stress | Location |
|---|---|---|---|---|
| Coffee | 8 LE | 30 min | -8 | Cafe locations |
| Shisha | 30 LE | 90 min | -15 | Cafe locations |
| Billiards | 25 LE | 60 min | -12 | Depot, Laundry |
| BarDrinking | 65 LE | 120 min | -20 | Square |
| FootballWatching | 14 LE | 120 min | -18 | Cafe locations |
| SocialHangout | 5 LE | 60 min | -12 | Cafe locations |

## Skill Training

4 training activities, once per day each, gated by NPC trust and evening hours (18:00-22:00):

| Activity | Skill | Cost | Energy | NPC Required |
|---|---|---|---|---|
| StudyMedical | Medical | 0 LE | 18 | NurseSalma trust 10 |
| PracticePersuasion | Persuasion | 10 LE | 15 | UmmKarim trust 5 |
| StreetDice | StreetSmarts | 15 LE | 12 | Youssef trust 5 |
| RooftopExercise | Physical | 0 LE | 20 | None (requires Home) |

Skills also gain passively from work (~70% of progression) and crime activities.

## Investments

6 investment types with weekly passive income:

| Investment | Cost | Risk | Income/Week | Access |
|---|---|---|---|---|
| FoulCart | 150 LE | Low | 8-12 LE | HajjMahmoud trust 30 |
| MicroLaundry | 200 LE | Low | 10-15 LE | Iman |
| ScrapCollection | 180 LE | Medium | 15-22 LE | StreetSmarts/ExPrisoner |
| Kiosk | 250 LE | Medium | 18-25 LE | UmmKarim trust 40 |
| MarketStall | 220 LE | Medium-High | 20-30 LE | Youssef trust 25 |
| HashishCourier | varies | High | varies | Crime route |

## Random Events

30 events in `RandomEventRegistry` (code-defined; JSON data contains a matching subset), filtered by:
- MinDay: event cannot fire before day N
- Condition(GameSession): custom eligibility check
- District/location restrictions
- Event history gating (prevents immediate repetition)

Each event has weight, stat effects, optional Ink knot for narrative scene.

## Clinic Travel System

Integrated travel+visit from Home:
1. Player selects "Take Mother to Clinic"
2. UI shows clinics with costs, travel time, open status
3. System deducts travel cost/energy, advances time, travels to clinic
4. Performs clinic visit (deducts cost, heals mother)

Clinic locations: Rahma Clinic (ArdAlLiwa, 35 LE base), Saidaleya Pharmacy (Bulaq, 46 LE base). Modifiers: Monday discount -5 LE, medical skill and NPC trust reduce costs, opening-day schedules.

## Ending Conditions (EndingService)

8 endings checked in priority order:

**Bad:**
1. **MotherDied**: Mother health reaches 0
2. **Arrested**: Global heat >=100, or (Day>=30 AND Crimes>=6 AND Heat>=85 AND Stress>=70)
3. **Eviction**: 7 days of unpaid rent
4. **Destitution**: Health<=0, or (starving + exhausted + no money), or loan shark critical debt

**Good/Ambiguous:**
5. **StabilityHonestWork**: Day>=30, work earnings >=180, mother alive, heat<60, no recent crime
6. **NetworkShelter**: Day>=30, network trust sum >=140, mother alive, money >=120
7. **QuitTheLuxorDream**: Day>=30, money >500, crimes <=3, mother health >60
8. **CrimeKingpin**: Crime earnings >=1000 AND ImbabaCrew rep >50

Ending Ink knots have variants by background and strongest support contact.

## Sleep Quality

Rest recovery (base 30) and overnight recovery (base 15) modified by:
- High stress (-10/-5), no meal (-5/-3), undereating streak (-5/-3)
- Mother in crisis (-5/-3), rent anxiety (-3/-2)
- Home upgrades: CleanBedding +2, Fan +3/+1, WindowScreen +1, Curtain +1
- Season bonus (winter +3)
- Minimum: 10 rest, 5 overnight

## Diagnostics

Every significant game state change records a `GameMutationRecord` with RunId, timestamp, category (20 categories), action, before/after stats, and reason. Categories include Work, Crime, DayTransition, Travel, Food, Clinic, Entertainment, Investment, Rest, Shop, Training, Community, Economy, Phone, Information, etc. Application layer logs these via structured `ILogger` events.

## Narrative System

### Ink Integration (InkNarrativeService)
- Loads compiled `content/ink/main.json`
- Syncs game state variables to Ink: money, health, energy, hunger, stress, mother_health, food_stockpile, day, background
- Processes tags for outcomes: MONEY, HEALTH, ENERGY, HUNGER, STRESS, MOTHER_HEALTH, FOOD, FLAG, MESSAGE, NPC_TRUST, FACTION_REP
- Fails fast if compiled Ink story cannot be loaded; no fallback narrative runtime

### Narrative Outcome Tags
```
# MONEY: 20
# NPC_TRUST: NurseSalma, 5
# FLAG: crime_first_success
# MESSAGE: The street remembers.
```

### Scene Queue
GameSession maintains a pending narrative scenes queue. Scenes are dequeued by the UI layer and passed to InkNarrativeService.

## Save/Load

- JSON serialization via `JsonSaveGameStore`
- `GameSessionSnapshot` captures and restores full state including all subsystems
- `LoadedGameSession` wraps loaded metadata plus `GameSession` ownership across the persistence boundary
- Loaded via `LoadGameUseCase`, saved via `SaveGameUseCase`

## Content Boundaries

Per REQS.md:
- No graphic violence, child harm, explicit drug use depiction, torture/explicit police brutality
- Crime is consequence-heavy, not glamorized
- Honest work is exhausting and underpaid

## Content Data Files

| File | Purpose |
|---|---|
| `backgrounds.json` | 3 background definitions with starting stats |
| `district_conditions.json` | Per-district condition effects |
| `jobs.json` | 12 job shift definitions |
| `locations.json` | 13 location definitions with facilities |
| `plants.json` | 15 plant types |
| `pets.json` | Cat and fish definitions |
| `random_events.json` | 30 random event definitions |

## Key Files to Read

1. `REQS.md` - product requirements
2. `PLAN.MD` - current priorities and roadmap
3. `AGENTS.md` - coding standards and architecture rules
4. `src/Slums.Core/State/GameSession.cs` - canonical runtime boundary
5. `src/Slums.Core/Endings/EndingService.cs` - ending conditions
6. `src/Slums.Core/Jobs/JobService.cs` - work variants and mistakes
7. `src/Slums.Core/Crimes/CrimeService.cs` - crime resolution
8. `src/Slums.Core/Heat/DistrictHeatState.cs` - per-district police pressure
9. `src/Slums.Core/Territory/TerritoryState.cs` - faction territory control
10. `src/Slums.Core/Economy/PlayerDebtState.cs` - debt system
11. `src/Slums.Core/Phone/PhoneState.cs` - phone and messages
12. `src/Slums.Core/Information/TipState.cs` - information network

## Testing

- Framework: TUnit
- Assertions: FluentAssertions
- Mocking: NSubstitute
- 1,287 tests across 5 suites (969 Core + 121 Application + 8 Game + 31 Infrastructure + 158 Narrative.Ink)
- Validation workflow:
  1. `dotnet build ./Slums.slnx`
  2. `dotnet run --project tests/Slums.Core.Tests`
  3. `dotnet run --project tests/Slums.Application.Tests`
  4. `dotnet run --project tests/Slums.Game.Tests`
  5. `dotnet run --project tests/Slums.Infrastructure.Tests`
  6. `dotnet run --project tests/Slums.Narrative.Ink.Tests`

## Building and Running

```bash
# Compile Ink files (from src/Slums.Game)
npm run compile-ink

# Build solution
dotnet build ./Slums.slnx

# Run game
dotnet run --project src/Slums.Game
```
