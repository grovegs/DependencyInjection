using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Unity;

namespace Sandbox
{
    public sealed class SandboxSceneInstaller : SceneInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.AddSingleton<SceneEntryPoint>();
        }
    }
}
