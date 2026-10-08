namespace GroveGames.DependencyInjection.Tests;

public sealed class ContainerTests
{
    [Fact]
    public void Resolve_ObjectResolver_ReturnsContainer()
    {
        using var container = new ContainerBuilder().Build();

        var resolver = container.Resolve<IObjectResolver>();

        Assert.Same(container, resolver);
    }

    [Fact]
    public void Resolve_UnregisteredType_ThrowsRegistrationNotFoundException()
    {
        using var container = new ContainerBuilder().Build();

        var exception = Record.Exception(() => container.Resolve<TestLog>());

        Assert.IsType<RegistrationNotFoundException>(exception);
    }

    [Fact]
    public void Resolve_AfterDispose_ThrowsObjectDisposedException()
    {
        var container = new ContainerBuilder().Build();
        container.Dispose();

        var exception = Record.Exception(() => container.Resolve<IObjectResolver>());

        Assert.IsType<ObjectDisposedException>(exception);
    }

    [Fact]
    public void CreateChild_ParentRegistration_ResolvesFromParent()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestLog>();
        using var parent = builder.Build();

        var child = parent.CreateChild(_ => { });

        Assert.Same(parent.Resolve<TestLog>(), child.Resolve<TestLog>());
        Assert.Same(parent, child.Parent);
    }

    [Fact]
    public void CreateChild_SameServiceType_OverridesParent()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestLog>();
        using var parent = builder.Build();

        var child = parent.CreateChild(childBuilder => childBuilder.AddSingleton<TestLog>());

        Assert.NotSame(parent.Resolve<TestLog>(), child.Resolve<TestLog>());
    }

    [Fact]
    public void CreateChild_Installer_InstallsRegistrations()
    {
        using var parent = new ContainerBuilder().Build();

        var child = parent.CreateChild(new TestInstaller());

        Assert.True(child.TryResolve<TestLog>(out _));
    }

    [Fact]
    public void Inject_PublicInjectMethod_InjectsDependencies()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestLog>();
        using var container = builder.Build();
        var injectable = new TestInjectable();

        container.Inject(injectable);

        Assert.Same(container.Resolve<TestLog>(), injectable.Log);
    }

    [Fact]
    public async Task InitializeAsync_AllPhases_RunsEachPhaseSyncThenAsync()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton<TestEntryPoint>();
        using var container = builder.Build();

        await container.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["PreInitialize", "AsyncPreInitialize", "Initialize", "AsyncInitialize", "PostInitialize", "AsyncPostInitialize"], log.Entries);
        Assert.True(container.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_MultipleEntryPoints_RunsEachPhaseInRegistrationOrder()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver => new TestNamedEntryPoint("A", resolver.Resolve<TestLog>()));
        builder.AddSingleton<IInitializable>(resolver => new TestNamedEntryPoint("B", resolver.Resolve<TestLog>()));
        using var container = builder.Build();

        await container.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["A.Initialize", "B.Initialize", "A.InitializeAsync", "B.InitializeAsync"], log.Entries);
    }

    [Fact]
    public async Task InitializeAsync_AsyncInitializerRegisteredBeforeItsSyncDependency_SeesDependencyInitialized()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestConsumer>();
        builder.AddSingleton<TestSettings>();
        using var container = builder.Build();

        await container.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.True(container.Resolve<TestConsumer>().SawLoadedSettings);
    }

    [Fact]
    public async Task InitializeAsync_EntryPointNotResolvedBefore_CreatesEntryPoint()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton<IInitializable, TestInitializable>();
        using var container = builder.Build();

        await container.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["Initialize"], log.Entries);
    }

    [Fact]
    public async Task InitializeAsync_AliasOfEntryPoint_InitializesOnce()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton<TestInitializable>();
        builder.AddSingleton<IInitializable>(resolver => resolver.Resolve<TestInitializable>());
        using var container = builder.Build();

        await container.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["Initialize"], log.Entries);
    }

    [Fact]
    public async Task InitializeAsync_ChildAliasOfParentEntryPoint_DoesNotInitializeAgain()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton<TestInitializable>();
        using var parent = builder.Build();
        await parent.InitializeAsync(TestContext.Current.CancellationToken);
        var child = parent.CreateChild(childBuilder => childBuilder.AddSingleton<IInitializable>(resolver => resolver.Resolve<TestInitializable>()));

        await child.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["Initialize"], log.Entries);
    }

    [Fact]
    public async Task InitializeAsync_TransientEntryPoint_IsNotInitialized()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddTransient<TestInitializable>();
        using var container = builder.Build();

        await container.InitializeAsync(TestContext.Current.CancellationToken);

        Assert.Empty(log.Entries);
    }

    [Fact]
    public async Task InitializeAsync_CalledTwice_ThrowsInvalidOperationException()
    {
        using var container = new ContainerBuilder().Build();
        await container.InitializeAsync(TestContext.Current.CancellationToken);

        var exception = await Record.ExceptionAsync(() => container.InitializeAsync(TestContext.Current.CancellationToken).AsTask());

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task InitializeAsync_ParentNotInitialized_ThrowsInvalidOperationException()
    {
        using var parent = new ContainerBuilder().Build();
        var child = parent.CreateChild(_ => { });

        var exception = await Record.ExceptionAsync(() => child.InitializeAsync(TestContext.Current.CancellationToken).AsTask());

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public async Task InitializeAsync_DisposedDuringAsyncPhase_SkipsLaterPhases()
    {
        var log = new TestLog();
        var gate = new TaskCompletionSource<bool>();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(gate);
        builder.AddSingleton<TestBlockingEntryPoint>();
        var container = builder.Build();
        var initialization = container.InitializeAsync(TestContext.Current.CancellationToken).AsTask();

        container.Dispose();
        gate.SetResult(true);
        var exception = await Record.ExceptionAsync(() => initialization);

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
        Assert.DoesNotContain("PostInitialize", log.Entries);
        Assert.False(container.IsInitialized);
    }

    [Fact]
    public async Task InitializeAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton<TestEntryPoint>();
        using var container = builder.Build();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exception = await Record.ExceptionAsync(() => container.InitializeAsync(cancellation.Token).AsTask());

        Assert.IsAssignableFrom<OperationCanceledException>(exception);
        Assert.Empty(log.Entries);
    }

    [Fact]
    public void ResolveAll_MatchingSingletons_ReturnsInRegistrationOrder()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestTickableB>();
        builder.AddSingleton<TestLog>();
        builder.AddSingleton<TestTickableA>();
        using var container = builder.Build();

        var tickables = container.ResolveAll<ITestTickable>();

        Assert.Equal(2, tickables.Count);
        Assert.IsType<TestTickableB>(tickables[0]);
        Assert.IsType<TestTickableA>(tickables[1]);
    }

    [Fact]
    public void ResolveAll_AliasedSingleton_ReturnsInstanceOnce()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestTickableA>();
        builder.AddSingleton<ITestTickable>(resolver => resolver.Resolve<TestTickableA>());
        using var container = builder.Build();

        var tickables = container.ResolveAll<ITestTickable>();

        Assert.Single(tickables);
    }

    [Fact]
    public void ResolveAll_TransientRegistration_IsExcluded()
    {
        var builder = new ContainerBuilder();
        builder.AddTransient<TestTickableA>();
        using var container = builder.Build();

        var tickables = container.ResolveAll<ITestTickable>();

        Assert.Empty(tickables);
    }

    [Fact]
    public void ResolveAll_ChildAliasOfParentInstance_IsExcluded()
    {
        var builder = new ContainerBuilder();
        builder.AddSingleton<TestTickableA>();
        using var parent = builder.Build();
        var child = parent.CreateChild(childBuilder =>
        {
            childBuilder.AddSingleton<ITestTickable>(resolver => resolver.Resolve<TestTickableA>());
            childBuilder.AddSingleton<TestTickableB>();
        });

        var tickables = child.ResolveAll<ITestTickable>();

        Assert.Single(tickables);
        Assert.IsType<TestTickableB>(tickables[0]);
    }

    [Fact]
    public void Dispose_CreatedInstances_DisposesInReverseCreationOrder()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver => new TestDisposable("A", resolver.Resolve<TestLog>()));
        builder.AddTransient(resolver => new TestOtherDisposable(resolver.Resolve<TestLog>()));
        var container = builder.Build();
        container.Resolve<TestDisposable>();
        container.Resolve<TestOtherDisposable>();

        container.Dispose();

        Assert.Equal(["Other.Dispose", "A.Dispose"], log.Entries);
    }

    [Fact]
    public void Dispose_RegisteredInstance_DoesNotDisposeInstance()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(new TestDisposable("A", log));
        var container = builder.Build();
        container.Resolve<TestDisposable>();

        container.Dispose();

        Assert.Empty(log.Entries);
    }

    [Fact]
    public void Dispose_AliasFactory_DisposesOnce()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver => new TestDisposable("A", resolver.Resolve<TestLog>()));
        builder.AddSingleton<IDisposable>(resolver => resolver.Resolve<TestDisposable>());
        var container = builder.Build();
        container.Resolve<IDisposable>();

        container.Dispose();

        Assert.Equal(["A.Dispose"], log.Entries);
    }

    [Fact]
    public void Dispose_AliasOfRegisteredInstance_DoesNotDisposeInstance()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(new TestDisposable("A", log));
        builder.AddSingleton<IDisposable>(resolver => resolver.Resolve<TestDisposable>());
        var container = builder.Build();
        container.Resolve<IDisposable>();

        container.Dispose();

        Assert.Empty(log.Entries);
    }

    [Fact]
    public void Dispose_ChildAliasOfParentInstance_DoesNotDisposeParentInstance()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver => new TestDisposable("Parent", resolver.Resolve<TestLog>()));
        using var parent = builder.Build();
        var child = parent.CreateChild(childBuilder => childBuilder.AddSingleton<IDisposable>(resolver => resolver.Resolve<TestDisposable>()));
        child.Resolve<IDisposable>();

        child.Dispose();

        Assert.Empty(log.Entries);
    }

    [Fact]
    public void Dispose_Parent_DisposesChildrenFirst()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver => new TestDisposable("Parent", resolver.Resolve<TestLog>()));
        var parent = builder.Build();
        parent.Resolve<TestDisposable>();
        var child = parent.CreateChild(childBuilder => childBuilder.AddSingleton(resolver => new TestDisposable("Child", resolver.Resolve<TestLog>())));
        child.Resolve<TestDisposable>();

        parent.Dispose();

        Assert.Equal(["Child.Dispose", "Parent.Dispose"], log.Entries);
        Assert.True(child.IsDisposed);
    }

    [Fact]
    public void AddDisposable_ContainerDisposed_DisposesAfterLaterCreatedInstances()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver =>
        {
            resolver.AddDisposable(new TestDisposable("Owned", resolver.Resolve<TestLog>()));
            return new TestDisposable("Created", resolver.Resolve<TestLog>());
        });
        var container = builder.Build();
        container.Resolve<TestDisposable>();

        container.Dispose();

        Assert.Equal(["Created.Dispose", "Owned.Dispose"], log.Entries);
    }

    [Fact]
    public void AddDisposable_AfterDispose_ThrowsObjectDisposedException()
    {
        var log = new TestLog();
        var container = new ContainerBuilder().Build();
        container.Dispose();

        var exception = Record.Exception(() => container.AddDisposable(new TestDisposable("A", log)));

        Assert.IsType<ObjectDisposedException>(exception);
    }

    [Fact]
    public void Dispose_CalledTwice_DisposesOnce()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver => new TestDisposable("A", resolver.Resolve<TestLog>()));
        var container = builder.Build();
        container.Resolve<TestDisposable>();

        container.Dispose();
        container.Dispose();

        Assert.Single(log.Entries);
    }

    [Fact]
    public void Dispose_DisposableThrows_DisposesRemainingAndThrowsAggregateException()
    {
        var log = new TestLog();
        var builder = new ContainerBuilder();
        builder.AddSingleton(log);
        builder.AddSingleton(resolver => new TestDisposable("A", resolver.Resolve<TestLog>()));
        builder.AddSingleton<TestThrowingDisposable>();
        var container = builder.Build();
        container.Resolve<TestDisposable>();
        container.Resolve<TestThrowingDisposable>();

        var exception = Record.Exception(() => container.Dispose());

        Assert.IsType<AggregateException>(exception);
        Assert.Equal(["A.Dispose"], log.Entries);
    }

    private sealed class TestLog
    {
        public List<string> Entries { get; }

        public TestLog()
        {
            Entries = [];
        }
    }

    private sealed class TestInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.AddSingleton<TestLog>();
        }
    }

    private sealed class TestInjectable
    {
        public TestLog? Log { get; private set; }

        [Inject]
        public void Construct(TestLog log)
        {
            Log = log;
        }
    }

    private sealed class TestEntryPoint : IAsyncPreInitializable, IAsyncInitializable, IAsyncPostInitializable, IPreInitializable, IInitializable, IPostInitializable
    {
        private readonly TestLog _log;

        public TestEntryPoint(TestLog log)
        {
            _log = log;
        }

        public async ValueTask PreInitializeAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            _log.Entries.Add("AsyncPreInitialize");
        }

        public async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            _log.Entries.Add("AsyncInitialize");
        }

        public async ValueTask PostInitializeAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            _log.Entries.Add("AsyncPostInitialize");
        }

        public void PreInitialize()
        {
            _log.Entries.Add("PreInitialize");
        }

        public void Initialize()
        {
            _log.Entries.Add("Initialize");
        }

        public void PostInitialize()
        {
            _log.Entries.Add("PostInitialize");
        }
    }

    private sealed class TestNamedEntryPoint : IAsyncInitializable, IInitializable
    {
        private readonly string _name;
        private readonly TestLog _log;

        public TestNamedEntryPoint(string name, TestLog log)
        {
            _name = name;
            _log = log;
        }

        public ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            _log.Entries.Add($"{_name}.InitializeAsync");
            return default;
        }

        public void Initialize()
        {
            _log.Entries.Add($"{_name}.Initialize");
        }
    }

    private sealed class TestSettings : IInitializable
    {
        public bool IsLoaded { get; private set; }

        public void Initialize()
        {
            IsLoaded = true;
        }
    }

    private sealed class TestConsumer : IAsyncInitializable
    {
        private readonly TestSettings _settings;

        public TestConsumer(TestSettings settings)
        {
            _settings = settings;
        }

        public bool SawLoadedSettings { get; private set; }

        public async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            SawLoadedSettings = _settings.IsLoaded;
        }
    }

    private sealed class TestInitializable : IInitializable
    {
        private readonly TestLog _log;

        public TestInitializable(TestLog log)
        {
            _log = log;
        }

        public void Initialize()
        {
            _log.Entries.Add("Initialize");
        }
    }

    private sealed class TestBlockingEntryPoint : IAsyncInitializable, IPostInitializable
    {
        private readonly TestLog _log;
        private readonly TaskCompletionSource<bool> _gate;

        public TestBlockingEntryPoint(TestLog log, TaskCompletionSource<bool> gate)
        {
            _log = log;
            _gate = gate;
        }

        public async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            await _gate.Task;
        }

        public void PostInitialize()
        {
            _log.Entries.Add("PostInitialize");
        }
    }

    private interface ITestTickable
    {
    }

    private sealed class TestTickableA : ITestTickable
    {
    }

    private sealed class TestTickableB : ITestTickable
    {
    }

    private sealed class TestDisposable : IDisposable
    {
        private readonly string _name;
        private readonly TestLog _log;

        public TestDisposable(string name, TestLog log)
        {
            _name = name;
            _log = log;
        }

        public void Dispose()
        {
            _log.Entries.Add($"{_name}.Dispose");
        }
    }

    private sealed class TestOtherDisposable : IDisposable
    {
        private readonly TestLog _log;

        public TestOtherDisposable(TestLog log)
        {
            _log = log;
        }

        public void Dispose()
        {
            _log.Entries.Add("Other.Dispose");
        }
    }

    private sealed class TestThrowingDisposable : IDisposable
    {
        public void Dispose()
        {
            throw new InvalidOperationException();
        }
    }
}
