namespace GroveGames.DependencyInjection;

public sealed class RegistrationNotFoundException : InvalidOperationException
{
    public Type Type { get; }
    public Type? DependentType { get; }

    public RegistrationNotFoundException(Type type)
        : base($"No registration found for type {type}.")
    {
        Type = type;
    }

    public RegistrationNotFoundException(Type type, Type dependentType)
        : base($"No registration found for type {type} required by {dependentType}.")
    {
        Type = type;
        DependentType = dependentType;
    }
}
