using GroveGames.DependencyInjection;

var rootContainer = RootContainerFactory.CreateRootContainer(builder =>
{
    builder.AddSingleton<IGreeter, ConsoleGreeter>();
    builder.AddSingleton<Application>();
});

var app = rootContainer.Resolve<Application>();
app.Run();

rootContainer.Dispose();

public interface IGreeter
{
    void Greet(string name);
}

public sealed class ConsoleGreeter : IGreeter
{
    public void Greet(string name)
    {
        Console.WriteLine($"Hello, {name}!");
    }
}

public sealed class Application
{
    private readonly IGreeter _greeter;

    public Application(IGreeter greeter)
    {
        _greeter = greeter;
    }

    public void Run()
    {
        _greeter.Greet("World");
    }
}
