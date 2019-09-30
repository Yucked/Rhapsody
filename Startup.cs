using Concept.Controllers;
using Concept.Middlewares;
using Concept.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Theory;

namespace Concept
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly Settings _settings;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _settings = _configuration.Get<Settings>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ClientsCache>();
            services.AddSingleton<Theoretical>();
            services.AddSingleton<WebSocketController>();
            services.Configure<Settings>(_configuration);
            services.AddControllers();
            //You need the specify the default scheme in this case "HeaderAuth"
            services.AddAuthentication("HeaderAuth")
                .UseHeaderAuthentication(options => options.Authorization = _settings.Authorization);
        }

        public void Configure(IApplicationBuilder app)
        {
            //If you change the order authorize will not work
            app.UseWebSockets();
            app.UseRouting();

            app.UseMiddleware(typeof(ExceptionMiddleware));
            app.UseMiddleware(typeof(WebSocketMiddleware), _settings.Authorization);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}