
using System.Linq;

using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

using static Microsoft.AspNetCore.Blazor.Hosting.BlazorWebAssemblyHost;

namespace Splendor.Blazor
{
    public class Program
    {
        public static void Main() => CreateDefaultBuilder()
                                    .UseBlazorStartup<Startup>()
                                    .Build()
                                    .Run();

        public sealed class Startup
        {
            // ReSharper disable once UnusedMember.Global
            public void Configure(IComponentsApplicationBuilder app) =>
                app.AddComponent<App>("app");

            public void ConfigureServices(IServiceCollection services)
            {
                var gameInitialiser = new DefaultGameInitialiser
                    (new DefaultCards());

                var playerNames = new [] { "James", "Robin", "Mat" };

                //services.AddTransient(_ => gameInitialiser.Create(playerNames));

                services.AddTransient(_ =>
                {
                    var gameState = gameInitialiser.Create(playerNames);

                    var purse = new Pool(1, 2, 2, 2, 2, 2);

                    var players = gameState.Players.Select
                        (p => p.Clone(withPurse: purse));

                    return gameState.Clone(withPlayers: players.ToArray());
                });
            }
        }
    }
}