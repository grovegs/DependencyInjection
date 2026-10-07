using System.Collections;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace GroveGames.DependencyInjection.Unity.Tests
{
    public sealed class ContainerPlayerLoopTests
    {
        [UnityTest]
        public IEnumerator Add_InitializedContainer_UpdatesEveryPhase()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestUpdatable>();
            var container = builder.Build();
            var updatable = container.Resolve<TestUpdatable>();
            ContainerPlayerLoop.Add(container);

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
                ContainerPlayerLoop.Remove(container);
                container.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator Remove_AddedContainer_StopsUpdating()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestUpdatable>();
            var container = builder.Build();
            var updatable = container.Resolve<TestUpdatable>();
            ContainerPlayerLoop.Add(container);
            yield return null;

            ContainerPlayerLoop.Remove(container);
            var updateCount = updatable.UpdateCount;
            yield return null;
            yield return null;

            Assert.AreEqual(updateCount, updatable.UpdateCount);
            container.Dispose();
        }

        [UnityTest]
        public IEnumerator Add_DisposedContainer_IsIgnored()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestUpdatable>();
            var container = builder.Build();
            var updatable = container.Resolve<TestUpdatable>();
            container.Dispose();

            ContainerPlayerLoop.Add(container);
            yield return null;

            Assert.AreEqual(0, updatable.UpdateCount);
            ContainerPlayerLoop.Remove(container);
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
