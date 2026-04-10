# Review

This review focuses on **UI quality, usability, maintainability, and documentation alignment** across the current codebase.

## Findings

### 1. High — Modal screens suppress held keys inconsistently on return

Several modal screens return to `GameScreen` without calling `_parentScreen.SuppressActionKeysUntilRelease()`, while others do. That means held Enter/Escape input can leak back into the parent screen and immediately trigger another action after closing a modal.

**Why it matters:** this is a high-frequency usability bug. Accidental follow-up actions are especially dangerous in a survival game where a stray confirm can spend money, advance time, start work, or trigger crime.

**References:**
- `src/Slums.Game/Screens/ClinicTravelScreen.cs`
- `src/Slums.Game/Screens/CrimeScreen.cs:177-182`
- `src/Slums.Game/Screens/PhoneScreen.cs:287-292`
- `src/Slums.Game/Screens/TravelScreen.cs:166-171`
- `src/Slums.Game/Screens/WorkScreen.cs:286-290`

**Fix direction:** standardize every `ReturnToParentScreen()` path so modal screens always suppress held confirm/cancel keys before restoring focus to the parent screen.

### 2. High — Event log history is not actually accessible "at any time"

The requirements explicitly say the event log must stay accessible from the HUD and must capture automatic money in/out so the player can understand passive systems. The current UI only keeps the **last 6 lines** and renders them in a fixed window with no scrolling, paging, or full-history screen. In longer runs, rent, investment, phone, rumor, and plant-income messages will be pushed out almost immediately, which makes core simulation feedback easy to miss.

**Why it matters:** this is both a usability problem and a design mismatch. The repo is investing in deeper passive systems, but the main audit trail for those systems is too shallow to support player understanding.

**References:**
- `REQS.md:211-216`
- `src/Slums.Game/Screens/GameScreenLayout.cs:13-17`
- `src/Slums.Game/Screens/GameScreen.cs:519-525`
- `src/Slums.Game/Screens/GameScreen.cs:619-654`

**Fix direction:** keep a longer in-memory history and add a dedicated log viewer or paging/scroll support from the HUD so older entries remain reachable during play.

### 3. Medium — Important controls are hidden in the UI and missing from docs

The game relies on several non-obvious controls (`Tab`, `T`, `P`, `W`, `I`, `R`, `Escape`), but the main HUD only surfaces a `Tab=status pages` hint. The README explains how to run the game, but not how to navigate it. Several modal screens do print their own controls, but the overall control scheme is still fragmented and under-documented.

**Why it matters:** text-heavy games live or die on discoverability. Hidden controls create needless friction, especially when the project claims UI clarity as a completed priority.

**References:**
- `PLAN.MD:166-176`
- `src/Slums.Game/Screens/GameScreen.cs:177-181`
- `src/Slums.Game/Screens/GameScreen.cs:250-271`
- `src/Slums.Game/Screens/TravelScreen.cs:40-41`
- `src/Slums.Game/Screens/PhoneScreen.cs:98-99`
- `README.md:17-57`

**Fix direction:** add a persistent control legend on the main game screen and document the baseline control map in `README.md`.

### 4. Medium — Travel screen mouse behavior removes the walk option and commits immediately

The travel screen advertises two travel modes: transport and walking. Keyboard users can choose both, but mouse users cannot: clicking a destination immediately calls `TravelToSelected()`, which always uses transport. There is also no mouse-friendly preview/confirm step before committing the move.

**Why it matters:** the screen exposes one set of choices in its copy, then silently narrows those choices for mouse users. That is a UI inconsistency and a real gameplay limitation because walking is a meaningful economic tradeoff.

**References:**
- `src/Slums.Game/Screens/TravelScreen.cs:40-63`
- `src/Slums.Game/Screens/TravelScreen.cs:113-145`
- `src/Slums.Game/Screens/TravelScreen.cs:153-164`

**Fix direction:** let mouse input select first and confirm second, or expose clickable transport/walk actions for the selected destination.

### 5. Medium — Save flow is much weaker than load flow

`LoadGameScreen` shows checkpoint name and timestamp, but `SaveGameScreen` only shows raw slot IDs (`slot1`, `slot2`, `slot3`) and writes immediately on Enter. There is no overwrite warning, no last-played metadata, and no help deciding which slot is safe to reuse.

**Why it matters:** this is a classic usability footgun. The repo already has the metadata needed for a better save UI, but the save path does not reuse it.

**References:**
- `src/Slums.Game/Screens/SaveGameScreen.cs:12-16`
- `src/Slums.Game/Screens/SaveGameScreen.cs:34-45`
- `src/Slums.Game/Screens/SaveGameScreen.cs:62-67`
- `src/Slums.Game/Screens/LoadGameScreen.cs:39-47`

**Fix direction:** show the same slot metadata in the save screen and add an overwrite confirmation or explicit "empty/occupied" status.

### 6. Medium — `GameScreen` has become the UI maintenance hotspot

`GameScreen` now owns automatic time flow, HUD rendering, status paging, event-log state, keyboard routing, action dispatch, modal-screen navigation, and three nested window types. That concentration makes the file hard to reason about and raises the cost of even small UI changes.

**Why it matters:** this is the place most future UX work will land, and it is already carrying too many responsibilities. The risk is not just code size; it is regression-prone coupling between rendering, navigation, and gameplay action orchestration.

**References:**
- `src/Slums.Game/Screens/GameScreen.cs:18-40`
- `src/Slums.Game/Screens/GameScreen.cs:216-345`
- `src/Slums.Game/Screens/GameScreen.cs:347-517`
- `src/Slums.Game/Screens/GameScreen.cs:619-790`

**Fix direction:** extract input routing, HUD/event-log presentation, and screen-launch/navigation responsibilities into smaller collaborators so UI changes can be tested and evolved independently.

### 7. Medium — UI test coverage is too thin for the amount of screen logic

The game project contains many interactive screens, but the current `Slums.Game.Tests` coverage is limited to layout helpers and the key gate. There are no behavior-level tests around save/load navigation, phone interactions, travel mode selection, screen return flow, or HUD/event-log behavior.

**Why it matters:** the UI is now large enough that manual testing alone will not protect against regressions. Thin UI tests compound the maintainability risk from the large screen classes.

**References:**
- `tests/Slums.Game.Tests/Screens/GameScreenLayoutTests.cs:1-29`
- `tests/Slums.Game.Tests/Screens/TravelScreenLayoutTests.cs:1-35`
- `tests/Slums.Game.Tests/Input/ScreenActionKeyGateTests.cs:1-43`
- `src/Slums.Game/Screens/GameScreen.cs`
- `src/Slums.Game/Screens/TravelScreen.cs`
- `src/Slums.Game/Screens/PhoneScreen.cs`
- `src/Slums.Game/Screens/SaveGameScreen.cs`
- `src/Slums.Game/Screens/LoadGameScreen.cs`

**Fix direction:** add focused UI-shell tests around screen input contracts and navigation outcomes, starting with travel, phone, save/load, and event-log behavior.

## Documentation gaps

1. `README.md` explains build/run flow but does not explain the actual control scheme, despite the game depending on many keyboard shortcuts.
2. `PLAN.MD` says UI clarity is implemented, but the current HUD still hides key commands and does not provide full event-log access.
3. The save/load documentation does not prepare players or contributors for the slot model, overwrite behavior, or the difference between save and load screens.
