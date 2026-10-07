namespace GroveGames.DependencyInjection.Tests;

public sealed class ContainerBuilderTests
{
    [Fact]
    public void Build_SingletonType_ResolvesSameInstance()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestService>();
        using var container = builder.Build();

        var first = container.Resolve<TestService>();
        var second = container.Resolve<TestService>();

        Assert.Same(first, second);
    }

    [Fact]
    public void Build_TransientType_ResolvesNewInstances()
    {
        var builder = new ContainerBuilder();
        builder.AddTransient<TestService>();
        using var container = builder.Build();

        var first = container.Resolve<TestService>();
        var second = container.Resolve<TestService>();

        Assert.NotSame(first, second);
    }

    [Fact]
    public void Build_InstanceRegistration_ResolvesInstance()
    {
        var instance = new TestService();
        var builder = new ContainerBuilder();
        builder.AddSingleton(instance);
        using var container = builder.Build();

        var resolved = container.Resolve<TestService>();

        Assert.Same(instance, resolved);
    }

    [Fact]
    public void Build_FactoryRegistration_PassesResolver()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestService>();
        builder.AddSingleton(resolver => new TestConsumer(resolver.Resolve<TestService>()));
        using var container = builder.Build();

        var consumer = container.Resolve<TestConsumer>();

        Assert.Same(container.Resolve<TestService>(), consumer.Service);
    }

    [Fact]
    public void Build_FactoryReturnsWrongType_ThrowsOnResolve()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton(typeof(TestConsumer), _ => new TestService());
        using var container = builder.Build();

        var exception = Record.Exception(() => container.Resolve<TestConsumer>());

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Build_ImplementationOnly_BindsImplementationType()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestService>();
        using var container = builder.Build();

        var found = container.TryResolve<ITestService>(out _);

        Assert.True(container.TryResolve<TestService>(out _));
        Assert.False(found);
    }

    [Fact]
    public void Build_ServiceAndImplementation_BindsServiceTypeOnly()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<ITestService, TestService>();
        using var container = builder.Build();

        var found = container.TryResolve<TestService>(out _);

        Assert.True(container.TryResolve<ITestService>(out _));
        Assert.False(found);
    }

    [Fact]
    public void Build_AliasFactory_SharesSingleton()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestService>();
        builder.AddSingleton<ITestService>(resolver => resolver.Resolve<TestService>());
        using var container = builder.Build();

        var byInterface = container.Resolve<ITestService>();
        var byType = container.Resolve<TestService>();

        Assert.Same(byInterface, byType);
    }

    [Fact]
    public void Build_DuplicateServiceType_ThrowsInvalidOperationException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<ITestService, TestService>();
        builder.AddSingleton<ITestService>(_ => new TestService());

        var exception = Record.Exception(() => builder.Build());

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Build_MissingDependency_ThrowsRegistrationNotFoundException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestConsumer>();

        var exception = Record.Exception(() => builder.Build());

        var notFound = Assert.IsType<RegistrationNotFoundException>(exception);
        Assert.Equal(typeof(TestService), notFound.Type);
        Assert.Equal(typeof(TestConsumer), notFound.DependentType);
    }

    [Fact]
    public void Build_CircularDependency_ThrowsCircularDependencyException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestCycleA>();
        builder.AddSingleton<TestCycleB>();

        var exception = Record.Exception(() => builder.Build());

        Assert.IsType<CircularDependencyException>(exception);
    }

    [Fact]
    public void Build_CircularFactoryDependency_ThrowsCircularDependencyExceptionOnResolve()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton(resolver => new TestConsumer(resolver.Resolve<TestService>()));
        builder.AddSingleton(resolver =>
        {
            resolver.Resolve<TestConsumer>();
            return new TestService();
        });
        using var container = builder.Build();

        var exception = Record.Exception(() => container.Resolve<TestConsumer>());

        Assert.IsType<CircularDependencyException>(exception);
    }

    [Fact]
    public void Build_MultipleConstructors_UsesConstructorWithMostParameters()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestService>();
        builder.AddSingleton<TestMultipleConstructors>();
        using var container = builder.Build();

        var instance = container.Resolve<TestMultipleConstructors>();

        Assert.NotNull(instance.Service);
    }

    [Fact]
    public void Build_ConstructorWithInjectAttribute_UsesMarkedConstructor()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestService>();
        builder.AddSingleton<TestMarkedConstructor>();
        using var container = builder.Build();

        var instance = container.Resolve<TestMarkedConstructor>();

        Assert.True(instance.UsedMarkedConstructor);
    }

    [Fact]
    public void Build_ConstructorThrows_RethrowsOriginalException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestThrowingService>();
        using var container = builder.Build();

        var exception = Record.Exception(() => container.Resolve<TestThrowingService>());

        Assert.IsType<NotSupportedException>(exception);
    }

    [Fact]
    public void Build_InstanceWithInjectMethod_InjectsDependencies()
    {
        var instance = new TestInjectable();
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestService>();
        builder.AddSingleton(instance);

        using var container = builder.Build();

        Assert.Same(container.Resolve<TestService>(), instance.Service);
    }

    [Fact]
    public void Build_CalledTwice_ThrowsInvalidOperationException()
    {
        var builder = new ContainerBuilder();
        using var container = builder.Build();

        var exception = Record.Exception(() => builder.Build());

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Build_AbstractType_ThrowsArgumentException()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton(typeof(ITestService), typeof(ITestService));

        var exception = Record.Exception(() => builder.Build());

        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void AddSingleton_ImplementationNotAssignable_ThrowsArgumentException()
    {
        var builder = new ContainerBuilder();

        var exception = Record.Exception(() => builder.AddSingleton(typeof(IDisposable), typeof(TestService)));

        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void AddSingleton_InstanceNotAssignable_ThrowsArgumentException()
    {
        var builder = new ContainerBuilder();

        var exception = Record.Exception(() => builder.AddSingleton(typeof(IDisposable), new TestService()));

        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void AddSingleton_TypeAsInstance_ThrowsArgumentException()
    {
        var builder = new ContainerBuilder();

        var exception = Record.Exception(() => builder.AddSingleton(typeof(TestService)));

        Assert.IsType<ArgumentException>(exception);
    }

    private interface ITestService
    {
    }

    private sealed class TestService : ITestService
    {
    }

    private sealed class TestConsumer
    {
        public TestService Service { get; }

        public TestConsumer(TestService service)
        {
            Service = service;
        }
    }

    private sealed class TestCycleA
    {
        public TestCycleA(TestCycleB b)
        {
        }
    }

    private sealed class TestCycleB
    {
        public TestCycleB(TestCycleA a)
        {
        }
    }

    private sealed class TestMultipleConstructors
    {
        public TestService? Service { get; }

        public TestMultipleConstructors()
        {
        }

        public TestMultipleConstructors(TestService service)
        {
            Service = service;
        }
    }

    private sealed class TestMarkedConstructor
    {
        public bool UsedMarkedConstructor { get; }

        [Inject]
        public TestMarkedConstructor()
        {
            UsedMarkedConstructor = true;
        }

        public TestMarkedConstructor(TestService service)
        {
        }
    }

    private sealed class TestThrowingService
    {
        public TestThrowingService()
        {
            throw new NotSupportedException();
        }
    }

    private sealed class TestInjectable
    {
        public TestService? Service { get; private set; }

        [Inject]
        public void Construct(TestService service)
        {
            Service = service;
        }
    }
}
