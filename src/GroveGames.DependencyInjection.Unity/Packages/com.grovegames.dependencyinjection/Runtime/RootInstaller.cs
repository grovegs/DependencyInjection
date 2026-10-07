using UnityEngine;

namespace GroveGames.DependencyInjection.Unity
{
    public abstract class RootInstaller : ScriptableObject, IInstaller
    {
        public abstract void Install(IContainerBuilder builder);
    }
}
