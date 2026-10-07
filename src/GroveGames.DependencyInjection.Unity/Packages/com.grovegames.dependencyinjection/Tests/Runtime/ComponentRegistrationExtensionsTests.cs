using NUnit.Framework;

using UnityEngine;

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
            TestBehaviour? instance = null;

            try
            {
                instance = container.Resolve<TestBehaviour>();

                Assert.AreNotSame(prefab, instance);
                Assert.AreSame(instance, container.Resolve<TestBehaviour>());
                Assert.AreSame(container.Resolve<TestService>(), instance.Service);
            }
            finally
            {
                container.Dispose();
                Object.DestroyImmediate(prefab.gameObject);

                if (instance != null)
                {
                    Object.DestroyImmediate(instance.gameObject);
                }
            }
        }

        [Test]
        public void AddSingletonOnNewGameObject_Resolve_CreatesNamedInjectedComponent()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            builder.AddSingletonOnNewGameObject<TestBehaviour>("Created");
            var container = builder.Build();
            TestBehaviour? instance = null;

            try
            {
                instance = container.Resolve<TestBehaviour>();

                Assert.AreEqual("Created", instance.gameObject.name);
                Assert.AreSame(container.Resolve<TestService>(), instance.Service);
            }
            finally
            {
                container.Dispose();

                if (instance != null)
                {
                    Object.DestroyImmediate(instance.gameObject);
                }
            }
        }

        private sealed class TestService
        {
        }

        private sealed class TestBehaviour : MonoBehaviour
        {
            public TestService? Service { get; private set; }

            [Inject]
            public void Construct(TestService service)
            {
                Service = service;
            }
        }
    }
}
