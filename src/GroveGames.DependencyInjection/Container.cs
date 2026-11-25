using System.Diagnostics.CodeAnalysis;

using GroveGames.DependencyInjection.Registration;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

internal sealed class Container : IContainer
{
    private readonly Container? _parent;
    private readonly InjectorCache _injectors;
    private Dictionary<Type, Binding> _bindings;
    private readonly List<IDisposable> _disposables;
    private readonly HashSet<object> _owned;
    private readonly CancellationTokenSource _disposeCancellation;
    private Type[] _singletonTypes;
    private Binding[] _singletonBindings;
    private Container[] _children;
    private IPreFrameUpdatable[] _preFrameUpdatables;
    private IFrameUpdatable[] _frameUpdatables;
    private IPostFrameUpdatable[] _postFrameUpdatables;
    private IPhysicsUpdatable[] _physicsUpdatables;
    private bool _hasStartedInitialization;
    private bool _isInitialized;
    private bool _isDisposed;
    private bool _isUpdating;

    public IContainer? Parent => _parent;
    public bool IsDisposed => _isDisposed;
    internal InjectorCache Injectors => _injectors;

    internal Container(Container? parent, InjectorCache injectors)
    {
        _parent = parent;
        _injectors = injectors;
        _bindings = [];
        _disposables = [];
        _owned = new HashSet<object>(ReferenceEqualityComparer.Instance);
        _disposeCancellation = new CancellationTokenSource();
        _singletonTypes = [];
        _singletonBindings = [];
        _children = [];
        _preFrameUpdatables = [];
        _frameUpdatables = [];
        _postFrameUpdatables = [];
        _physicsUpdatables = [];
    }

    internal void SetBindings(Dictionary<Type, Binding> bindings)
    {
        _bindings = bindings;
    }

    internal void SetSingletons(Type[] implementationTypes, Binding[] bindings)
    {
        _singletonTypes = implementationTypes;
        _singletonBindings = bindings;
    }

    internal Binding? FindBinding(Type serviceType)
    {
        for (var container = this; container is not null; container = container._parent)
        {
            if (container._bindings.TryGetValue(serviceType, out var binding))
            {
                return binding;
            }
        }

        return null;
    }

    internal void AddExternal(object instance)
    {
        _owned.Add(instance);
    }

    internal void OwnSingleton(object instance)
    {
        if (IsOwned(instance))
        {
            return;
        }

        _owned.Add(instance);

        if (instance is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }

    internal void OwnTransient(object instance)
    {
        if (instance is not IDisposable disposable || IsOwned(instance))
        {
            return;
        }

        _owned.Add(instance);
        _disposables.Add(disposable);
    }

    private bool IsOwned(object instance)
    {
        for (var container = this; container is not null; container = container._parent)
        {
            if (container._owned.Contains(instance))
            {
                return true;
            }
        }

        return false;
    }

    internal void AddChild(Container child)
    {
        var children = _children;
        var newChildren = new Container[children.Length + 1];
        Array.Copy(children, newChildren, children.Length);
        newChildren[children.Length] = child;
        _children = newChildren;
    }

    internal void RemoveChild(Container child)
    {
        var children = _children;
        var index = Array.IndexOf(children, child);

        if (index < 0)
        {
            return;
        }

        if (children.Length == 1)
        {
            _children = [];
            return;
        }

        var newChildren = new Container[children.Length - 1];
        Array.Copy(children, 0, newChildren, 0, index);
        Array.Copy(children, index + 1, newChildren, index, children.Length - index - 1);
        _children = newChildren;
    }

    public object Resolve(Type serviceType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var binding = FindBinding(serviceType) ?? throw new RegistrationNotFoundException(serviceType);
        return binding.Resolve(this);
    }

    public bool TryResolve(Type serviceType, [NotNullWhen(true)] out object? instance)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var binding = FindBinding(serviceType);

        if (binding is null)
        {
            instance = null;
            return false;
        }

        instance = binding.Resolve(this);
        return true;
    }

    public void Inject(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var injector = _injectors.Get(instance.GetType());

        if (injector.IsEmpty || IsOwned(instance))
        {
            return;
        }

        injector.Inject(instance, this);
    }

    internal void InjectInstance(object instance)
    {
        var injector = _injectors.Get(instance.GetType());

        if (injector.IsEmpty)
        {
            return;
        }

        injector.Inject(instance, this);
    }

