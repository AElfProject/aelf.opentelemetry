namespace AElf.OpenTelemetry.ExecutionTime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AggregateExecutionTimeAttribute : Attribute
{
    public AggregateExecutionTimeAttribute()
    {
    }
}