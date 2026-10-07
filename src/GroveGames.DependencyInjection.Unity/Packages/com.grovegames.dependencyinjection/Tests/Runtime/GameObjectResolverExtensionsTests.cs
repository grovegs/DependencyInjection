using NUnit.Framework;

using UnityEngine;

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

        [Test]
        public void Instantiate_Prefab_ReturnsInjectedClone()
        {
            var builder = new ContainerBuilder();
            builder.AddSingleton<TestService>();
            using var container = builder.Build();
            var prefab = new GameObject("Prefab").AddComponent<TestBehaviour>();
            TestBehaviour? instance = null;

            try
            {
                instance = container.Instantiate(prefab);

                Assert.AreNotSame(prefab, instance);
                Assert.AreSame(container.Resolve<TestService>(), instance.Service);
                Assert.IsNull(prefab.Service);
            }
            finally
            {
                Object.DestroyImmediate(prefab.gameObject);

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
