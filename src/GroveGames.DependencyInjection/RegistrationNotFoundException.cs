namespace GroveGames.DependencyInjection;

public sealed class RegistrationNotFoundException : InvalidOperationException
{
    private readonly Type _type;
    private readonly Type? _dependentType;

    public Type Type => _type;
    public Type? DependentType => _dependentType;

    public RegistrationNotFoundException(Type type)
        : base($"No registration found for type {type}.")
    {
        _type = type;
    }

    public RegistrationNotFoundException(Type type, Type dependentType)
        : base($"No registration found for type {type} required by {dependentType}.")
    {
        _type = type;
        _dependentType = dependentType;
    }
}
