namespace GroveGames.DependencyInjection.Collections;

internal interface IDisposableCollection : IDisposable, IEnumerable<IDisposable>
{
    void TryAdd(object disposableObject);
}