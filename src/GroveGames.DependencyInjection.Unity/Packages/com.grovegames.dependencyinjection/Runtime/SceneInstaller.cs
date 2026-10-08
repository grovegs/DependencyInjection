using UnityEngine;

namespace GroveGames.DependencyInjection.Unity
{
    [DefaultExecutionOrder(-10000)]
    public abstract class SceneInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);

        protected virtual void Awake()
        {
            ContainerBootstrapper.InitializeScene(gameObject.scene);
        }

        protected virtual void OnDestroy()
        {
            ContainerBootstrapper.DisposeSceneContainer(gameObject.scene);
        }
    }
}
