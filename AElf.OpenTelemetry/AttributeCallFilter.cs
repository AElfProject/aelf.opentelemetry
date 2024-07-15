using AElf.OpenTelemetry.ExecutionTime;
using Microsoft.Extensions.DependencyInjection;
using Orleans;

namespace AElf.OpenTelemetry;

public class AttributeCallFilter : IIncomingGrainCallFilter
{
    public async Task Invoke(IIncomingGrainCallContext context)
    {
        if (context.Grain.GetType().IsDefined(typeof(AggregateExecutionTimeAttribute), true))
        {
            var executionTimeRecorder = context.TargetContext.ActivationServices.GetService<ExecutionTimeRecorder>();

            if (executionTimeRecorder == null)
            {
                throw new Exception("ExecutionTimeRecorder is not registered.");
            }

            await executionTimeRecorder.InterceptAsync(context.Grain.GetType().Name, context.InterfaceMethod.Name, async () =>
            {
                await context.Invoke();
            });
        }
        else
        {
            await context.Invoke();
        }
    }
}