using System.Diagnostics.CodeAnalysis;
using System.Text;

using GroveGames.DependencyInjection.Registration;
using GroveGames.DependencyInjection.Resolution;

namespace GroveGames.DependencyInjection;

public sealed class ContainerBuilder : IContainerBuilder
{
    private readonly Container? _parent;
    private readonly List<ServiceRegistration> _registrations;
    private bool _isBuilt;

    public ContainerBuilder()
    {
        _parent = null;
        _registrations = [];
    }

    internal ContainerBuilder(Container parent)
    {
        _parent = parent;
        _registrations = [];
    }

    public IContainerBuilder AddSingleton(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
        ValidateImplementation(serviceType, implementationType);
        return Add(new ServiceRegistration(serviceType, implementationType, Lifetime.Singleton, null, null));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Instance registrations never use constructors.")]
    public IContainerBuilder AddSingleton(Type serviceType, object instance)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(instance);

        if (instance is Type)
        {
            throw new ArgumentException("A Type cannot be registered as an instance; use AddSingleton(serviceType, implementationType).", nameof(instance));
        }

        if (!serviceType.IsInstanceOfType(instance))
        {
            throw new ArgumentException($"{instance.GetType()} is not assignable to {serviceType}.", nameof(instance));
        }

        return Add(new ServiceRegistration(serviceType, instance.GetType(), Lifetime.Singleton, instance, null));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Factory registrations never use constructors.")]
    public IContainerBuilder AddSingleton(Type serviceType, Func<IObjectResolver, object> factory)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(factory);
        return Add(new ServiceRegistration(serviceType, serviceType, Lifetime.Singleton, null, factory));
    }

    public IContainerBuilder AddTransient(Type serviceType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
        ValidateImplementation(serviceType, implementationType);
        return Add(new ServiceRegistration(serviceType, implementationType, Lifetime.Transient, null, null));
    }

    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Factory registrations never use constructors.")]
    public IContainerBuilder AddTransient(Type serviceType, Func<IObjectResolver, object> factory)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(factory);
        return Add(new ServiceRegistration(serviceType, serviceType, Lifetime.Transient, null, factory));
    }

    public IContainer Build()
    {
        if (_isBuilt)
        {
            throw new InvalidOperationException("Container is already built.");
        }

        if (_parent is not null && _parent.IsDisposed)
        {
            throw new ObjectDisposedException(nameof(IContainer));
        }

        _isBuilt = true;
        var container = new Container(_parent, _parent?.Injectors ?? new InjectorCache());
        var self = new InstanceBinding(container);
        container.AddBinding(typeof(IObjectResolver), self);
        container.AddBinding(typeof(IContainer), self);
        var bindings = new Binding[_registrations.Count];
        var entryPoints = new List<Binding>();

        for (var i = 0; i < _registrations.Count; i++)
        {
            var registration = _registrations[i];
            var binding = CreateBinding(registration, container);
            bindings[i] = binding;
            container.AddBinding(registration.ServiceType, binding);

            if (registration.Instance is not null)
            {
                container.AddExternal(registration.Instance);
            }

            if (registration.Lifetime == Lifetime.Singleton && LifecycleTypes.IsEntryPoint(registration.ImplementationType))
            {
                entryPoints.Add(binding);
            }
        }

        for (var i = 0; i < bindings.Length; i++)
        {
            if (bindings[i].Activator is ConstructorActivator activator)
            {
                activator.Link(container);
            }
        }

        DetectCircularDependencies(bindings);
        container.SetEntryPoints(entryPoints.ToArray());
        _parent?.AddChild(container);

        try
        {
            for (var i = 0; i < _registrations.Count; i++)
            {
                var instance = _registrations[i].Instance;

                if (instance is not null)
                {
                    container.Inject(instance);
                }
            }
        }
        catch
        {
            container.Dispose();
            throw;
        }

        return container;
    }

    private static void ValidateImplementation(Type serviceType, Type implementationType)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        ArgumentNullException.ThrowIfNull(implementationType);

        if (!serviceType.IsAssignableFrom(implementationType))
        {
            throw new ArgumentException($"{implementationType} is not assignable to {serviceType}.", nameof(implementationType));
        }
    }

    private IContainerBuilder Add(ServiceRegistration registration)
    {
        if (_isBuilt)
        {
            throw new InvalidOperationException("Container is already built.");
        }

        _registrations.Add(registration);
        return this;
    }

    private static Binding CreateBinding(ServiceRegistration registration, Container container)
    {
        if (registration.Instance is not null)
        {
            return new InstanceBinding(registration.Instance);
        }

        InstanceActivator activator = registration.Factory is not null
            ? new FactoryActivator(registration.ImplementationType, registration.Factory, container)
            : new ConstructorActivator(registration.ImplementationType);

        return registration.Lifetime == Lifetime.Singleton
            ? new SingletonBinding(activator, container)
            : new TransientBinding(activator, container);
    }

    private static void DetectCircularDependencies(Binding[] bindings)
    {
        var visited = new HashSet<InstanceActivator>();
        var path = new List<InstanceActivator>();

        for (var i = 0; i < bindings.Length; i++)
        {
            var activator = bindings[i].Activator;

            if (activator is not null)
            {
                Visit(activator, visited, path);
            }
        }
    }

    private static void Visit(InstanceActivator activator, HashSet<InstanceActivator> visited, List<InstanceActivator> path)
    {
        if (visited.Contains(activator))
        {
            return;
        }

        var index = path.IndexOf(activator);

        if (index >= 0)
        {
            throw new CircularDependencyException(activator.ImplementationType, FormatPath(path, index, activator));
        }

        path.Add(activator);
        var dependencies = activator.Dependencies;

        for (var i = 0; i < dependencies.Length; i++)
        {
            var dependency = dependencies[i].Activator;

            if (dependency is not null)
            {
                Visit(dependency, visited, path);
            }
        }

        path.RemoveAt(path.Count - 1);
        visited.Add(activator);
    }

    private static string FormatPath(List<InstanceActivator> path, int start, InstanceActivator activator)
    {
        var builder = new StringBuilder();

        for (var i = start; i < path.Count; i++)
        {
            builder.Append(path[i].ImplementationType.Name).Append(" -> ");
        }

        return builder.Append(activator.ImplementationType.Name).ToString();
    }
}
