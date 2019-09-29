using Concept.Controllers;
using Concept.Middlewares;
using Concept.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Concept
{
    public readonly struct Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ClientsCache>();
            services.AddSingleton<WebSocketController>();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseWebSockets();
            app.UseMiddleware(typeof(WebSocketMiddleware));
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}