using GroveGames.DependencyInjection;

using UnityEngine;

namespace Sandbox
{
    public sealed class SceneEntryPoint : IPostInitializable
    {
        private readonly Greeter _greeter;

        public SceneEntryPoint(Greeter greeter)
        {
            _greeter = greeter;
        }

        public void PostInitialize()
        {
            Debug.Log(_greeter.Greet("scene"));
        }
    }
}
