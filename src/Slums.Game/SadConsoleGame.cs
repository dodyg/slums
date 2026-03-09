using Microsoft.Extensions.Logging;
using SadConsole;
using SadConsole.Configuration;
using Slums.Game.Screens;

namespace Slums.Game;

[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via DI")]
internal sealed class SadConsoleGame : IGame
{
    private readonly ILogger<SadConsoleGame> _logger;

    public SadConsoleGame(ILogger<SadConsoleGame> logger)
    {
        _logger = logger;
    }

    public void Run()
    {
        Settings.WindowTitle = "Slums";
        Settings.AllowWindowResize = false;

        Builder gameConfig = new Builder()
            .SetWindowSizeInCells(80, 25)
            .SetStartingScreen(host => new MainMenuScreen(80, 25));

        global::SadConsole.Game.Create(gameConfig);
        global::SadConsole.Game.Instance.Run();
        global::SadConsole.Game.Instance.Dispose();
    }
}
