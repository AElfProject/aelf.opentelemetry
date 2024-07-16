# AElf OpenTelemetry

An OpenTelemetry module for use in ABP and Orleans framework.

- [About The Project](#about-the-project)
- [Getting Started](#getting-started)
  - [Installation](#installation)
  - [Configuration](#configuration)
  - [Setup](#setup)
    - [Orleans](#orleans)
- [Examples](#examples)
  - [AggregateExecutionTime](#aggregateexecutiontime)
  - [Instrumentation](#instrumentation)
- [Contributing](#contributing)
- [License](#license)

## About The Project

This project is a module for ABP and Orleans framework to use OpenTelemetry. It provides a simple and fast way to integrate OpenTelemetry in ABP and Orleans framework.

The module will provide instrumentation for both ASP and Orleans out of the box:
1. [Orleans Observability](https://learn.microsoft.com/en-us/dotnet/orleans/host/monitoring/?pivots=orleans-7-0)
2. [AspNetCore Observability](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md)
3. [HttpClient Observability](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/blob/main/src/OpenTelemetry.Instrumentation.Http/README.md)

## Getting Started

### Installation

Run the following command in your project to install this module:

```sh
dotnet add package AElf.OpenTelemetry
```

### Configuration

To configure your opentelemetry service name, version and collector's endpoint, you need to add the following to your appsettings.json file:

```json
{
  "OpenTelemetry": {
    "ServiceName": "YourServiceName",
    "ServiceVersion": "YourServiceVersion",
    "CollectorEndpoint": "http://localhost:4317"
  }
}
```

### Setup

Add the following dependency to your project's Module class:

```cs
using AElf.OpenTelemetry;

[DependsOn(
    typeof(OpenTelemetryModule)
)]
public class MyTemplateModule : AbpModule
```

This will automatically register the OpenTelemetry module and setup your project for instrumentation.

#### Orleans

For Orleans, you need to add the following to your SiloBuilder:

```csharp
hostBuilder.UseOrleans((context, siloBuilder) =>
{
    siloBuilder.AddActivityPropagation();
});
```

Do the same for the Clientbuilder:

```csharp
hostBuilder.UseOrleansClient((context, clientBuilder) =>
{
    clientBuilder.AddActivityPropagation();
});
```

This will automatically propagate the respective activities of the Orleans grain calls.

## Examples

Here are some examples of how to use this module.

### AggregateExecutionTime

AggregateExecutionTime is an attribute that can be used to measure the execution time of a class's method. It can be used in both ABP and Orleans framework, either on the class or method level.

Class level:
```cs
using AElf.OpenTelemetry.ExecutionTime;

[AggregateExecutionTime]
public class MessageValidatorGrain : Grain
```

Method level:
```cs
[AggregateExecutionTime]
public Task<bool> IsOffensive(string message)
```

The attribute can also be used in ABP's services or controllers.
```csharp
[AggregateExecutionTime]
public class AuthorAppService : ApplicationService
```
This will automatically measure the execution time of the method of the class and send the data to the OpenTelemetry collector.

**Do note that for Controllers and Application Services in ABP, please make sure that the method you would like metrics for is a virtual method for the attribute to work.**

### Instrumentation

For custom instrumentation, you can use the `IInstrumentationProvider` interface to create custom spans and metrics. Get the instrumentation provider from the DI container and use it to create spans and metrics.

```csharp
public class MessageValidatorGrain : Grain, IMessageValidator
{
    private readonly ILogger _logger;
    private readonly ActivitySource _activitySource;
    private static readonly string[] OffensiveWords = new string[] {"offensive", "bad", "rude"};

    public MessageValidatorGrain(ILogger<MessageValidatorGrain> logger, IInstrumentationProvider instrumentationProvider)
    {
        _logger = logger;
        _activitySource = instrumentationProvider.ActivitySource;
    }

    public Task<bool> IsOffensive(string message)
    {
        // This will start a custom trace for the IsOffensive method
        using var myActivity = _activitySource.StartActivity($"{nameof(MessageValidatorGrain)}.IsOffensive");
        
        _logger.LogInformation("""
                               IsOffensive message received: message = "{Message}"
                               """,
            message);
        
        return Task.FromResult(OffensiveWords.Any(message.Contains));
    }
}
```

## Contributing

If you encounter a bug or have a feature request, please use the [Issue Tracker](https://github.com/AElfProject/aelf.opentelemetry/issues/new). The project is also open to contributions, so feel free to fork the project and open pull requests.

## License

Distributed under the MIT License. See [License](LICENSE) for more information.
