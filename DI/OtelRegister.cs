namespace LongRunningActor.DI;

public static class OtelRegister
{
    public static IServiceCollection RegisterOtel(this IServiceCollection services, string serviceName)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName: serviceName,
                    serviceVersion: Environment.GetEnvironmentVariable("OTEL_SERVICE_VERSION") ?? "0.1.0",
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(
                    [
                        new KeyValuePair<string, object>("deployment.environment", Environment.GetEnvironmentVariable("DEPLOY_ENV") ?? "dev")
                    ])
            )
            .UseOtlpExporter()
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation(o => o.RecordException = true)
                .AddAspNetCoreInstrumentation(o =>
                {
                    // 可选：把 /metrics 过滤掉，避免列表里全是它
                    o.Filter = ctx => ctx.Request.Path != "/metrics";
                    o.RecordException = true;
                })
                //.AddRuntimeInstrumentation()
                .AddSource(
                    AppActivitySources.ApiActivitySource.Name,
                    AppActivitySources.ActorActivitySource.Name) // 自定义 ActivitySource
                .SetSampler(new TraceIdRatioBasedSampler(1.0)) // 或 AlwaysOnSampler
                                                               //.AddOtlpExporter(configure => configure.Endpoint = new Uri("http://localhost:4317"))
            )
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(AppMetrics.Meter.Name)
            //.AddOtlpExporter(configure => configure.Endpoint = new Uri("http://localhost:4317"))
            );

        services.AddLogging(lb =>
        {
            lb.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.IncludeFormattedMessage = true;
                o.ParseStateValues = true;
                //o.AddOtlpExporter(configure => configure.Endpoint = new Uri("http://localhost:4317"));
            });
        });

        return services;
    }
}
