using System;
using System.Collections;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity.Tests
{
    public sealed class ComponentRegistrationExtensionsTests
    {
        [Test]
        public void AddSingletonFromPrefab_Resolve_InstantiatesInjectedSingleton()
        {
            var prefab = new GameObject("Prefab").AddComponent<TestBehaviour>();
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            builder.AddSingletonFromPrefab(prefab);
            var container = builder.Build();

            try
            {
                var instance = container.Resolve<TestBehaviour>();

                Assert.AreNotSame(prefab, instance);
                Assert.AreSame(instance, container.Resolve<TestBehaviour>());
                Assert.AreSame(container.Resolve<TestService>(), instance.Service);
                Assert.AreEqual("DontDestroyOnLoad", instance.gameObject.scene.name);
            }
            finally
            {
                container.Dispose();
                Object.DestroyImmediate(prefab.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator AddSingletonFromPrefab_ContainerDisposed_DestroysInstanceAfterDisposingComponent()
        {
            var prefab = new GameObject("Prefab").AddComponent<TestBehaviour>();
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            builder.AddSingletonFromPrefab(prefab);
            var container = builder.Build();
            var instance = container.Resolve<TestBehaviour>();

            container.Dispose();

            Assert.IsTrue(instance.IsDisposed);
            Assert.IsTrue(instance != null);

            yield return null;

            Assert.IsTrue(instance == null);
            Object.Destroy(prefab.gameObject);
        }

        [UnityTest]
        public IEnumerator AddSingletonOnNewGameObject_Resolve_CreatesNamedInjectedComponentOwnedByContainer()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            builder.AddSingletonOnNewGameObject<TestBehaviour>("Created");
            var container = builder.Build();
            var instance = container.Resolve<TestBehaviour>();

            Assert.AreEqual("Created", instance.gameObject.name);
            Assert.AreEqual("DontDestroyOnLoad", instance.gameObject.scene.name);
            Assert.AreSame(container.Resolve<TestService>(), instance.Service);

            container.Dispose();
            yield return null;

            Assert.IsTrue(instance == null);
        }

        private sealed class TestService
        {
        }

        private sealed class TestBehaviour : MonoBehaviour, IDisposable
        {
            public TestService? Service { get; private set; }
            public bool IsDisposed { get; private set; }

            [Inject]
            public void Construct(TestService service)
            {
                Service = service;
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
