using System.Diagnostics.CodeAnalysis;

using GroveGames.DependencyInjection.Registration;
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
    private Type[] _singletonTypes;
    private Binding[] _singletonBindings;
    private Container[] _children;
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
        _singletonTypes = [];
        _singletonBindings = [];
        _children = [];
    }

    internal void AddBinding(Type serviceType, Binding binding)
    {
        if (_bindings.ContainsKey(serviceType))
        {
            throw new InvalidOperationException($"{serviceType} is already registered.");
        }

        _bindings.Add(serviceType, binding);
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

    public IReadOnlyList<T> ResolveAll<T>()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        var types = _singletonTypes;
        var bindings = _singletonBindings;
        List<T>? result = null;
        HashSet<object>? seen = null;

        for (var i = 0; i < types.Length; i++)
        {
            if (!typeof(T).IsAssignableFrom(types[i]))
            {
                continue;
            }

            var instance = bindings[i].Resolve();

            if (instance is not T item || (_parent is not null && _parent.IsOwned(instance)))
            {
                continue;
            }

            seen ??= new HashSet<object>(ReferenceEqualityComparer.Instance);

            if (seen.Add(instance))
            {
                (result ??= []).Add(item);
            }
        }

        return result is null ? [] : result.ToArray();
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
            _isInitialized = true;
        }
        finally
        {
            _isInitializing = false;
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
        _singletonTypes = [];
        _singletonBindings = [];
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

            var instance = bindings[i].Resolve();

            if (!seen.Add(instance) || (_parent is not null && _parent.IsOwned(instance)))
            {
                continue;
            }

            instances.Add(instance);
        }

        return instances.ToArray();
    }
}
