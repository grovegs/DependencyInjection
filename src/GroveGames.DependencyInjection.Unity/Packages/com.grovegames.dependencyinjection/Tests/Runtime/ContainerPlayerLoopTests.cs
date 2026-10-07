using System.Collections;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace GroveGames.DependencyInjection.Unity.Tests
{
    public sealed class ContainerPlayerLoopTests
    {
        [UnityTest]
        public IEnumerator Register_InitializedContainer_UpdatesEveryPhase()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestUpdatable>();
            var container = builder.Build();
            var initialization = container.InitializeAsync().AsTask();

            while (!initialization.IsCompleted)
            {
                yield return null;
            }

            var updatable = container.Resolve<TestUpdatable>();
            ContainerPlayerLoop.Register(container);

            try
            {
                yield return null;
                yield return new WaitForFixedUpdate();
                yield return null;

                Assert.Greater(updatable.UpdateCount, 0);
                Assert.Greater(updatable.FixedUpdateCount, 0);
                Assert.Greater(updatable.LateUpdateCount, 0);
            }
            finally
            {
                RestoreRoot();
                container.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator Unregister_RegisteredContainer_StopsUpdating()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestUpdatable>();
            var container = builder.Build();
            var initialization = container.InitializeAsync().AsTask();

            while (!initialization.IsCompleted)
            {
                yield return null;
            }

            var updatable = container.Resolve<TestUpdatable>();
            ContainerPlayerLoop.Register(container);
            yield return null;
            ContainerPlayerLoop.Unregister();
            var updateCount = updatable.UpdateCount;

            try
            {
                yield return null;
                yield return null;

                Assert.AreEqual(updateCount, updatable.UpdateCount);
            }
            finally
            {
                RestoreRoot();
                container.Dispose();
            }
        }

        private static void RestoreRoot()
        {
            var root = ContainerBootstrapper.Root;

            if (root != null)
            {
                ContainerPlayerLoop.Register(root);
            }
            else
            {
                ContainerPlayerLoop.Unregister();
            }
        }

        private sealed class TestUpdatable : IUpdatable, IFixedUpdatable, ILateUpdatable
        {
            public int UpdateCount { get; private set; }
            public int FixedUpdateCount { get; private set; }
            public int LateUpdateCount { get; private set; }

            public void Update(float deltaTime)
            {
                UpdateCount++;
            }

            public void FixedUpdate(float deltaTime)
            {
                FixedUpdateCount++;
            }

            public void LateUpdate(float deltaTime)
            {
                LateUpdateCount++;
            }
        }
    }
}
