
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.Extensions.DependencyInjection;

using Splendor.AIs.Unofficial;
using Splendor.Core;
using Splendor.Core.AI;

namespace Splendor.Blazor
{
    public sealed class Program
    {
        public static void Main()
        {
            var builder = WebAssemblyHostBuilder.CreateDefault();

            builder.RootComponents.Add<App>("app");

            builder.Services.AddTransient(_ => CreateGame());

            builder.Services.AddTransient(_ => new ISpendorAi[]
            {
                new StupidSplendorAi(string.Empty),
                null,
                new ObservantStupidSplendorAi(string.Empty),
                new BogoSpendorAi()
            });

            builder.Build().RunAsync();
        }

        private static GameState CreateGame()
        {
            var gameInitialiser = new DefaultGameInitialiser
                (new DefaultCards());

            var playerNames = new [] { nameof(StupidSplendorAi),
                                       "Robin",
                                       nameof(ObservantStupidSplendorAi),
                                       nameof(BogoSpendorAi) };

            return gameInitialiser.Create(playerNames);
        }
    }
}