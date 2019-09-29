using Concept.Authentication;
using Concept.Configuration;
using Concept.Controllers;
using Concept.Middlewares;
using Concept.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Config = Concept.Configuration.Configuration;

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
            var config = new ConfigurationLoader().GetConfiguration();

            services.AddSingleton(config);
            services.AddTransient<ClientsCache>();
            services.AddSingleton<WebSocketController>();
            services.AddControllers();
            services.AddAuthentication(HeaderDefaults.AuthenticationScheme)
                .AddHeaderAuth(options =>
                {
                    options.Authorization = config.Authorization;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Config config)
        {
            app.UseRouting();
            app.UseWebSockets();

            //Don't works in other middleware
            app.UseMiddleware(typeof(ExceptionMiddleware));
            app.UseMiddleware(typeof(WebSocketMiddleware), config.Authorization);

            //For any reason in asp.net core 3.0 we need the 2 uses.
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}