using System.Diagnostics.CodeAnalysis;

namespace GroveGames.DependencyInjection;

public interface IObjectResolver
{
    public object Resolve(Type serviceType);
    public bool TryResolve(Type serviceType, [NotNullWhen(true)] out object? instance);
    public void Inject(object instance);
}
