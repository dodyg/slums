# slums
This is a text driven RPG about survival in Cairo.

## Running the game

From the repo root, start the game with:

```powershell
dotnet run --project .\src\Slums.Game\Slums.Game.csproj
```

On Windows ARM64, `Slums.Game` defaults to `win-x64` so SadConsole and CSFML load the supported native binaries instead of the ARM64 host.

## Ink workflow

From `src/Slums.Game`, regenerate the compiled story with:

```powershell
npm run compile-ink
```

This updates `content/ink/main.json` from the authored `.ink` files in `content/ink/`.
