
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

using Splendor.Core;
using Splendor.Core.AI;

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

                var playerNames = new [] { nameof(StupidSplendorAi),
                                           "Robin",
                                           nameof(ObservantStupidSplendorAi) };

                services.AddTransient(_ => gameInitialiser.Create(playerNames));

                services.AddTransient(_ => new ISpendorAi[]
                {
                    new StupidSplendorAi(string.Empty),
                    null,
                    new ObservantStupidSplendorAi(string.Empty)
                });
            }
        }
    }
}