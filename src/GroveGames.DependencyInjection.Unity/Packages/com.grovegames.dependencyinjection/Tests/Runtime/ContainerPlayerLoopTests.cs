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
        public IEnumerator InitializedChildOfRoot_UpdatesEveryPhase()
        {
            yield return WaitForRoot();
            var child = CreateInitializedChild(builder => builder.AddSingleton<TestUpdatable>());
            var updatable = child.Resolve<TestUpdatable>();

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
                child.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator DisposedChildOfRoot_StopsUpdating()
        {
            yield return WaitForRoot();
            var child = CreateInitializedChild(builder => builder.AddSingleton<TestUpdatable>());
            var updatable = child.Resolve<TestUpdatable>();
            yield return null;

            child.Dispose();
            var updateCount = updatable.FrameUpdateCount;
            yield return null;
            yield return null;

            Assert.AreEqual(updateCount, updatable.FrameUpdateCount);
        }

        [UnityTest]
        public IEnumerator ChildOfRootNotInitialized_IsNotUpdated()
        {
            yield return WaitForRoot();
            var child = Root.CreateChild(builder => builder.AddSingleton<TestUpdatable>());
            var updatable = child.Resolve<TestUpdatable>();

            yield return null;
            yield return null;

            Assert.AreEqual(0, updatable.FrameUpdateCount);
            child.Dispose();
        }

        [UnityTest]
        public IEnumerator FrameUpdate_MultipleChildren_RunsEveryPreFrameUpdateBeforeAnyFrameUpdate()
        {
            yield return WaitForRoot();
            var log = new List<string>();
            var first = CreateInitializedChild(builder =>
            {
                builder.AddSingleton(log);
                builder.AddSingleton<TestLoggingFrameUpdatable>();
            });
            var second = CreateInitializedChild(builder =>
            {
                builder.AddSingleton(log);
                builder.AddSingleton<TestLoggingPreFrameUpdatable>();
            });

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
                first.Dispose();
                second.Dispose();
            }
        }

        private static IContainer Root => (IContainer)ContainerRegistry.Root!;

        private static IContainer CreateInitializedChild(System.Action<IContainerBuilder> configure)
        {
            var child = Root.CreateChild(configure);
            child.InitializeAsync().AsTask().GetAwaiter().GetResult();
            return child;
        }

        private static IEnumerator WaitForRoot()
        {
            for (var i = 0; i < 100 && ContainerRegistry.Root == null; i++)
            {
                yield return null;
            }

            Assert.IsNotNull(ContainerRegistry.Root);
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
            private int _frameUpdateCount;
            private int _physicsUpdateCount;
            private int _preFrameUpdateCount;
            private int _postFrameUpdateCount;

            public int FrameUpdateCount => _frameUpdateCount;
            public int PhysicsUpdateCount => _physicsUpdateCount;
            public int PreFrameUpdateCount => _preFrameUpdateCount;
            public int PostFrameUpdateCount => _postFrameUpdateCount;

            public void FrameUpdate(float deltaTime)
            {
                _frameUpdateCount++;
            }

            public void PhysicsUpdate(float deltaTime)
            {
                _physicsUpdateCount++;
            }

            public void PreFrameUpdate(float deltaTime)
            {
                _preFrameUpdateCount++;
            }

            public void PostFrameUpdate(float deltaTime)
            {
                _postFrameUpdateCount++;
            }
        }
    }
}
