namespace AElf.OpenTelemetry;

public interface IInterceptor
{
    Task InterceptAsync(string className, string methodName, Func<Task> invocation);
}