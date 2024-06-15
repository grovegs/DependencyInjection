namespace DependencyInjection.Resolution
{
    internal sealed class InstanceResolver : IInstanceResolver
    {
        private readonly object _implementationInstance;

        public InstanceResolver(object implementationInstance)
        {
            _implementationInstance = implementationInstance;
        }

        public object Resolve()
        {
            return _implementationInstance;
        }
    }
}
