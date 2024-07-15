using AElf.OpenTelemetry.ExecutionTime;
using AElf.OpenTelemetry.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace AElf.OpenTelemetry;

public class OpenTelemetryModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var config = context.Services.GetConfiguration();
        var serviceName = config["OpenTelemetry:ServiceName"];
        var serviceVersion = config["OpenTelemetry:ServiceVersion"];
        var endpoint = config["OpenTelemetry:CollectorEndpoint"] ?? OpenTelemetryConfigurationExtension.DefaultCollectorEndpoint;
        
        if(serviceName.IsNullOrWhiteSpace() || serviceVersion.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("ServiceName and ServiceVersion must be provided in the configuration file.");
        }
        
        context.Services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(serviceName, serviceVersion, endpoint);
        });
        
        context.Services.AddOpenTelemetry(serviceName, serviceVersion, endpoint)
                        .AddSingleton<IInstrumentationProvider>(_ => new InstrumentationProvider(serviceName, serviceVersion))
                        .AddSingleton<IInterceptor, ExecutionTimeRecorder>()
                        .AddSingleton<IIncomingGrainCallFilter, AttributeCallFilter>()
                        .OnRegistered(options =>
                        {
                            if (options.ImplementationType.IsDefined(typeof(AggregateExecutionTimeAttribute), true))
                            {
                                var result = options.Interceptors.TryAdd<ExecutionTimeInterceptor>();
                                if (!result)
                                {
                                    throw new AbpException("ExecutionTimeInterceptor is already registered.");
                                }
                            }
                        });
    }
}
