using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;

namespace AElf.OpenTelemetry.ExecutionTime;

[Dependency(ServiceLifetime.Singleton)]
public class ExecutionTimeInterceptor : AbpInterceptor
{
    private readonly IInterceptor _interceptor;

    public ExecutionTimeInterceptor(IInterceptor interceptor)
    {
        _interceptor = interceptor;
    }

    public override async Task InterceptAsync(IAbpMethodInvocation invocation)
    {
        await _interceptor.InterceptAsync(invocation.TargetObject.GetType().Name, invocation.Method.Name, async () =>
        {
            await invocation.ProceedAsync();
        });
    }
}
