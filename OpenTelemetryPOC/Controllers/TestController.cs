using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;

namespace OpenTelemetryPOC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly Tracer _tracer;

        public TestController(
            ILogger<TestController> logger,
            TracerFactory traceFactory)
        {
            _logger = logger;
            _tracer = traceFactory.GetTracer("Testing123");
        }

        [HttpGet]
        public async Task<int> Get()
        {
            int delay = Environment.TickCount % 3000;
            _tracer.CurrentSpan.SetAttribute("DB", "Write");
            using (_tracer.StartActiveSpan("Demo", out TelemetrySpan span))
            {
                span.AddEvent("Event 1");
                span.SetAttribute("Dev", true);
                using (_tracer.StartActiveSpan("Sub", span, out TelemetrySpan span1))
                {
                    await Task.Delay(delay);
                    using (_tracer.StartActiveSpan("Sub-Sub", span1, out TelemetrySpan span2))
                    {
                        span2.SetAttribute("Final", true);
                        await Task.Delay(100);
                    }
                }
            }
            return delay;
        }
    }

    public class TraceFactory
    {
    }
}
