namespace GroveGames.DependencyInjection;

public sealed class CircularDependencyException : InvalidOperationException
{
    private readonly Type _type;

    public Type Type => _type;

    public CircularDependencyException(Type type)
        : base($"Circular dependency detected while resolving {type}.")
    {
        _type = type;
    }

    public CircularDependencyException(Type type, string path)
        : base($"Circular dependency detected while resolving {type}: {path}.")
    {
        _type = type;
    }
}
