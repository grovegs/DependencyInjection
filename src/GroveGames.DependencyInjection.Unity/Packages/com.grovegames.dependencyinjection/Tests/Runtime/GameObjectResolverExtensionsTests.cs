using System;
using System.Collections;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using Object = UnityEngine.Object;

namespace GroveGames.DependencyInjection.Unity.Tests
{
    public sealed class GameObjectResolverExtensionsTests
    {
        [Test]
        public void InjectGameObject_Hierarchy_InjectsEveryBehaviour()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            using var container = builder.Build();
            var parent = new GameObject("Parent");
            var child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            child.SetActive(false);
            var parentBehaviour = parent.AddComponent<TestBehaviour>();
            var childBehaviour = child.AddComponent<TestBehaviour>();

            try
            {
                container.InjectGameObject(parent);

                Assert.AreSame(container.Resolve<TestService>(), parentBehaviour.Service);
                Assert.AreSame(container.Resolve<TestService>(), childBehaviour.Service);
            }
            finally
            {
                Object.DestroyImmediate(parent);
            }
        }

        [UnityTest]
        public IEnumerator Instantiate_Prefab_ReturnsInjectedCloneOwnedByContainer()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            var container = builder.Build();
            var prefab = new GameObject("Prefab").AddComponent<TestBehaviour>();

            var instance = container.Instantiate(prefab);

            Assert.AreNotSame(prefab, instance);
            Assert.AreSame(container.Resolve<TestService>(), instance.Service);
            Assert.IsNull(prefab.Service);
            Assert.AreEqual("DontDestroyOnLoad", instance.gameObject.scene.name);

            container.Dispose();
            yield return null;

            Assert.IsTrue(instance == null);
            Object.Destroy(prefab.gameObject);
        }

        [UnityTest]
        public IEnumerator Instantiate_Component_CreatesNamedGameObjectOwnedByContainer()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            var container = builder.Build();

            var instance = container.Instantiate<TestBehaviour>();

            Assert.AreEqual(nameof(TestBehaviour), instance.gameObject.name);
            Assert.AreEqual("DontDestroyOnLoad", instance.gameObject.scene.name);
            Assert.AreSame(container.Resolve<TestService>(), instance.Service);

            container.Dispose();
            yield return null;

            Assert.IsTrue(instance == null);
        }

        [UnityTest]
        public IEnumerator Instantiate_WithParent_KeepsParentAndIsOwnedByContainer()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            var container = builder.Build();
            var prefab = new GameObject("Prefab").AddComponent<TestBehaviour>();
            var parent = new GameObject("Parent").transform;

            var instance = container.Instantiate(prefab, parent);

            Assert.AreSame(parent, instance.transform.parent);
            Assert.AreSame(container.Resolve<TestService>(), instance.Service);

            container.Dispose();
            yield return null;

            Assert.IsTrue(instance == null);
            Assert.IsTrue(parent != null);
            Object.Destroy(prefab.gameObject);
            Object.Destroy(parent.gameObject);
        }

        [UnityTest]
        public IEnumerator AddSingleton_InstantiateFactory_DisposesComponentBeforeDestroyingGameObject()
        {
            var prefab = new GameObject("Prefab").AddComponent<TestBehaviour>();
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            builder.AddSingleton(resolver => resolver.Instantiate(prefab));
            var container = builder.Build();
            var instance = container.Resolve<TestBehaviour>();

            Assert.AreSame(instance, container.Resolve<TestBehaviour>());

            container.Dispose();

            Assert.IsTrue(instance.IsDisposed);
            Assert.IsTrue(instance != null);

            yield return null;

            Assert.IsTrue(instance == null);
            Object.Destroy(prefab.gameObject);
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
