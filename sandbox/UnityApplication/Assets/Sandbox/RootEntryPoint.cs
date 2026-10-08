using System.Threading;
using System.Threading.Tasks;

using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Unity;

using UnityEngine;

namespace Sandbox
{
    public sealed class RootEntryPoint : IAsyncInitializable, IInitializable, IFrameUpdatable
    {
        private readonly Greeter _greeter;
        private int _frameCount;

        public RootEntryPoint(Greeter greeter)
        {
            _greeter = greeter;
        }

        public async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            Debug.Log("Root initialized asynchronously.");
        }

        public void Initialize()
        {
            Debug.Log(_greeter.Greet("root"));
        }

        public void FrameUpdate(float deltaTime)
        {
            if (++_frameCount == 10)
            {
                Debug.Log("Root updated 10 frames.");
            }
        }
    }
}
