using OpenTelemetry.Trace;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace OpenTelemetryPOC
{
    public class HealthRejectsSampler : Sampler
    {
        private Sampler _sampler;
        public HealthRejectsSampler(Sampler chainedSampler)
        {
            _sampler = chainedSampler;
        }

        public override string Description => nameof(HealthRejectsSampler);

        public override Decision ShouldSample(
                                in SpanContext parentContext,
                                in ActivityTraceId traceId, 
                                in ActivitySpanId spanId, 
                                string name, 
                                SpanKind spanKind,
                                IDictionary<string, object> attributes,
                                IEnumerable<Link> links)
        {
            if (name == "/ health")
            {
                return new Decision(false);
            }
            Decision decision = _sampler.ShouldSample(
                               parentContext, traceId, spanId, name, spanKind, attributes, links);
            return decision;
        }
    }
}
