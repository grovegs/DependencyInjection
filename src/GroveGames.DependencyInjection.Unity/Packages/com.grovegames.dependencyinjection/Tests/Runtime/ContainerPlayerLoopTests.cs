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

                Assert.Greater(updatable.UpdateCount, 0);
                Assert.Greater(updatable.FixedUpdateCount, 0);
                Assert.Greater(updatable.PreUpdateCount, 0);
                Assert.Greater(updatable.PostUpdateCount, 0);
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

        [UnityTest]
        public IEnumerator Update_MultipleContainers_RunsEveryPreUpdateBeforeAnyUpdate()
        {
            var log = new List<string>();
            var first = new ContainerBuilder();
            first.AddSingleton(log);
            first.AddSingleton<TestLoggingUpdatable>();
            var firstContainer = first.Build();
            var second = new ContainerBuilder();
            second.AddSingleton(log);
            second.AddSingleton<TestLoggingPreUpdatable>();
            var secondContainer = second.Build();
            ContainerPlayerLoop.Add(firstContainer);
            ContainerPlayerLoop.Add(secondContainer);

            try
            {
                yield return null;
                yield return null;

                var preUpdate = log.IndexOf("PreUpdate");
                var update = log.IndexOf("Update");
                var postUpdate = log.IndexOf("PostUpdate");

                Assert.GreaterOrEqual(preUpdate, 0);
                Assert.Less(preUpdate, update);
                Assert.Less(update, postUpdate);
            }
            finally
            {
                ContainerPlayerLoop.Remove(firstContainer);
                ContainerPlayerLoop.Remove(secondContainer);
                firstContainer.Dispose();
                secondContainer.Dispose();
            }
        }

        private sealed class TestLoggingUpdatable : IUpdatable
        {
            private readonly List<string> _log;

            public TestLoggingUpdatable(List<string> log)
            {
                _log = log;
            }

            public void Update(float deltaTime)
            {
                _log.Add("Update");
            }
        }

        private sealed class TestLoggingPreUpdatable : IPreUpdatable, IPostUpdatable
        {
            private readonly List<string> _log;

            public TestLoggingPreUpdatable(List<string> log)
            {
                _log = log;
            }

            public void PreUpdate(float deltaTime)
            {
                _log.Add("PreUpdate");
            }

            public void PostUpdate(float deltaTime)
            {
                _log.Add("PostUpdate");
            }
        }

        private sealed class TestUpdatable : IPreUpdatable, IUpdatable, IPostUpdatable, IFixedUpdatable
        {
            public int UpdateCount { get; private set; }
            public int FixedUpdateCount { get; private set; }
            public int PreUpdateCount { get; private set; }
            public int PostUpdateCount { get; private set; }

            public void Update(float deltaTime)
            {
                UpdateCount++;
            }

            public void FixedUpdate(float deltaTime)
            {
                FixedUpdateCount++;
            }

            public void PreUpdate(float deltaTime)
            {
                PreUpdateCount++;
            }

            public void PostUpdate(float deltaTime)
            {
                PostUpdateCount++;
            }
        }
    }
}
