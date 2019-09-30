using Concept.Caches;
using Concept.Controllers;
using Concept.Middlewares;
using Concept.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Theory;

namespace Concept
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationOptions _applicationOptions;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _applicationOptions = _configuration.Get<ApplicationOptions>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ClientsCache>();
            services.AddSingleton<Theoretical>();
            services.AddSingleton<WebSocketController>();
            services.Configure<ApplicationOptions>(_configuration);
            services.AddControllers();
            services.AddAuthentication()
                .UseHeaderAuthentication(options => options.Authorization = _applicationOptions.Authorization);

            if (_applicationOptions.CacheOptions.IsEnabled)
                services.AddSingleton<ResponsesCache>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware(typeof(ExceptionMiddleware));
            app.UseMiddleware(typeof(WebSocketMiddleware), _applicationOptions.Authorization);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseWebSockets();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}