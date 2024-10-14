namespace GroveGames.DependencyInjection.Collections;

internal interface IInitializableCollection : IInitializable
{
    void TryAdd(Type registrationType, Type implementationType);
}
