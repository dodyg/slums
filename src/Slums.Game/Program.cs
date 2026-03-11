using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Slums.Application.Content;
using Slums.Application.Narrative;
using Slums.Application.Persistence;
using Slums.Application.Randomness;
using Slums.Game;
using Slums.Infrastructure.Content;
using Slums.Infrastructure.Persistence;
using Slums.Infrastructure.Randomness;
using Slums.Narrative.Ink;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<INarrativeService, InkNarrativeService>();
        services.AddSingleton<IContentRepository, JsonContentRepository>();
        services.AddSingleton<ISaveGameStore, JsonSaveGameStore>();
        services.AddSingleton<SaveGameUseCase>();
        services.AddSingleton<LoadGameUseCase>();
        services.AddSingleton<IRandomSource, SeededRandomSource>();
        services.AddSingleton<IGame, SadConsoleGame>();
    })
    .Build();

var game = host.Services.GetRequiredService<IGame>();
game.Run();
