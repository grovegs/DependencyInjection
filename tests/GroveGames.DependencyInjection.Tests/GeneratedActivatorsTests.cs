using GroveGames.DependencyInjection.Generated;

namespace GroveGames.DependencyInjection.Tests;

// Test doubles here are internal rather than private: the source generator skips private types,
// and these tests exercise the generated path. Private doubles in the other test files cover reflection.
public sealed class GeneratedActivatorsTests
{
    [Fact]
    public void TryGetConstructor_RegisteredInternalType_ReturnsGeneratedConstructor()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestDependency>();

        var found = GeneratedActivators.TryGetConstructor(typeof(TestDependency), out var constructor);

        Assert.True(found);
        Assert.Empty(constructor!.ParameterTypes);
    }

    [Fact]
    public void TryGetConstructor_PrivateType_ReturnsFalse()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestPrivateService>();

        var found = GeneratedActivators.TryGetConstructor(typeof(TestPrivateService), out _);

        Assert.False(found);
    }

    [Fact]
    public void Resolve_GeneratedConstructor_InjectsDependencies()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestDependency>();
        builder.AddTransient<TestConsumer>();
        using var container = builder.Build();

        var consumer = container.Resolve<TestConsumer>();

        Assert.Same(container.Resolve<TestDependency>(), consumer.Dependency);
        Assert.True(GeneratedActivators.TryGetConstructor(typeof(TestConsumer), out _));
    }

    [Fact]
    public void Resolve_GeneratedConstructorThrows_ThrowsOriginalException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestThrowingService>();
        using var container = builder.Build();

        var exception = Record.Exception(() => container.Resolve<TestThrowingService>());

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Build_GeneratedCircularDependency_ThrowsCircularDependencyException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestCycleA>();
        builder.AddSingleton<TestCycleB>();

        var exception = Record.Exception(() => builder.Build());

        Assert.IsType<CircularDependencyException>(exception);
    }

    [Fact]
    public void Build_GeneratedMissingDependency_ThrowsRegistrationNotFoundException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestConsumer>();

        var exception = Record.Exception(() => builder.Build());

        Assert.IsType<RegistrationNotFoundException>(exception);
    }

    [Fact]
    public void Inject_GeneratedInjectMethod_CallsMethod()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestDependency>();
        using var container = builder.Build();
        var injectable = new TestInjectable();

        container.Inject(injectable);

        Assert.Same(container.Resolve<TestDependency>(), injectable.Dependency);
        Assert.True(GeneratedActivators.TryGetInjectMethods(typeof(TestInjectable), out var methods));
        Assert.Single(methods!);
    }

    internal sealed class TestDependency
    {
    }

    internal sealed class TestConsumer
    {
        private readonly TestDependency _dependency;

        public TestDependency Dependency => _dependency;

        public TestConsumer(TestDependency dependency)
        {
            _dependency = dependency;
        }
    }

    internal sealed class TestThrowingService
    {
        public TestThrowingService()
        {
            throw new InvalidOperationException();
        }
    }

    internal sealed class TestCycleA
    {
        public TestCycleA(TestCycleB dependency)
        {
        }
    }

    internal sealed class TestCycleB
    {
        public TestCycleB(TestCycleA dependency)
        {
        }
    }

    internal sealed class TestInjectable
    {
        private TestDependency? _dependency;

        public TestDependency? Dependency => _dependency;

        [Inject]
        public void Construct(TestDependency dependency)
        {
            _dependency = dependency;
        }
    }

    private sealed class TestPrivateService
    {
    }
}
