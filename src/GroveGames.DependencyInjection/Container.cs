using System.Diagnostics.CodeAnalysis;

using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

internal sealed class Container : IContainer
{
    private readonly Container? _parent;
    private readonly InjectorCache _injectors;
    private readonly Dictionary<Type, Binding> _bindings;
    private readonly List<IDisposable> _disposables;
    private readonly HashSet<object> _owned;
    private readonly CancellationTokenSource _disposeCancellation;
    private Binding[] _entryPoints;
    private Container[] _children;
    private IUpdatable[] _updatables;
    private IFixedUpdatable[] _fixedUpdatables;
    private ILateUpdatable[] _lateUpdatables;
    private bool _isInitializing;
    private bool _isInitialized;
    private bool _isDisposed;

    public IContainer? Parent => _parent;
    public bool IsInitialized => _isInitialized;
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
        _entryPoints = [];
        _children = [];
        _updatables = [];
        _fixedUpdatables = [];
        _lateUpdatables = [];
    }

    internal void AddBinding(Type serviceType, Binding binding)
    {
        if (_bindings.ContainsKey(serviceType))
        {
            throw new InvalidOperationException($"{serviceType} is already registered.");
        }

        _bindings.Add(serviceType, binding);
    }

    internal void SetEntryPoints(Binding[] entryPoints)
    {
        _entryPoints = entryPoints;
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
        return binding.Resolve();
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

        instance = binding.Resolve();
        return true;
    }

    public void Inject(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var injector = _injectors.Get(instance.GetType());

        if (injector.IsEmpty)
        {
            return;
        }

        injector.Inject(instance, this);
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

        if (_isInitializing || _isInitialized)
        {
            throw new InvalidOperationException("Container is already initialized.");
        }

        if (_parent is not null && !_parent._isInitialized)
        {
            throw new InvalidOperationException("Parent container must be initialized first.");
        }

        _isInitializing = true;

        try
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _disposeCancellation.Token);
            var token = cancellation.Token;
            var instances = ResolveEntryPoints();

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
                if (instances[i] is IAsyncInitializable initializable)
                {
                    await initializable.InitializeAsync(token);
                    token.ThrowIfCancellationRequested();
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

            for (var i = 0; i < instances.Length; i++)
            {
                if (instances[i] is IPreInitializable initializable)
                {
                    initializable.PreInitialize();
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
                if (instances[i] is IPostInitializable initializable)
                {
                    initializable.PostInitialize();
                }
            }

            token.ThrowIfCancellationRequested();
            _updatables = Collect<IUpdatable>(instances);
            _fixedUpdatables = Collect<IFixedUpdatable>(instances);
            _lateUpdatables = Collect<ILateUpdatable>(instances);
            _isInitialized = true;
        }
        finally
        {
            _isInitializing = false;
        }
    }

    public void Update(float deltaTime)
    {
        var updatables = _updatables;

        for (var i = 0; i < updatables.Length && !_isDisposed; i++)
        {
            updatables[i].Update(deltaTime);
        }

        var children = _children;

        for (var i = 0; i < children.Length && !_isDisposed; i++)
        {
            children[i].Update(deltaTime);
        }
    }

    public void FixedUpdate(float deltaTime)
    {
        var fixedUpdatables = _fixedUpdatables;

        for (var i = 0; i < fixedUpdatables.Length && !_isDisposed; i++)
        {
            fixedUpdatables[i].FixedUpdate(deltaTime);
        }

        var children = _children;

        for (var i = 0; i < children.Length && !_isDisposed; i++)
        {
            children[i].FixedUpdate(deltaTime);
        }
    }

    public void LateUpdate(float deltaTime)
    {
        var lateUpdatables = _lateUpdatables;

        for (var i = 0; i < lateUpdatables.Length && !_isDisposed; i++)
        {
            lateUpdatables[i].LateUpdate(deltaTime);
        }

        var children = _children;

        for (var i = 0; i < children.Length && !_isDisposed; i++)
        {
            children[i].LateUpdate(deltaTime);
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _disposeCancellation.Cancel();
        _updatables = [];
        _fixedUpdatables = [];
        _lateUpdatables = [];
        _entryPoints = [];
        List<Exception>? exceptions = null;
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
        _bindings.Clear();
        _parent?.RemoveChild(this);

        if (exceptions is not null)
        {
            throw new AggregateException(exceptions);
        }
    }

    private object[] ResolveEntryPoints()
    {
        var entryPoints = _entryPoints;
        var instances = new List<object>(entryPoints.Length);
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

        for (var i = 0; i < entryPoints.Length; i++)
        {
            var instance = entryPoints[i].Resolve();

            if (!seen.Add(instance) || (_parent is not null && _parent.IsOwned(instance)))
            {
                continue;
            }

            instances.Add(instance);
        }

        return instances.ToArray();
    }

    private static T[] Collect<T>(object[] instances)
        where T : class
    {
        var count = 0;

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is T)
            {
                count++;
            }
        }

        if (count == 0)
        {
            return [];
        }

        var result = new T[count];
        var index = 0;

        for (var i = 0; i < instances.Length; i++)
        {
            if (instances[i] is T item)
            {
                result[index++] = item;
            }
        }

        return result;
    }
}
