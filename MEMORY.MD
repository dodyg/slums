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
  Slums.Infrastructure.Tests/
  Slums.Narrative.Ink.Tests/
content/
  ink/                  # .ink source files, compiled to main.json
  data/                 # JSON: backgrounds.json, jobs.json, locations.json, random_events.json
```

**Dependency Direction**: Always inward. `Slums.Core` references nothing. UI never contains business logic.

## Core Simulation Loop

### GameState (src/Slums.Core/State/GameState.cs)

The single source of truth for all game state. Contains:

- **Clock**: Day/hour/minute tracking, time-of-day enum
- **Player**: Character stats, skills, nutrition, household
- **World**: Current location and district
- **Relationships**: NPC trust levels, faction reputations
- **JobProgress**: Reliability scores per job type, lockouts
- **Counters**: Crime/honest work earnings, days survived, police pressure
- **StoryFlags**: Set of strings for narrative state
- **EventHistory**: Random event occurrence counts

### Time Flow

1. **AdvanceTime(minutes)**: Advances clock, triggers EndDay at 22:00
2. **EndDay()**: 
   - Applies daily stat decay
   - Resolves nutrition consequences
   - Charges daily rent (20 LE)
   - Handles mother care
   - Decays police pressure (faster for released prisoner)
   - Rolls random events
   - Queues narrative follow-up scenes
   - Checks ending conditions

### Player Character

**SurvivalStats** (0-100 scale):
- Money (can go to 0, tracks earnings)
- Hunger (mapped from NutritionState.Satiety)
- Energy (restored by sleep, consumed by work/travel)
- Health (decays from starvation, injury, events)
- Stress (increases from pressure, crime, poverty)

**NutritionState**:
- Satiety: 0-100, decays 18/day
- DaysUndereating: tracks consecutive poor meals
- MealQuality: None/Scraps/Basic/HotMeal
- Skipping meals drains energy, increases stress, eventually damages health

**HouseholdCareState**:
- MotherHealth: 0-100, death at 0 triggers bad ending
- MotherCondition: Stable(65+)/Fragile(30-64)/Crisis(0-29)
- FoodStockpile, MedicineStock
- Daily flags: FedMotherToday, MedicationGivenToday

**SkillState** (0-10 scale):
- Medical: Unlocks clinic variants, reduces medicine cost
- Persuasion: Improves trust gains, unlocks call center/dispatch variants
- StreetSmarts: Lowers crime detection, unlocks crime variants
- Physical: Reduces energy cost for manual labor jobs

## Three Playable Backgrounds

Defined in `content/data/backgrounds.json`:

1. **MedicalSchoolDropout**: High stress, Medical 3, better clinic outcomes
2. **ReleasedPoliticalPrisoner**: Low money, higher police scrutiny, Persuasion 2 + StreetSmarts 1
3. **SudaneseRefugee**: Accent friction in Dokki, StreetSmarts 2 + Physical 1, community support

Each background has unique:
- Starting stats and resources
- Skill bonuses
- Ink intro knot
- Narrative reactivity (clinic scenes, police heat, solidarity events)

## Locations and Districts

**Districts** (DistrictId):
- Imbaba: Home district, market, bakery
- Dokki: Call center, square, cafe - more expensive, refugee stress
- ArdAlLiwa: Clinic, workshop
- BulaqAlDakrour: Pharmacy, microbus depot
- Shubra: Laundry

**District Effects**:
- Food costs vary (13-20 LE)
- Street food costs vary (7-10 LE)
- Medicine costs vary (42-58 LE)
- Refugee background adds stress in Dokki
- NPC relationships can reduce costs

## Honest Work System

### Job Types (JobType enum)
- BakeryWork, HouseCleaning (Imbaba)
- CallCenterWork, CafeService (Dokki)
- ClinicReception, WorkshopSewing (ArdAlLiwa)
- PharmacyStock, MicrobusDispatch (BulaqAlDakrour)
- LaundryPressing (Shubra)

### Job Variants
Each job has multiple variants unlocked by:
- **Reliability**: 55/60/70/75/80 thresholds
- **NPC Trust**: 10/20 thresholds with employer
- **Skills**: Physical 2/3, Persuasion 2, Medical 2/3
- **Background**: Medical dropout gets clinic advantages

Variants modify: pay (+/-), energy cost, stress cost, duration.

### Mistake System
Jobs can trigger mistakes based on:
- High stress (60-65+ thresholds)
- Low energy (near minimum required)

Mistakes cause:
- Reduced pay (half)
- Extra stress
- Reliability loss (-10 to -15)
- Temporary lockout (1-2 days)
- Employer trust loss (-4)

### Job Progress Tracking (JobProgressState)
- Per-job reliability score
- Shifts completed count
- Lockout expiration day

## Crime System

### Crime Types (CrimeType enum)
- **PettyTheft, Robbery, HashishTrade**: Basic crimes at market/square
- **MarketFencing**: Requires Fence Hanan access
- **DokkiDrop**: Requires Call Center/Cafe reliability 60+
- **NetworkErrand**: Requires released prisoner background + ex-prisoner network rep
- **DepotFareSkim**: Requires Microbus Dispatch reliability 60+
- **ShubraBundleLift**: Requires Laundry reliability 60+

### Crime Resolution (CrimeService)
1. Preview calculates detection/success chances
2. StreetSmarts 3 reduces detection by 10%
3. Police pressure increases detection risk
4. Success rolls: money earned + variable bonus
5. Detection rolls: if detected, police pressure increases more

### Crime Modifiers
- Same-day public work: thin alibi, reduces risk
- Released prisoner background: increased scrutiny
- Police pressure 60+: materially increases risk

### Police Pressure (0-100)
- Increases on detected crimes
- Decays daily (5 normally, 2 for released prisoner)
- At 100: Arrested ending
- At 80+: Warning scene triggers
- Affects detection chance, NPC availability, ending eligibility

## Relationships and Factions

### NPCs (NpcId enum)
12 recurring NPCs:
- LandlordHajjMahmoud, NeighborMona (home building)
- FixerUmmKarim (crime contact)
- OfficerKhalid (police)
- NurseSalma, WorkshopBossAbuSamir (employers)
- CafeOwnerNadia, PharmacistMariam (employers)
- FenceHanan, RunnerYoussef (crime contacts)
- DispatcherSafaa, LaundryOwnerIman (employers/contacts)

### NpcRelationship Tracks
- Trust: -100 to 100
- SeenConversationKnots: prevents dialogue repetition
- WasHelped, HasUnpaidDebt, WasEmbarrassed: relationship memory flags
- LastRefusalDay, RecentContactCount: timing-based reactivity
- FactionReputation: ImbabaCrew, DokkiThugs, ExPrisonerNetwork

### Conversation Pool System
- Each NPC has 100+ unique dialogue knots per context
- Context determined by: trust level, police pressure, money, mother health, double-life status, recent events
- Example contexts: "broke_soft", "hot", "double_life", "debt_warm"
- Seen knots are excluded, ensuring 100 days without repetition

## Random Events

Defined in `RandomEventRegistry`, filtered by:
- MinDay: event cannot fire before day N
- Condition(GameState): custom eligibility check

Each event has:
- Weight: selection probability
- Effect: stat changes, money, police pressure, mother health
- Optional InkKnot: narrative scene trigger

## Narrative System

### Ink Integration (InkNarrativeService)
- Loads compiled `content/ink/main.json`
- Syncs game state variables to Ink: money, health, energy, hunger, stress, mother_health, food_stockpile, day, background
- Processes tags for outcomes: MONEY, HEALTH, ENERGY, HUNGER, STRESS, MOTHER_HEALTH, FOOD, FLAG, MESSAGE, NPC_TRUST, FACTION_REP

### Narrative Outcome Tags
In Ink files, use tags like:
```
# MONEY: 20
# NPC_TRUST: NurseSalma, 5
# FLAG: crime_first_success
# MESSAGE: The street remembers.
```

### Scene Queue
GameState maintains `_pendingNarrativeScenes` queue. Scenes are dequeued by the UI layer and passed to InkNarrativeService.

## Ending Conditions (EndingService)

9 endings checked in priority order:

1. **MotherDied**: Mother health reaches 0
2. **CollapseFromExhaustion**: Player health reaches 0
3. **Destitution**: Starving + Exhausted + No money
4. **Arrested**: Police pressure reaches 100
5. **BuriedByHeat**: 30+ days, 6+ crimes, pressure 85+, stress 70+
6. **LeavingCrime**: 30+ days, crimes committed, 5+ days clean, some honest earnings
7. **NetworkShelter**: 30+ days, high trust with support NPCs
8. **QuitTheLuxorDream**: 30+ days, 500+ money, 3 or fewer crimes, mother healthy
9. **StabilityHonestWork**: 30+ days, no crimes, 200+ money, low pressure
10. **CrimeKingpin**: 1000+ crime earnings, Imbaba crew rep 50+

Ending Ink knots have variants by background (e.g., `ending_stability_medical`) and strongest support contact.

## Save/Load

- JSON serialization via `JsonSaveGameStore`
- Full state preservation including story flags, event history, job progress
- Loaded via `LoadGameUseCase`, saved via `SaveGameUseCase`

## Content Boundaries

Per REQS.md:
- No graphic violence
- No child harm
- No explicit drug use depiction (hashish trade exists as implication/consequence only)
- No torture/explicit police brutality detail

Crime is consequence-heavy, not glamorized. Honest work is exhausting and underpaid.

## Key Files to Read

When making changes:
1. `REQS.md` - product requirements
2. `PLAN.MD` - current priorities and roadmap
3. `AGENTS.md` - coding standards and architecture rules
4. `src/Slums.Core/State/GameState.cs` - central state machine
5. `src/Slums.Core/Endings/EndingService.cs` - ending conditions
6. `src/Slums.Core/Jobs/JobService.cs` - work variants and mistakes
7. `src/Slums.Core/Crimes/CrimeService.cs` - crime resolution

## Testing

- Framework: TUnit
- Assertions: FluentAssertions
- Mocking: NSubstitute
- Run tests: `dotnet run --project tests/Slums.Core.Tests` (executable test projects)

## Building and Running

```bash
# Compile Ink files (from src/Slums.Game)
npm run compile-ink

# Build solution
dotnet build ./Slums.slnx

# Run game
dotnet run --project src/Slums.Game
```
