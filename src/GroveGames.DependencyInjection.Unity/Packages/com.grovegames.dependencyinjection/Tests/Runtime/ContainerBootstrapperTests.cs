using System.Collections;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GroveGames.DependencyInjection.Unity.Tests
{
    public sealed class ContainerBootstrapperTests
    {
        [UnityTest]
        public IEnumerator Root_PlayMode_IsBuiltAndInitialized()
        {
            var root = ContainerBootstrapper.Root;

            Assert.IsNotNull(root);

            for (var i = 0; i < 100 && !root!.IsInitialized; i++)
            {
                yield return null;
            }

            Assert.IsTrue(root!.IsInitialized);
        }

        [UnityTest]
        public IEnumerator OnSceneLoaded_SceneWithInstaller_CreatesAndDisposesSceneContainer()
        {
            var scene = SceneManager.CreateScene(nameof(OnSceneLoaded_SceneWithInstaller_CreatesAndDisposesSceneContainer));
            var gameObject = new GameObject(nameof(TestSceneInstaller));
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            var installer = gameObject.AddComponent<TestSceneInstaller>();

            ContainerBootstrapper.OnSceneLoaded(scene, LoadSceneMode.Additive);

            IContainer? container = null;

            for (var i = 0; i < 100 && (container == null || !container.IsInitialized); i++)
            {
                ContainerBootstrapper.TryGetSceneContainer(scene, out container);
                yield return null;
            }

            Assert.IsNotNull(container);
            Assert.IsTrue(container!.IsInitialized);
            Assert.IsTrue(installer.IsInstalled);
            Assert.AreSame(ContainerBootstrapper.Root, container.Parent);
            Assert.IsNotNull(container.Resolve<TestService>());

            yield return SceneManager.UnloadSceneAsync(scene);

            Assert.IsTrue(container.IsDisposed);
            Assert.IsFalse(ContainerBootstrapper.TryGetSceneContainer(scene, out _));
        }

        [Test]
        public void OnSceneLoaded_SceneWithoutInstaller_CreatesNoContainer()
        {
            var scene = SceneManager.CreateScene(nameof(OnSceneLoaded_SceneWithoutInstaller_CreatesNoContainer));

            ContainerBootstrapper.OnSceneLoaded(scene, LoadSceneMode.Additive);

            Assert.IsFalse(ContainerBootstrapper.TryGetSceneContainer(scene, out _));
            SceneManager.UnloadSceneAsync(scene);
        }

        private sealed class TestSceneInstaller : SceneInstaller
        {
            public bool IsInstalled { get; private set; }

            public override void Install(IContainerBuilder builder)
            {
                builder.AddSingleton<TestService>();
                IsInstalled = true;
            }
        }

        private sealed class TestService
        {
        }
    }
}
