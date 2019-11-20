using System;
using Concept.Caches;
using Concept.Controllers;
using Concept.Entities.Options;
using Concept.Jobs;
using Concept.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Theory;

namespace Concept
{
    public sealed class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationOptions _options;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _options = _configuration.Get<ApplicationOptions>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        public void Configure(IApplicationBuilder app, IServiceProvider provider)
        {
            app.UseWebSockets();
            app.UseRouting();

            app.UseMiddleware(typeof(ExceptionMiddleware));
            app.UseMiddleware(typeof(WebSocketMiddleware), _options.Authorization);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}