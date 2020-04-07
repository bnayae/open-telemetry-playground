using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter.Jaeger;
using OpenTelemetry.Exporter.Zipkin;
using OpenTelemetry.Trace.Samplers;
using OpenTelemetry.Trace.Configuration;
using OpenTelemetry.Collector.Dependencies;
using OpenTelemetry.Resources;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;


namespace OpenTelemetryPOC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IHealthChecksBuilder healthBuilder = services.AddHealthChecks();
            healthBuilder.AddCheck<HealthCheckHandler>("health", tags: new[] { "health" });

            services.AddOpenTelemetry(builder =>
            {
                var t = builder
                    .SetSampler(new HealthRejectsSampler(new AlwaysSampleSampler()))
                    .UseJaeger(config =>
                        {
                            // config.AgentPort = 6831;
                            config.ServiceName = "Testing123";

                            config.ProcessTags = new Dictionary<string, object>
                            {
                                ["Tenant"] = "Super Organization"
                            };
                        }, processorBuilder =>
                        {
                            //processorBuilder.
                        })
                    .UseZipkin(options =>
                    {
                        // options.Endpoint = new Uri("http://localhost/api/v2/spans");
                        options.ServiceName = "Testing456";
                        options.TimeoutSeconds = TimeSpan.FromSeconds(30);
                    }, processorBuilder =>
                    {
                    })
                    // 9411
                    // you may also configure request and dependencies collectors
                    .AddRequestCollector()
                    .AddDependencyCollector();
            });


            //builder => builder      
            //    .UseZipkin(o => o.Endpoint = new Uri("Http://zipkin/api/v2/spans"))
            //          .AddRequestCollector()
            //          .AddDependencyCollector();
            //    });

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    AllowCachingResponses = false,
                    Predicate = (check) => check.Tags.Contains("health"), // filter the health check for this end-point
                                                                          //ResponseWriter = HealthCheckHelpers.WriteResponseAsync,
                });
                endpoints.MapControllers();
            });
        }
    }
}
