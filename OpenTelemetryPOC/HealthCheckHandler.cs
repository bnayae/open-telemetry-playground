using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

// ASP Logging: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-3.1

namespace OpenTelemetryPOC
{
    internal class HealthCheckHandler : IHealthCheck
    {
        private readonly ILogger _logger;

        public HealthCheckHandler(ILogger<HealthCheckHandler> logger)
        {
            _logger = logger;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            string content = $"Is Healthy: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}";
            _logger.LogDebug(content);

            var result = HealthCheckResult.Healthy(content);
            return Task.FromResult(result);
        }
    }
}
