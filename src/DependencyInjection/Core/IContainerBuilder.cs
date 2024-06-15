namespace DependencyInjection.Core
{
    public interface IContainerBuilder : IContainerConfigurer
    {
        IContainer Build();
    }
}
