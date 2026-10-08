using System.Collections;
using System.Collections.Generic;

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

                Assert.Greater(updatable.FrameUpdateCount, 0);
                Assert.Greater(updatable.PhysicsUpdateCount, 0);
                Assert.Greater(updatable.PreFrameUpdateCount, 0);
                Assert.Greater(updatable.PostFrameUpdateCount, 0);
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
            var updateCount = updatable.FrameUpdateCount;
            yield return null;
            yield return null;

            Assert.AreEqual(updateCount, updatable.FrameUpdateCount);
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

            Assert.AreEqual(0, updatable.FrameUpdateCount);
            ContainerPlayerLoop.Remove(container);
        }

        [UnityTest]
        public IEnumerator FrameUpdate_MultipleContainers_RunsEveryPreFrameUpdateBeforeAnyFrameUpdate()
        {
            var log = new List<string>();
            var first = new ContainerBuilder();
            first.AddSingleton(log);
            first.AddSingleton<TestLoggingFrameUpdatable>();
            var firstContainer = first.Build();
            var second = new ContainerBuilder();
            second.AddSingleton(log);
            second.AddSingleton<TestLoggingPreFrameUpdatable>();
            var secondContainer = second.Build();
            ContainerPlayerLoop.Add(firstContainer);
            ContainerPlayerLoop.Add(secondContainer);

            try
            {
                yield return null;
                yield return null;

                var preFrameUpdate = log.IndexOf("PreFrameUpdate");
                var frameUpdate = log.IndexOf("FrameUpdate");
                var postFrameUpdate = log.IndexOf("PostFrameUpdate");

                Assert.GreaterOrEqual(preFrameUpdate, 0);
                Assert.Less(preFrameUpdate, frameUpdate);
                Assert.Less(frameUpdate, postFrameUpdate);
            }
            finally
            {
                ContainerPlayerLoop.Remove(firstContainer);
                ContainerPlayerLoop.Remove(secondContainer);
                firstContainer.Dispose();
                secondContainer.Dispose();
            }
        }

        private sealed class TestLoggingFrameUpdatable : IFrameUpdatable
        {
            private readonly List<string> _log;

            public TestLoggingFrameUpdatable(List<string> log)
            {
                _log = log;
            }

            public void FrameUpdate(float deltaTime)
            {
                _log.Add("FrameUpdate");
            }
        }

        private sealed class TestLoggingPreFrameUpdatable : IPreFrameUpdatable, IPostFrameUpdatable
        {
            private readonly List<string> _log;

            public TestLoggingPreFrameUpdatable(List<string> log)
            {
                _log = log;
            }

            public void PreFrameUpdate(float deltaTime)
            {
                _log.Add("PreFrameUpdate");
            }

            public void PostFrameUpdate(float deltaTime)
            {
                _log.Add("PostFrameUpdate");
            }
        }

        private sealed class TestUpdatable : IPreFrameUpdatable, IFrameUpdatable, IPostFrameUpdatable, IPhysicsUpdatable
        {
            public int FrameUpdateCount { get; private set; }
            public int PhysicsUpdateCount { get; private set; }
            public int PreFrameUpdateCount { get; private set; }
            public int PostFrameUpdateCount { get; private set; }

            public void FrameUpdate(float deltaTime)
            {
                FrameUpdateCount++;
            }

            public void PhysicsUpdate(float deltaTime)
            {
                PhysicsUpdateCount++;
            }

            public void PreFrameUpdate(float deltaTime)
            {
                PreFrameUpdateCount++;
            }

            public void PostFrameUpdate(float deltaTime)
            {
                PostFrameUpdateCount++;
            }
        }
    }
}
