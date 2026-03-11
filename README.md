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
dotnet test .\Slums.slnx
```

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
	Slums.Infrastructure.Tests/
	Slums.Narrative.Ink.Tests/
content/
	ink/
	data/
```

## Planning documents

- `PLAN.MD` tracks the current implementation shape and next execution priorities.
- `MAJOR-PLAN.md` is archived historical context for the completed expansion pass.
- `GAP-PLAN.MD` is archived historical context for an earlier pre-expansion gap list.
- `AGENTS.md` defines repository execution and architecture rules for future agents.
