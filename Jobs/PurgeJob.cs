using System;
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
        /// <inheritdoc />
        protected override string Name { get; }

        private readonly ResponsesCache _responsesCache;
        private readonly ApplicationOptions _applicationOptions;

        public PurgeJob(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<PurgeJob> logger)
            : base(logger)
        {
            Name = "Purge";
            _applicationOptions = configuration.Get<ApplicationOptions>();
            _responsesCache = serviceProvider.GetService<ResponsesCache>();
        }

        /// <inheritdoc />
        protected override async Task InitializeAsync()
        {
            if (!_applicationOptions.CacheOptions.IsEnabled
                || _responsesCache == null)
            {
                Logger.LogError("Responses cache is disabled. Auto purge job won't be running.");
                return;
            }

            while (!TokenSource.IsCancellationRequested)
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