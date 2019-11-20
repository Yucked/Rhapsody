using System;
using System.Threading;
using System.Threading.Tasks;
using Concept.Caches;
using Concept.Entities.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Concept.Jobs
{
    public sealed class PurgeJob : BaseJob
    {
        private readonly ResponsesCache _responsesCache;
        private readonly ApplicationOptions _applicationOptions;

        public PurgeJob(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<PurgeJob> logger)
            : base(logger)
        {
            _applicationOptions = configuration.Get<ApplicationOptions>();
            _responsesCache = serviceProvider.GetService<ResponsesCache>();
            Delay = TimeSpan.FromMilliseconds(_applicationOptions.CacheOptions.PurgeDelayMs);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            if (!_applicationOptions.CacheOptions.IsEnabled
                || _responsesCache == null)
            {
                Logger.LogError("Responses cache is disabled. Auto purge job won't be running.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _responsesCache.RemoveExpiredEntries(_responsesCache.YtCache);
                _responsesCache.RemoveExpiredEntries(_responsesCache.ScCache);
                _responsesCache.RemoveExpiredEntries(_responsesCache.BcCache);

                await Task.Delay(Delay)
                    .ConfigureAwait(false);
            }
        }
    }
}