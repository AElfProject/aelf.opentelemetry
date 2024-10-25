using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Volo.Abp.DependencyInjection;

namespace AElf.OpenTelemetry.ExecutionTime;

public class ExecutionTimeRecorder : ISingletonDependency, IInterceptor
{
    private readonly Meter _meter;
    private readonly ConcurrentDictionary<string, Histogram<long>> _histogramMapCache = new ConcurrentDictionary<string, Histogram<long>>();

    public ExecutionTimeRecorder(IInstrumentationProvider instrumentationProvider)
    {
        _meter = instrumentationProvider.Meter;
    }

    public async Task InterceptAsync(string className, string methodName, Func<Task> invocation)
    {
        var histogram = GetHistogram(className, methodName);

        var stopwatch = Stopwatch.StartNew();

        await invocation();

        stopwatch.Stop();

        histogram.Record(stopwatch.ElapsedMilliseconds);
    }
    
    private Histogram<long> GetHistogram(string className, string methodName)
    {
        var key = $"{className}.{methodName}.execution.time";

        if (_histogramMapCache.TryGetValue(key, out var rtKeyCache))
        {
            return rtKeyCache;
        }
        
        var histogram = _meter.CreateHistogram<long>(
            name: key,
            description: "Histogram for method execution time",
            unit: "ms"
        );
        _histogramMapCache.TryAdd(key, histogram);
        return histogram;
    }
}