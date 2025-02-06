namespace GroveGames.DependencyInjection;

public class RegistrationNotFoundException : InvalidOperationException
{
    public Type Type { get; }

    public RegistrationNotFoundException(Type type)
        : base($"No registration found for type {type}.")
    {
        Type = type;
    }
}