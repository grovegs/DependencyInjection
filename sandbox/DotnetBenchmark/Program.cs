using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GroveGames.DependencyInjection;

BenchmarkRunner.Run<ContainerBenchmarks>();

[MemoryDiagnoser]
public class ContainerBenchmarks
{
    private IRootContainer _rootContainer = null!;
    private IContainer _childContainer = null!;

    [GlobalSetup]
    public void Setup()
    {
        _rootContainer = RootContainerFactory.CreateRootContainer(builder =>
        {
            builder.AddSingleton<IService, ServiceImpl>();
            builder.AddTransient<ITransientService, TransientServiceImpl>();
        });

        _childContainer = ContainerFactory.CreateContainer("Child", _rootContainer, builder =>
        {
            builder.AddSingleton<IChildService, ChildServiceImpl>();
        });
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _childContainer.Dispose();
        _rootContainer.Dispose();
    }

    [Benchmark]
    public object ResolveSingleton()
    {
        return _rootContainer.Resolve<IService>();
    }

    [Benchmark]
    public object ResolveTransient()
    {
        return _rootContainer.Resolve<ITransientService>();
    }

    [Benchmark]
    public object ResolveFromChild()
    {
        return _childContainer.Resolve<IChildService>();
    }

    [Benchmark]
    public object ResolveFromParentViaChild()
    {
        return _childContainer.Resolve<IService>();
    }

    [Benchmark]
    public IRootContainer CreateAndDisposeContainer()
    {
        var container = RootContainerFactory.CreateRootContainer(builder =>
        {
            builder.AddSingleton<IService, ServiceImpl>();
        });
        container.Dispose();
        return container;
    }
}

public interface IService { }
public sealed class ServiceImpl : IService { }

public interface ITransientService { }
public sealed class TransientServiceImpl : ITransientService { }

public interface IChildService { }
public sealed class ChildServiceImpl : IChildService { }
