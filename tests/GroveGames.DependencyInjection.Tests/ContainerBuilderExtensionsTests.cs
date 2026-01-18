namespace GroveGames.DependencyInjection.Tests;

public class ContainerBuilderExtensionsTests
{
    [Fact]
    public void AddSingleton_WithInstance_ShouldAddSingletonWithImplementationType()
    {
        var instance = new MockTest();
        var mockContainerBuilder = new TestContainerBuilder();

        mockContainerBuilder.AddSingleton(instance);

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(typeof(MockTest), mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal(instance, mockContainerBuilder.Calls[0].Value);
        Assert.Equal("AddSingleton_Instance", mockContainerBuilder.Calls[0].Method);
    }

    [Fact]
    public void AddSingleton_WithType_ShouldAddSingletonWithImplementationType()
    {
        var type = typeof(MockTest);
        var mockContainerBuilder = new TestContainerBuilder();

        mockContainerBuilder.AddSingleton(type);

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(type, mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal(type, mockContainerBuilder.Calls[0].Value);
        Assert.Equal("AddSingleton_Type", mockContainerBuilder.Calls[0].Method);
    }

    [Fact]
    public void AddTransient_WithType_ShouldAddTransientWithImplementationType()
    {
        var type = typeof(MockTest);
        var mockContainerBuilder = new TestContainerBuilder();

        mockContainerBuilder.AddTransient(type);

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(type, mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal(type, mockContainerBuilder.Calls[0].Value);
        Assert.Equal("AddTransient_Type", mockContainerBuilder.Calls[0].Method);
    }

    [Fact]
    public void AddSingleton_Generic_WithFactory_ShouldAddSingletonUsingFactory()
    {
        var mockContainerBuilder = new TestContainerBuilder();
        Func<MockTest> factory = () => new MockTest();

        mockContainerBuilder.AddSingleton(factory);

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(typeof(MockTest), mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal("AddSingleton_Factory", mockContainerBuilder.Calls[0].Method);
    }

    [Fact]
    public void AddTransient_Generic_WithFactory_ShouldAddTransientUsingFactory()
    {
        var mockContainerBuilder = new TestContainerBuilder();
        Func<MockTest> factory = () => new MockTest();

        mockContainerBuilder.AddTransient(factory);

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(typeof(MockTest), mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal("AddTransient_Factory", mockContainerBuilder.Calls[0].Method);
    }

    [Fact]
    public void AddSingleton_Generic_ShouldAddSingletonWithType()
    {
        var mockContainerBuilder = new TestContainerBuilder();

        mockContainerBuilder.AddSingleton<IMockTest, MockTest>();

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(typeof(IMockTest), mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal(typeof(MockTest), mockContainerBuilder.Calls[0].Value);
        Assert.Equal("AddSingleton_Type", mockContainerBuilder.Calls[0].Method);
    }

    [Fact]
    public void AddTransient_Generic_ShouldAddTransientWithType()
    {
        var mockContainerBuilder = new TestContainerBuilder();

        mockContainerBuilder.AddTransient<IMockTest, MockTest>();

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(typeof(IMockTest), mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal(typeof(MockTest), mockContainerBuilder.Calls[0].Value);
        Assert.Equal("AddTransient_Type", mockContainerBuilder.Calls[0].Method);
    }

    [Fact]
    public void AddSingleton_Generic_WithInstance_ShouldAddSingletonInstance()
    {
        var instance = new MockTest();
        var mockContainerBuilder = new TestContainerBuilder();

        mockContainerBuilder.AddSingleton<IMockTest>(instance);

        Assert.Single(mockContainerBuilder.Calls);
        Assert.Equal(typeof(IMockTest), mockContainerBuilder.Calls[0].RegistrationType);
        Assert.Equal(instance, mockContainerBuilder.Calls[0].Value);
        Assert.Equal("AddSingleton_Instance", mockContainerBuilder.Calls[0].Method);
    }

    private interface IMockTest
    {
        public void Execute();
    }

    private sealed class MockTest : IMockTest
    {
        public void Execute() { }
    }

    private sealed class TestContainerBuilder : IContainerBuilder
    {
        private readonly List<(Type RegistrationType, object Value, string Method)> _calls = new();

        public IReadOnlyList<(Type RegistrationType, object Value, string Method)> Calls => _calls;

        public IContainerBuilder AddSingleton(Type registrationType, object implementationInstance)
        {
            _calls.Add((registrationType, implementationInstance, "AddSingleton_Instance"));
            return this;
        }

        public IContainerBuilder AddSingleton(Type registrationType, Type implementationType)
        {
            _calls.Add((registrationType, implementationType, "AddSingleton_Type"));
            return this;
        }

        public IContainerBuilder AddSingleton(Type registrationType, Func<object> instanceFactory)
        {
            _calls.Add((registrationType, instanceFactory, "AddSingleton_Factory"));
            return this;
        }

        public IContainerBuilder AddTransient(Type registrationType, Type implementationType)
        {
            _calls.Add((registrationType, implementationType, "AddTransient_Type"));
            return this;
        }

        public IContainerBuilder AddTransient(Type registrationType, Func<object> instanceFactory)
        {
            _calls.Add((registrationType, instanceFactory, "AddTransient_Factory"));
            return this;
        }
    }
}
