using System.Collections;
using System.Threading;
using System.Threading.Tasks;

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
        public IEnumerator SceneInstaller_Awake_CreatesAndDisposesSceneContainer()
        {
            var scene = SceneManager.CreateScene(nameof(SceneInstaller_Awake_CreatesAndDisposesSceneContainer));
            var installer = AddInstaller<TestSceneInstaller>(scene);
            var ready = ContainerBootstrapper.WhenSceneReadyAsync(scene);

            yield return WaitFor(ready);

            Assert.IsTrue(ready.Result);
            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var container));
            Assert.IsTrue(container!.IsInitialized);
            Assert.IsTrue(installer.IsInstalled);
            Assert.AreSame(ContainerBootstrapper.Root, container.Parent);
            Assert.IsNotNull(container.Resolve<TestService>());

            yield return SceneManager.UnloadSceneAsync(scene);

            Assert.IsTrue(container.IsDisposed);
            Assert.IsFalse(ContainerBootstrapper.TryGetContainer(scene, out _));
        }

        [UnityTest]
        public IEnumerator SceneInstaller_Destroyed_DisposesSceneContainerBeforeSceneUnloads()
        {
            var scene = SceneManager.CreateScene(nameof(SceneInstaller_Destroyed_DisposesSceneContainerBeforeSceneUnloads));
            var installer = AddInstaller<TestSceneInstaller>(scene);
            var ready = ContainerBootstrapper.WhenSceneReadyAsync(scene);

            yield return WaitFor(ready);

            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var container));

            Object.Destroy(installer.gameObject);
            yield return null;

            Assert.IsTrue(container!.IsDisposed);
            Assert.IsTrue(scene.isLoaded);
            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var fallback));
            Assert.AreSame(ContainerBootstrapper.Root, fallback);

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator SceneInstaller_Awake_PausesSceneUntilAllInitializersComplete()
        {
            var scene = SceneManager.CreateScene(nameof(SceneInstaller_Awake_PausesSceneUntilAllInitializersComplete));
            var content = new GameObject("Content");
            SceneManager.MoveGameObjectToScene(content, scene);
            var behaviour = content.AddComponent<TestBehaviour>();
            var gate = new TestGate();
            var installer = AddInstaller<TestGatedSceneInstaller>(scene, gatedInstaller => gatedInstaller.Gate = gate);
            var ready = ContainerBootstrapper.WhenSceneReadyAsync(scene);

            yield return null;

            Assert.IsFalse(content.activeSelf);
            Assert.IsFalse(ready.IsCompleted);
            Assert.IsFalse(ContainerBootstrapper.TryGetContainer(scene, out _));
            Assert.IsNotNull(behaviour.Service);

            gate.Release();
            yield return WaitFor(ready);

            Assert.IsTrue(ready.Result);
            Assert.IsTrue(content.activeSelf);
            Assert.IsTrue(installer.gameObject.activeSelf);
            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out _));

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator SceneInstaller_UnloadedWhilePaused_CompletesNotReady()
        {
            var scene = SceneManager.CreateScene(nameof(SceneInstaller_UnloadedWhilePaused_CompletesNotReady));
            AddInstaller<TestGatedSceneInstaller>(scene, gatedInstaller => gatedInstaller.Gate = new TestGate());
            var ready = ContainerBootstrapper.WhenSceneReadyAsync(scene);

            yield return null;
            yield return SceneManager.UnloadSceneAsync(scene);

            Assert.IsTrue(ready.IsCompleted);
            Assert.IsFalse(ready.Result);
        }

        [UnityTest]
        public IEnumerator SceneInstaller_SceneActivatedSameFrame_InitializesAsActiveScene()
        {
            var previousActiveScene = SceneManager.GetActiveScene();
            var scene = SceneManager.CreateScene(nameof(SceneInstaller_SceneActivatedSameFrame_InitializesAsActiveScene));
            AddInstaller<TestNewObjectSceneInstaller>(scene);

            Assert.IsFalse(ContainerBootstrapper.TryGetContainer(scene, out _));

            SceneManager.SetActiveScene(scene);
            var ready = ContainerBootstrapper.WhenSceneReadyAsync(scene);

            yield return WaitFor(ready);

            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var container));
            Assert.AreEqual(scene, container!.Resolve<TestNewObjectCreator>().CreatedScene);

            SceneManager.SetActiveScene(previousActiveScene);
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator SceneInstaller_SceneNeverActivated_InitializesOnNextFrame()
        {
            var scene = SceneManager.CreateScene(nameof(SceneInstaller_SceneNeverActivated_InitializesOnNextFrame));
            AddInstaller<TestSceneInstaller>(scene);

            Assert.IsFalse(ContainerBootstrapper.TryGetContainer(scene, out _));

            for (var i = 0; i < 10 && !ContainerBootstrapper.TryGetContainer(scene, out _); i++)
            {
                yield return null;
            }

            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var container));
            Assert.AreNotSame(ContainerBootstrapper.Root, container);

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator SceneInstaller_ActiveSceneAtAwake_InitializesImmediately()
        {
            var previousActiveScene = SceneManager.GetActiveScene();
            var scene = SceneManager.CreateScene(nameof(SceneInstaller_ActiveSceneAtAwake_InitializesImmediately));
            SceneManager.SetActiveScene(scene);

            AddInstaller<TestSceneInstaller>(scene);

            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var container));
            Assert.AreNotSame(ContainerBootstrapper.Root, container);

            SceneManager.SetActiveScene(previousActiveScene);
            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator Instantiate_FromSceneContainer_PlacesObjectInContainerScene()
        {
            var scene = SceneManager.CreateScene(nameof(Instantiate_FromSceneContainer_PlacesObjectInContainerScene));
            AddInstaller<TestCreatingSceneInstaller>(scene);
            var ready = ContainerBootstrapper.WhenSceneReadyAsync(scene);

            yield return WaitFor(ready);

            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var container));
            Assert.AreNotEqual(scene, SceneManager.GetActiveScene());
            Assert.AreEqual(scene, container!.Resolve<TestObjectCreator>().CreatedScene);

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [Test]
        public void WhenSceneReadyAsync_SceneWithoutInstaller_ReturnsLoaded()
        {
            var scene = SceneManager.CreateScene(nameof(WhenSceneReadyAsync_SceneWithoutInstaller_ReturnsLoaded));

            var ready = ContainerBootstrapper.WhenSceneReadyAsync(scene);

            Assert.IsTrue(ready.IsCompleted);
            Assert.IsTrue(ready.Result);
            SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTest]
        public IEnumerator TryGetContainer_SceneWithoutInstaller_ReturnsInitializedRoot()
        {
            var root = ContainerBootstrapper.Root;

            for (var i = 0; i < 100 && root != null && !root.IsInitialized; i++)
            {
                yield return null;
            }

            var scene = SceneManager.CreateScene(nameof(TryGetContainer_SceneWithoutInstaller_ReturnsInitializedRoot));

            Assert.IsTrue(ContainerBootstrapper.TryGetContainer(scene, out var container));
            Assert.AreSame(root, container);

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        private static T AddInstaller<T>(Scene scene, System.Action<T>? configure = null)
            where T : SceneInstaller
        {
            var gameObject = new GameObject(typeof(T).Name);
            gameObject.SetActive(false);
            SceneManager.MoveGameObjectToScene(gameObject, scene);
            var installer = gameObject.AddComponent<T>();
            configure?.Invoke(installer);
            gameObject.SetActive(true);
            return installer;
        }

        private static IEnumerator WaitFor(Task task)
        {
            for (var i = 0; i < 100 && !task.IsCompleted; i++)
            {
                yield return null;
            }

            Assert.IsTrue(task.IsCompleted);
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

        private sealed class TestGatedSceneInstaller : SceneInstaller
        {
            public TestGate? Gate { get; set; }

            public override void Install(IContainerBuilder builder)
            {
                builder.AddSingleton<TestService>();
                builder.AddSingleton(Gate!);
                builder.AddSingleton<TestGatedEntryPoint>();
            }
        }

        private sealed class TestNewObjectSceneInstaller : SceneInstaller
        {
            public override void Install(IContainerBuilder builder)
            {
                builder.AddSingleton<TestNewObjectCreator>();
            }
        }

        private sealed class TestNewObjectCreator : IInitializable
        {
            public Scene CreatedScene { get; private set; }

            public void Initialize()
            {
                CreatedScene = new GameObject(nameof(TestNewObjectCreator)).scene;
            }
        }

        private sealed class TestCreatingSceneInstaller : SceneInstaller
        {
            public override void Install(IContainerBuilder builder)
            {
                builder.AddSingleton<TestObjectCreator>();
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

        private sealed class TestMarker : MonoBehaviour
        {
        }

        private sealed class TestGate
        {
            private readonly TaskCompletionSource<bool> _source;

            public TestGate()
            {
                _source = new TaskCompletionSource<bool>();
            }

            public Task Task => _source.Task;

            public void Release()
            {
                _source.TrySetResult(true);
            }
        }

        private sealed class TestGatedEntryPoint : IAsyncInitializable
        {
            private readonly TestGate _gate;

            public TestGatedEntryPoint(TestGate gate)
            {
                _gate = gate;
            }

            public async ValueTask InitializeAsync(CancellationToken cancellationToken)
            {
                await Task.WhenAny(_gate.Task, Task.Delay(Timeout.Infinite, cancellationToken));
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        private sealed class TestObjectCreator : IInitializable
        {
            private readonly IObjectResolver _resolver;

            public TestObjectCreator(IObjectResolver resolver)
            {
                _resolver = resolver;
            }

            public Scene CreatedScene { get; private set; }

            public void Initialize()
            {
                CreatedScene = _resolver.Instantiate<TestMarker>().gameObject.scene;
            }
        }
    }
}
