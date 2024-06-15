using System;

namespace DependencyInjection.Resolution
{
    internal interface IContainerResolver : IRegistrationResolver
    {
        void AddInstanceResolver(Type registrationType, IInstanceResolver instanceResolver);
        void Clear();
    }
}
