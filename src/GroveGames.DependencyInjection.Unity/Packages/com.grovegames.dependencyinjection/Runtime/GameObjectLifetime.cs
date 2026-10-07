using System;

using UnityEngine;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    internal sealed class GameObjectLifetime : IDisposable
    {
        private readonly GameObject _gameObject;

        public GameObjectLifetime(GameObject gameObject)
        {
            _gameObject = gameObject;
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
