using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTelemetryPOC
{
    /// <summary>
    /// Base class for hosted services (workers) which 
    /// help with best practice implementation of the host.
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.IHostedService" />
    public class HostedService : IHostedService
    {
        protected readonly ILogger _logger;
        private readonly Tracer _tracer;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedService"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public HostedService(
            ILogger<HostedService> logger,
            TracerFactory tracer)
        {
            _logger = logger;
            _tracer = tracer.GetTracer("X");
        }

        #endregion // Ctor

        #region StartAsync

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns></returns>
        async Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            int i = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                i++;
                _logger.LogWarning("Background processing {index}", i);

                using (_tracer.StartActiveSpan("LongRunningOperation", SpanKind.Server, out TelemetrySpan parentSpan))
                {
                    var attributes = new Dictionary<string, object>();
                    attributes.Add("use", "demo");
                    attributes.Add("iteration", i);
                    parentSpan.AddEvent(new Event($"Invoking DoWork {i}", attributes));
                    
                    await Task.Delay(i * 1000);

                    using (_tracer.StartActiveSpan("Sub",
                                                parentSpan,
                                                SpanKind.Producer,
                                                out TelemetrySpan childSpan))
                    {
                        _logger.LogWarning("Processing {index}", i);
                        childSpan.Status = Status.Ok.WithDescription("In-Process");
                    }

                    // Annotate our span to capture metadata about our operation

                    parentSpan.Status = Status.Ok.WithDescription("COMPLETE");
                }
            }
        }

        #endregion // StartAsync

        #region StopAsync

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns></returns>
        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion // StopAsync
    }
}
