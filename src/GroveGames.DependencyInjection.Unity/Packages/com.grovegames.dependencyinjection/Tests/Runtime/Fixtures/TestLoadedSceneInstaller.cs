namespace GroveGames.DependencyInjection.Unity.Tests
{
    internal sealed class TestLoadedSceneInstaller : SceneInstaller
    {
        public override void Install(IContainerBuilder builder)
        {
            builder.AddSingleton<TestLoadedSceneService>();
        }
    }

    internal sealed class TestLoadedSceneService
    {
    }
}
