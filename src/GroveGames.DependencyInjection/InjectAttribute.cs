namespace GroveGames.DependencyInjection;

[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
public sealed class InjectAttribute : Attribute
{
}
