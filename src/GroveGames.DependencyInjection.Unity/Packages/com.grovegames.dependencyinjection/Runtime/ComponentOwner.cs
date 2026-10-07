using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    internal sealed class ComponentOwner<T> : IDisposable
        where T : Component
    {
        private readonly T _component;
        private readonly GameObject _gameObject;

        public T Component => _component;

        public ComponentOwner(T component)
        {
            _component = component;
            _gameObject = component.gameObject;
            Object.DontDestroyOnLoad(_gameObject);
        }

        public void Dispose()
        {
            if (_gameObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(_gameObject);
            }
            else
            {
                Object.DestroyImmediate(_gameObject);
            }
        }
    }
}