    public IReadOnlyList<T> ResolveAll<T>()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return ResolveAllArray<T>();
    }

    private T[] ResolveAllArray<T>()
    {
        var types = _singletonTypes;
        var bindings = _singletonBindings;
        List<object>? found = null;

        for (var i = 0; i < types.Length; i++)
        {
            if (!typeof(T).IsAssignableFrom(types[i]))
            {
                continue;
            }

            var instance = bindings[i].Resolve(this);

            if (instance is not T || (_parent is not null && _parent.IsOwned(instance)) || (found is not null && ContainsReference(found, instance)))
            {
                continue;
            }

            (found ??= []).Add(instance);
        }

        if (found is null)
        {
            return Array.Empty<T>();
        }

        var result = new T[found.Count];

        for (var i = 0; i < result.Length; i++)
        {
            result[i] = (T)found[i];
        }

        return result;
    }

    public void AddDisposable(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _disposables.Add(disposable);
    }

    public IContainer CreateChild(Action<IContainerBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var builder = new ContainerBuilder(this);
        configure.Invoke(builder);
        return builder.Build();
    }

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_hasStartedInitialization)
        {
            throw new InvalidOperationException(_isInitialized ? "Container is already initialized." : "Container initialization already ran and did not complete; build a new container to retry.");
        }

        if (_parent is not null && !_parent._isInitialized)
        {
            throw new InvalidOperationException("Parent container must be initialized first.");
        }

        _hasStartedInitialization = true;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCancellation.Token);
        var token = cancellation.Token;
        token.ThrowIfCancellationRequested();
        var instances = ResolveEntryPoints();

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is IPreInitializable initializable)
            {
                initializable.PreInitialize();
            }
        }

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is IAsyncPreInitializable initializable)
            {
                await initializable.PreInitializeAsync(token);
                token.ThrowIfCancellationRequested();
            }
        }

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is IInitializable initializable)
            {
                initializable.Initialize();
            }
        }

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is IAsyncInitializable initializable)
            {
                await initializable.InitializeAsync(token);
                token.ThrowIfCancellationRequested();
            }
        }

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is IPostInitializable initializable)
            {
                initializable.PostInitialize();
            }
        }

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is IAsyncPostInitializable initializable)
            {
                await initializable.PostInitializeAsync(token);
                token.ThrowIfCancellationRequested();
            }
        }

        token.ThrowIfCancellationRequested();
        _preFrameUpdatables = ResolveAllArray<IPreFrameUpdatable>();
        _frameUpdatables = ResolveAllArray<IFrameUpdatable>();
        _postFrameUpdatables = ResolveAllArray<IPostFrameUpdatable>();
        _physicsUpdatables = ResolveAllArray<IPhysicsUpdatable>();
        _isInitialized = true;
    }

    public void PreFrameUpdate(DeltaTime deltaTime)
    {
        Update(deltaTime, static container => container._preFrameUpdatables, static (updatable, time) => updatable.PreFrameUpdate(time));
    }

    public void FrameUpdate(DeltaTime deltaTime)
    {
        Update(deltaTime, static container => container._frameUpdatables, static (updatable, time) => updatable.FrameUpdate(time));
    }

    public void PostFrameUpdate(DeltaTime deltaTime)
    {
        Update(deltaTime, static container => container._postFrameUpdatables, static (updatable, time) => updatable.PostFrameUpdate(time));
    }

    public void PhysicsUpdate(DeltaTime deltaTime)
    {
        Update(deltaTime, static container => container._physicsUpdatables, static (updatable, time) => updatable.PhysicsUpdate(time));
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        List<Exception>? exceptions = null;

        try
        {
            _disposeCancellation.Cancel();
        }
        catch (Exception exception)
        {
            (exceptions ??= []).Add(exception);
        }

        _disposeCancellation.Dispose();
        _singletonTypes = [];
        _singletonBindings = [];
        _preFrameUpdatables = [];
        _frameUpdatables = [];
        _postFrameUpdatables = [];
        _physicsUpdatables = [];
        var children = _children;
        _children = [];

        for (var i = children.Length - 1; i >= 0; i--)
        {
            try
            {
                children[i].Dispose();
            }
            catch (Exception exception)
            {
                (exceptions ??= []).Add(exception);
            }
        }

        for (var i = _disposables.Count - 1; i >= 0; i--)
        {
            try
            {
                _disposables[i].Dispose();
            }
            catch (Exception exception)
            {
                (exceptions ??= []).Add(exception);
            }
        }

        _disposables.Clear();
        _owned.Clear();
        _bindings = [];
        _parent?.RemoveChild(this);

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }

    private static bool ContainsReference(List<object> items, object instance)
    {
        for (var i = 0; i < items.Count; i++)
        {
            if (ReferenceEquals(items[i], instance))
            {
                return true;
            }
        }

        return false;
    }

    private void Update<T>(DeltaTime deltaTime, Func<Container, T[]> select, Action<T, DeltaTime> call)
    {
        if (_isUpdating)
        {
            throw new InvalidOperationException("A container cannot be updated again from inside its own update.");
        }

        List<Exception>? exceptions = null;
        Update(deltaTime, select, call, ref exceptions);

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }

    private void Update<T>(DeltaTime deltaTime, Func<Container, T[]> select, Action<T, DeltaTime> call, ref List<Exception>? exceptions)
    {
        if (_isDisposed || !_isInitialized)
        {
            return;
        }

        _isUpdating = true;

        try
        {
            var updatables = select(this);

            for (var i = 0; i < updatables.Length && !_isDisposed; i++)
            {
                try
                {
                    call(updatables[i], deltaTime);
                }
                catch (Exception exception)
                {
                    (exceptions ??= []).Add(exception);
                }
            }

            var children = _children;

            for (var i = 0; i < children.Length && !_isDisposed; i++)
            {
                children[i].Update(deltaTime, select, call, ref exceptions);
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private object[] ResolveEntryPoints()
    {
        var types = _singletonTypes;
        var bindings = _singletonBindings;
        var instances = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

        for (var i = 0; i < types.Length; i++)
        {
            if (!LifecycleTypes.IsEntryPoint(types[i]))
            {
                continue;
            }

            var instance = bindings[i].Resolve(this);

            if (!seen.Add(instance) || (_parent is not null && _parent.IsOwned(instance)))
            {
                continue;
            }

            instances.Add(instance);
        }

        return instances.ToArray();
    }
}
