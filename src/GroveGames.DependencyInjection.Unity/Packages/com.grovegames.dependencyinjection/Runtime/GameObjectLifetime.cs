using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity
{
    internal sealed class GameObjectLifetime : IDisposable
    {
        private const int MinimumPruneCount = 16;

        private static readonly ConditionalWeakTable<IObjectResolver, GameObjectLifetime> s_lifetimes = new();

        private readonly IObjectResolver _resolver;
        private readonly List<GameObject> _gameObjects;
        private int _pruneCount;

        private GameObjectLifetime(IObjectResolver resolver)
        {
            _resolver = resolver;
            _gameObjects = new List<GameObject>();
            _pruneCount = MinimumPruneCount;
        }

        internal int Count => _gameObjects.Count;

        public static void Track(IObjectResolver resolver, GameObject gameObject)
        {
            if (!s_lifetimes.TryGetValue(resolver, out var lifetime))
            {
                lifetime = new GameObjectLifetime(resolver);
                resolver.AddDisposable(lifetime);
                s_lifetimes.Add(resolver, lifetime);
            }

            lifetime.Add(gameObject);
        }

        internal static bool TryGet(IObjectResolver resolver, out GameObjectLifetime? lifetime)
        {
            return s_lifetimes.TryGetValue(resolver, out lifetime);
        }

        public void Dispose()
        {
            s_lifetimes.Remove(_resolver);

            for (var i = _gameObjects.Count - 1; i >= 0; i--)
            {
                var gameObject = _gameObjects[i];

                if (gameObject == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Object.Destroy(gameObject);
                }
                else
                {
                    Object.DestroyImmediate(gameObject);
                }
            }

            _gameObjects.Clear();
        }

        private void Add(GameObject gameObject)
        {
            if (_gameObjects.Count >= _pruneCount)
            {
                _gameObjects.RemoveAll(static item => item == null);
                _pruneCount = Math.Max(MinimumPruneCount, _gameObjects.Count * 2);
            }

            _gameObjects.Add(gameObject);
        }
    }
}
