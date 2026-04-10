# slums

Slums is a text-driven RPG about survival in Cairo, built with SadConsole, Ink, and .NET 10.

The current project includes:

- a playable SadConsole game loop
- multiple districts across Imbaba, Dokki, and Ard al-Liwa
- honest work tracks with progression, reliability, and lockouts
- contact-specific crime routes and relationship-driven aftermath
- NPC conversations with trust and memory-based variation
- district-specific random events
- background-specific advantages and vulnerabilities
- save/load support
- multiple ending paths shaped by behavior over time

## Running the game

From the repo root:

```powershell
dotnet run --project .\src\Slums.Game\Slums.Game.csproj
```

On Windows ARM64, `Slums.Game` defaults to `win-x64` so SadConsole and CSFML load the supported native binaries instead of the ARM64 host.

## Build and test

From the repo root:

```powershell
dotnet build .\Slums.slnx
dotnet run --project .\tests\Slums.Core.Tests
dotnet run --project .\tests\Slums.Application.Tests
dotnet run --project .\tests\Slums.Game.Tests
dotnet run --project .\tests\Slums.Infrastructure.Tests
dotnet run --project .\tests\Slums.Narrative.Ink.Tests
```

## Controls

### Main game screen

| Key | Action |
|-----|--------|
| Up/Down | Select action |
| Enter | Confirm action |
| Tab | Cycle status pages |
| T | Open travel menu |
| P | Save game |
| L | Open full event log |
| Esc | Return to main menu |

### Modal screens (work, crime, shop, etc.)

| Key | Action |
|-----|--------|
| Up/Down | Select item |
| Enter | Confirm selection |
| W | Walk (travel screen only) |
| R | Refill credit / replace phone (phone screen only) |
| I | Ignore message (phone screen only) |
| Esc | Cancel / go back |

### Narrative scenes

| Key | Action |
|-----|--------|
| Up/Down | Scroll text / select choice |
| 1-9 | Select choice directly |
| Enter | Confirm choice / continue |

### Event log viewer

| Key | Action |
|-----|--------|
| Up/Down | Scroll one line |
| PgUp/PgDn | Scroll one page |
| Home/End | Jump to start/end |
| Esc | Close viewer |

## Architecture notes

- `GameSession` is the canonical runtime boundary and is backed internally by EntitiesDb.
- Player-facing screen actions flow through `Slums.Application` commands/queries rather than mutating `GameSession` directly from SadConsole screens.
- `GameSession` keeps state ownership but delegates orchestration-heavy narrative/work/crime/investment logic to focused core helpers.
- Save/load works through `GameSession` snapshots and `LoadedGameSession`.
- Repo-owned JSON content is fail-fast at bootstrap rather than silently falling back.
- Ink loading is intentionally fail-fast; missing or invalid story content is treated as an error.

## Ink workflow

From `src/Slums.Game`, regenerate the compiled story with:

```powershell
npm run compile-ink
```

This updates `content/ink/main.json` from the authored `.ink` files in `content/ink/`.

## Project layout

```text
src/
	Slums.Game/
	Slums.Core/
	Slums.Application/
	Slums.Infrastructure/
	Slums.Narrative.Ink/
tests/
	Slums.Core.Tests/
	Slums.Application.Tests/
	Slums.Game.Tests/
	Slums.Infrastructure.Tests/
	Slums.Narrative.Ink.Tests/
content/
	ink/
	data/
```

## Planning documents

- `PLAN.MD` tracks the current implementation shape and next execution priorities.
- `MEMORY.MD` captures the current architecture and simulation notes for future sessions.
- `AGENTS.md` defines repository execution and architecture rules for future agents.
