using System;
using System.Collections.Generic;
using DependencyInjection.Resolution;

namespace DependencyInjection.Core
{
    internal sealed class InitializableCollection : IInitializableCollection
    {
        private static readonly Type s_initializableType = typeof(IInitializable);

        private readonly IList<Type> _initializableRegistrationTypes;
        private readonly IRegistrationResolver _registrationResolver;

        public InitializableCollection(IRegistrationResolver registrationResolver)
        {
            _initializableRegistrationTypes = new List<Type>();
            _registrationResolver = registrationResolver;
        }

        public void TryAdd(Type registrationType, Type implementationType)
        {
            // TODO: Very costly operation. Find better way.
            if (s_initializableType.IsAssignableFrom(implementationType))
            {
                _initializableRegistrationTypes.Add(registrationType);
            }
        }

        public void Initialize()
        {
            foreach (var initializableRegistrationType in _initializableRegistrationTypes)
            {
                var initializable = (IInitializable)_registrationResolver.Resolve(initializableRegistrationType);
                initializable.Initialize();
            }
        }
    }
}
