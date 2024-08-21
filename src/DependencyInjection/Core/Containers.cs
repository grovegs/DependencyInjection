namespace DependencyInjection.Core;

public static class Containers
{
    public static readonly IContainer Root = new RootContainer();
    public static IContainer? Application { get; set; }
    public static IContainer? Scene { get; set; }
}
