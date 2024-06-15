using System;

namespace DependencyInjection.Core
{
    internal interface IInitializableCollection : IInitializable
    {
        void TryAdd(Type registrationType, Type implementationType);
    }
}
