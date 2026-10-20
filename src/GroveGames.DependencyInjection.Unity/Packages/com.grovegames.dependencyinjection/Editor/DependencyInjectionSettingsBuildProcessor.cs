using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace GroveGames.DependencyInjection.Unity.Editor
{
    internal sealed class DependencyInjectionSettingsBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            DependencyInjectionSettingsAsset.GetOrCreate();
        }
    }
}
