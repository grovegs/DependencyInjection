using GroveGames.DependencyInjection;
using GroveGames.DependencyInjection.Unity;

using UnityEngine;

namespace Sandbox
{
    [CreateAssetMenu(menuName = "Sandbox/Root Installer")]
    public sealed class SandboxRootInstaller : RootInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.AddSingleton<Greeter>();
            builder.AddSingleton<RootEntryPoint>();
        }
    }
}
