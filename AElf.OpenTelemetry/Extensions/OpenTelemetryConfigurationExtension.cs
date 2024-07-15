using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace AElf.OpenTelemetry.Extensions;

public static class OpenTelemetryConfigurationExtension
{
    public const string DefaultCollectorEndpoint = "http://localhost:4317";

    public static IServiceCollection AddOpenTelemetry(this IServiceCollection services, string serviceName, string serviceVersion, string endpoint)
    {
        return services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: serviceName,
                serviceVersion: serviceVersion))
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddOtlpExporter(exporter => exporter.Endpoint = new Uri(endpoint))
                .AddSource("Microsoft.Orleans.Runtime")
                .AddSource("Microsoft.Orleans.Application")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(exporter => exporter.Endpoint = new Uri(endpoint))
                .AddMeter("Microsoft.Orleans")
                .AddMeter(serviceName)).Services;
    }

    public static ILoggingBuilder AddOpenTelemetry(this ILoggingBuilder builder, string serviceName, string serviceVersion, string endpoint)
    {
        return builder.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
            options.ParseStateValues = true;
            options
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion))
                .AddOtlpExporter(exporter => exporter.Endpoint = new Uri(endpoint));
        });
    }
}