
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Splendor.Blazor
{
    public class Program
    {
        public static void Main()
        {
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                                 .UseBlazorStartup<Startup>()
                                 .Build()
                                 .Run();
        }

        public sealed class Startup
        {
            public void Configure(IComponentsApplicationBuilder app)
            {
                app.AddComponent<App>("app");
            }

            public void ConfigureServices(IServiceCollection services) {}
        }
    }
}