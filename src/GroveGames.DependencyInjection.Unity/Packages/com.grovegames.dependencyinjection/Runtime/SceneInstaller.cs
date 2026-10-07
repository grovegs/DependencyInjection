using UnityEngine;

namespace GroveGames.DependencyInjection.Unity
{
    public abstract class SceneInstaller : MonoBehaviour, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
