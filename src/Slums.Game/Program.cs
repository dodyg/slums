using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slums.Game;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IGame, SadConsoleGame>();
    })
    .Build();

var game = host.Services.GetRequiredService<IGame>();
game.Run();
