namespace DependencyInjection.Tests.Fakes;

internal sealed class InitializableClass : IInitializableClass
{
    public bool Initialized { get; private set; }

    public InitializableClass()
    {
    }

    public void Initialize()
    {
        Initialized = true;
    }
}
