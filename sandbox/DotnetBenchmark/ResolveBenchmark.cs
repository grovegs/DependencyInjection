using BenchmarkDotNet.Attributes;

using GroveGames.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

[MemoryDiagnoser]
public class ResolveBenchmark
{
    private IContainer _container = null!;
    private ServiceProvider _serviceProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<ServiceA>();
        builder.AddSingleton<ServiceB>();
        builder.AddSingleton<ServiceC>();
        builder.AddTransient<Consumer>();
        builder.AddTransient<IFactoryProduct>(_ => new FactoryProduct());
        _container = builder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<ServiceA>();
        services.AddSingleton<ServiceB>();
        services.AddSingleton<ServiceC>();
        services.AddTransient<Consumer>();
        services.AddTransient<IFactoryProduct>(_ => new FactoryProduct());
        _serviceProvider = services.BuildServiceProvider();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _container.Dispose();
        _serviceProvider.Dispose();
    }

    [Benchmark]
    public object GroveGamesSingleton() => _container.Resolve(typeof(ServiceA));

    [Benchmark]
    public object MicrosoftSingleton() => _serviceProvider.GetRequiredService(typeof(ServiceA));

    [Benchmark]
    public object GroveGamesTransient() => _container.Resolve(typeof(Consumer));

    [Benchmark]
    public object MicrosoftTransient() => _serviceProvider.GetRequiredService(typeof(Consumer));

    [Benchmark]
    public object GroveGamesFactory() => _container.Resolve(typeof(IFactoryProduct));

    [Benchmark]
    public object MicrosoftFactory() => _serviceProvider.GetRequiredService(typeof(IFactoryProduct));

    [Benchmark]
    public IContainer GroveGamesBuild()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<ServiceA>();
        builder.AddSingleton<ServiceB>();
        builder.AddSingleton<ServiceC>();
        builder.AddTransient<Consumer>();
        return builder.Build();
    }

    [Benchmark]
    public ServiceProvider MicrosoftBuild()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ServiceA>();
        services.AddSingleton<ServiceB>();
        services.AddSingleton<ServiceC>();
        services.AddTransient<Consumer>();
        return services.BuildServiceProvider();
    }
}

public sealed class ServiceA
{
}

public sealed class ServiceB
{
}

public sealed class ServiceC
{
}

public sealed class Consumer
{
    public Consumer(ServiceA a, ServiceB b, ServiceC c)
    {
    }
}

public interface IFactoryProduct
{
}

public sealed class FactoryProduct : IFactoryProduct
{
}
