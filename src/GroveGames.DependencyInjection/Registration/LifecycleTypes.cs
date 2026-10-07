namespace GroveGames.DependencyInjection.Registration;

internal static class LifecycleTypes
{
    public static bool IsEntryPoint(Type type)
    {
        return typeof(IAsyncPreInitializable).IsAssignableFrom(type)
            || typeof(IAsyncInitializable).IsAssignableFrom(type)
            || typeof(IAsyncPostInitializable).IsAssignableFrom(type)
            || typeof(IPreInitializable).IsAssignableFrom(type)
            || typeof(IInitializable).IsAssignableFrom(type)
            || typeof(IPostInitializable).IsAssignableFrom(type)
            || typeof(IUpdatable).IsAssignableFrom(type)
            || typeof(IFixedUpdatable).IsAssignableFrom(type)
            || typeof(ILateUpdatable).IsAssignableFrom(type);
    }
}
