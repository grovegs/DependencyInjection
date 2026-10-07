namespace GroveGames.DependencyInjection;

public sealed class CircularDependencyException : InvalidOperationException
{
    public Type Type { get; }

    public CircularDependencyException(Type type)
        : base($"Circular dependency detected while resolving {type}.")
    {
        Type = type;
    }

    public CircularDependencyException(Type type, string path)
        : base($"Circular dependency detected while resolving {type}: {path}.")
    {
        Type = type;
    }
}
